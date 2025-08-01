using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Native;
using Jint.Native.Promise;
using Jint.Runtime;
using Yilduz.Streams.ReadableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamDefaultReader;

public sealed partial class ReadableStreamDefaultReaderInstance
{
    internal readonly List<ReadRequest> ReadRequests = [];
    internal ReadableStreamInstance? Stream { get; set; }
    internal ManualPromise ClosedPromise { get; set; }
    internal bool PromiseIsHandled { get; set; }

    /// <summary>
    /// https://streams.spec.whatwg.org/#set-up-readable-stream-default-reader
    /// </summary>
    [MemberNotNull(nameof(Stream), nameof(ClosedPromise))]
    private void SetUpDefaultReader(ReadableStreamInstance stream)
    {
        // If ! IsReadableStreamLocked(stream) is true, throw a TypeError exception.
        if (stream.Locked)
        {
            throw new JavaScriptException(
                Engine.Intrinsics.TypeError.Construct("Stream is already locked")
            );
        }

        // Perform ! ReadableStreamReaderGenericInitialize(reader, stream).
        GenericInitialize(stream);

        // Set reader.[[readRequests]] to a new empty list.
        ReadRequests.Clear();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-reader-generic-initialize
    /// </summary>
    [MemberNotNull(nameof(Stream), nameof(ClosedPromise))]
    private void GenericInitialize(ReadableStreamInstance stream)
    {
        // Set reader.[[stream]] to stream.
        Stream = stream;
        // Set stream.[[reader]] to reader.
        Stream.Reader = this;

        // If stream.[[state]] is "readable",
        switch (stream.State)
        {
            case ReadableStreamState.Readable:
                // Set reader.[[closedPromise]] to a new promise.
                ClosedPromise = Engine.Advanced.RegisterPromise();
                PromiseIsHandled = false;
                break;

            case ReadableStreamState.Closed:
                // Set reader.[[closedPromise]] to a new promise resolved with undefined.
                ClosedPromise = PromiseHelper.CreateResolvedPromise(Engine, Undefined);
                break;

            case ReadableStreamState.Errored:
                // Set reader.[[closedPromise]] to a new promise rejected with stream.[[storedError]].
                // Set reader.[[closedPromise]].[[PromiseIsHandled]] to true.
                ClosedPromise = PromiseHelper.CreateRejectedPromise(Engine, stream.StoredError);
                PromiseIsHandled = true;
                break;

            default:
                throw new NotSupportedException();
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#abstract-opdef-readablestreamdefaultreaderrelease
    /// </summary>
    internal void Release()
    {
        // Perform ! ReadableStreamReaderGenericRelease(reader).
        GenericRelease();
        // Let e be a new TypeError exception.
        // Perform ! ReadableStreamDefaultReaderErrorReadRequests(reader, e).
        ErrorReadRequests(Engine.Intrinsics.TypeError.Construct("Reader was released"));
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-reader-generic-release
    /// </summary>
    private void GenericRelease()
    {
        // Let stream be reader.[[stream]].
        // Assert: stream is not undefined.
        if (Stream == null)
        {
            return;
        }

        // Assert: stream.[[reader]] is reader.
        // If stream.[[state]] is "readable", reject reader.[[closedPromise]] with a TypeError exception.
        if (Stream.State == ReadableStreamState.Readable)
        {
            var error = Engine.Intrinsics.TypeError.Construct("Reader was released");
            ClosedPromise.Reject(error);
        }
        // Otherwise, set reader.[[closedPromise]] to a promise rejected with a TypeError exception.
        else
        {
            ClosedPromise = PromiseHelper.CreateRejectedPromise(
                Engine,
                Engine.Intrinsics.TypeError.Construct("Reader was released")
            );
        }
        // Set reader.[[closedPromise]].[[PromiseIsHandled]] to true.
        PromiseIsHandled = true;

        // Perform ! stream.[[controller]].[[ReleaseSteps]]().
        Stream.Controller.ReleaseSteps();

        // Set stream.[[reader]] to undefined.
        Stream.Reader = null;
        // Set reader.[[stream]] to undefined.
        Stream = null;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#abstract-opdef-readablestreamdefaultreadererrorreadrequests
    /// </summary>
    private void ErrorReadRequests(JsValue e)
    {
        // Let readRequests be reader.[[readRequests]].
        // Set reader.[[readRequests]] to a new empty list.
        // For each readRequest of readRequests,
        // Perform readRequest’s error steps, given e.

        foreach (var readRequest in ReadRequests)
        {
            readRequest.ErrorSteps(e);
        }
        ReadRequests.Clear();
    }
}
