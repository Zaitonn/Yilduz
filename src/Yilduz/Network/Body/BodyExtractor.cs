using System;
using Jint;
using Jint.Native;
using Yilduz.Extensions;
using Yilduz.Files.Blob;
using Yilduz.Network.FormData;
using Yilduz.Streams.ReadableStream;
using Yilduz.URLs.URLSearchParams;
using Yilduz.Utils;

namespace Yilduz.Network.Body;

internal static class BodyExtractor
{
    internal static BodyWithType Extract(Engine engine, JsValue obj, bool keepalive = false)
    {
        // Step 1: Let stream be null.
        ReadableStreamInstance? stream = null;

        // Step 2: If object is a ReadableStream object, then set stream to object.
        if (obj is ReadableStreamInstance readableStream)
        {
            stream = readableStream;
        }
        // Step 3: Otherwise, if object is a Blob object, set stream to the result of running object’s get stream.
        else if (obj is BlobInstance blob)
        {
            stream = blob.Stream();
        }
        else
        {
            // Step 4: Otherwise, set stream to a new ReadableStream object, and set up stream with byte reading support.
            stream = engine
                .GetWebApiIntrinsics()
                .ReadableStream.Construct(JsValue.Undefined, JsValue.Undefined);
        }

        // Step 5: Assert: stream is a ReadableStream object.
        if (stream is null)
        {
            throw new InvalidOperationException(
                "ReadableStream could not be created for body extraction."
            );
        }

        // Step 6: Let action be null.
        Func<byte[]>? action = null;

        // Step 7: Let source be null.
        JsValue? source = null;

        // Step 8: Let length be null.
        long? length = null;

        // Step 9: Let type be null.
        string? type = null;

        // Helper for step 11: Track the extracted byte sequence when available.
        byte[]? sourceBytes = null;

        // Step 10: Switch on object:
        if (obj is BlobInstance blobSource)
        {
            // Step 10 (Blob): Set source to object.
            source = blobSource;

            // Step 10 (Blob): Set length to object’s size.
            length = blobSource.Size;

            // Step 10 (Blob): If object’s type attribute is not the empty byte sequence, set type to its value.
            if (!string.IsNullOrEmpty(blobSource.Type))
            {
                type = blobSource.Type;
            }
        }
        else if (IsBufferSource(obj))
        {
            // Step 10 (BufferSource): Set source to a copy of the bytes held by object.
            sourceBytes = obj.TryAsBytes() ?? [];

            // Step 10 (BufferSource): Copy bytes into a fresh JS value.
            source = JsValue.FromObject(engine, sourceBytes);
        }
        else if (obj.TryAsBytes() is { } rawBytes)
        {
            // Step 10 (byte sequence): Set source to object.
            source = obj;

            // Step 10 (byte sequence): Capture the byte sequence for streaming.
            sourceBytes = rawBytes;
        }
        else if (obj is FormDataInstance formData)
        {
            // Step 10 (FormData): Set action to the multipart/form-data encoding algorithm placeholder.
            action = () =>
                throw new NotImplementedException(
                    "multipart/form-data encoding is not implemented yet."
                );

            // Step 10 (FormData): Set source to object.
            source = formData;

            // Step 10 (FormData): Set length to unclear (unknown) per spec.
            length = null;

            // Step 10 (FormData): Set type to `multipart/form-data; boundary=` + generated boundary string.
            var boundary = $"--------------------------{Guid.NewGuid():N}";
            type = $"multipart/form-data; boundary={boundary}";
        }
        else if (obj is URLSearchParamsInstance searchParams)
        {
            // Step 10 (URLSearchParams): Set source to the result of running the application/x-www-form-urlencoded serializer.
            var serialized = searchParams.ToString();

            // Step 10 (URLSearchParams): Encode the serialized data as bytes.
            sourceBytes = System.Text.Encoding.UTF8.GetBytes(serialized);

            // Step 10 (URLSearchParams): Store encoded bytes as source.
            source = JsValue.FromObject(engine, sourceBytes);

            // Step 10 (URLSearchParams): Set type to `application/x-www-form-urlencoded;charset=UTF-8`.
            type = "application/x-www-form-urlencoded;charset=UTF-8";
        }
        else if (obj.IsString())
        {
            // Step 10 (scalar value string): Set source to the UTF-8 encoding of object.
            sourceBytes = System.Text.Encoding.UTF8.GetBytes(obj.AsString());

            // Step 10 (scalar value string): Store encoded bytes as source.
            source = JsValue.FromObject(engine, sourceBytes);

            // Step 10 (scalar value string): Set type to `text/plain;charset=UTF-8`.
            type = "text/plain;charset=UTF-8";
        }
        else if (obj is ReadableStreamInstance readableSource)
        {
            // Step 10 (ReadableStream): If keepalive is true, then throw a TypeError.
            if (keepalive)
            {
                TypeErrorHelper.Throw(
                    engine,
                    "keepalive cannot be used with a ReadableStream body."
                );
            }

            // Step 10 (ReadableStream): If object is disturbed or locked, then throw a TypeError.
            if (readableSource.Disturbed || readableSource.Locked)
            {
                TypeErrorHelper.Throw(engine, "ReadableStream body is disturbed or locked.");
            }

            // Step 10 (ReadableStream): Keep source reference for completeness.
            source = readableSource;
        }
        else
        {
            // Step 10 (fallback): Throw for unsupported BodyInit types.
            TypeErrorHelper.Throw(engine, "Unsupported BodyInit type.");
        }

        // Step 11: If source is a byte sequence, then set action to a step that returns source and length to source’s length.
        if (sourceBytes is not null)
        {
            action = () => sourceBytes;
            length = sourceBytes.LongLength;
        }

        // Step 12: If action is non-null, then run these steps in parallel (executed synchronously here).
        if (action is not null)
        {
            // Step 13.1: Run action.
            var bytes = action();

            // Step 13.1: Whenever one or more bytes are available and stream is not errored, enqueue a Uint8Array of the available bytes into stream.
            if (bytes.Length > 0)
            {
                stream.Enqueue(engine.Intrinsics.Uint8Array.Construct(bytes));
            }

            // Step 13.1: When running action is done, close stream.
            // Use the controller's CloseInternal so that it only marks the stream as
            // closed AFTER any enqueued data has been fully drained from the queue.
            // Calling stream.CloseInternal() directly would immediately set the stream
            // state to "Closed", causing Read() to return {done:true} without reading
            // the enqueued bytes.
            stream.Controller.CloseInternal();
        }

        // Step 14: Let body be a body whose stream is stream, source is source, and length is length.
        var body = new BodyConcept(stream, source, length ?? -1);

        // Step 15: Return (body, type).
        return new BodyWithType(body, type);
    }

    private static bool IsBufferSource(JsValue value)
    {
        return value.IsArrayBuffer() || value.IsDataView() || value is JsTypedArray;
    }
}

internal sealed record BodyWithType(BodyConcept Body, string? Type);
