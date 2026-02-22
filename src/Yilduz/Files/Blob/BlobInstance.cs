using System;
using System.Collections.Generic;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Extensions;
using Yilduz.Streams.ReadableStream;
using Yilduz.Utils;
using SystemEncoding = System.Text.Encoding;

namespace Yilduz.Files.Blob;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/Blob
/// </summary>
public class BlobInstance : ObjectInstance
{
    private readonly Lazy<WebApiIntrinsics> _webApiIntrinsics;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Blob/type
    /// </summary>
    public new string Type { get; protected internal set; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Blob/size
    /// </summary>
    public int Size => Value.Count;

    protected internal List<byte> Value { get; set; } = [];

    internal BlobInstance(Engine engine, JsValue blobParts, JsValue options)
        : base(engine)
    {
        _webApiIntrinsics = new(Engine.GetWebApiIntrinsics);

        var optionsObject = !options.IsUndefined() ? options.AsObject() : null;

        Type = (optionsObject?.Get("type").ToString() ?? string.Empty).ToLowerInvariant().Trim();

        var endings = optionsObject?.Get("endings");
        var endingsStr =
            endings is null || endings.IsUndefined() == true ? "transparent" : endings.ToString();

        if (endingsStr != "transparent" && endingsStr != "native")
        {
            TypeErrorHelper.Throw(
                engine,
                $"Failed to read the 'endings' property from 'BlobPropertyBag': The provided value '{endings}' is not a valid enum value of type EndingType."
            );
        }

        if (!blobParts.IsUndefined())
        {
            ProcessBlobParts(blobParts.AsObject(), endingsStr);
        }
    }

    private void ProcessBlobParts(ObjectInstance blobParts, string endings)
    {
        foreach (var part in blobParts)
        {
            if (part is BlobInstance blob)
            {
                Value.AddRange(blob.Value);
            }
            else if (part.IsArrayBuffer())
            {
                Value.AddRange(part.AsArrayBuffer()!);
            }
            else if (part.IsDataView())
            {
                Value.AddRange(part.AsDataView()!);
            }
            else if (part is JsTypedArray)
            {
                Value.AddRange(part.Get("buffer").AsArrayBuffer()!);
            }
            else
            {
                var result = part.IsArray() ? string.Join(",", part.AsArray()) : part.ToString();

                if (endings == "native" && Environment.NewLine != "\n")
                {
                    result = result.Replace("\r\n", "\n").Replace("\n", "\r\n");
                }

                Value.AddRange(SystemEncoding.UTF8.GetBytes(result));
            }
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Blob/text
    /// </summary>
    public string Text()
    {
        return SystemEncoding.UTF8.GetString([.. Value]);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Blob/arrayBuffer
    /// </summary>
    public JsArrayBuffer ArrayBuffer()
    {
        return Engine.Intrinsics.ArrayBuffer.Construct([.. Value]);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Blob/bytes
    /// </summary>
    public JsTypedArray Bytes()
    {
        return Engine.Intrinsics.Uint8Array.Construct(Value.ToArray());
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Blob/stream
    /// <br/>
    /// https://w3c.github.io/FileAPI/#blob-get-stream
    /// </summary>
    public ReadableStreamInstance Stream()
    {
        var readableStream = (ReadableStreamInstance)
            _webApiIntrinsics.Value.ReadableStream.Construct(Arguments.Empty, Undefined);
        var chunk = Bytes();

        // Let chunk be a new Uint8Array wrapping an ArrayBuffer containing bytes.
        // If creating the ArrayBuffer throws an exception, then error stream with that exception and abort these steps.
        readableStream.Enqueue(chunk);
        return readableStream;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Blob/slice
    /// </summary>
    public BlobInstance Slice(int start = 0, int? end = null, string contentType = "")
    {
        if (start < 0)
        {
            start += Size;
        }

        if (end < 0)
        {
            end += Size;
        }
        if (end > Size)
        {
            end = Size;
        }

        end ??= Size;

        var slicedValue =
            start < 0 || start >= Size || end < 0 || start > end
                ? []
                : Value.Skip(start).Take((int)(end - start)).ToList();
        var blobInstance = (BlobInstance)
            _webApiIntrinsics.Value.Blob.Construct(Arguments.Empty, Undefined);

        blobInstance.Value = slicedValue;
        blobInstance.Type = contentType;

        return blobInstance;
    }
}
