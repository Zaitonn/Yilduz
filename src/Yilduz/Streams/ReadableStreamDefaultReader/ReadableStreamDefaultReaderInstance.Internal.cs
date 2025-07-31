using System;
using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Runtime;
using Yilduz.Streams.ReadableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamDefaultReader;

public sealed partial class ReadableStreamDefaultReaderInstance
{
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
        #region ReadableStreamReaderGenericInitialize

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

        #endregion

        // Set reader.[[readRequests]] to a new empty list.
        ReadRequests.Clear();
    }

    /// <summary>
    /// Releases the reader
    /// </summary>
    internal void Release()
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
        // Stream.Controller?.CallReleaseSteps();
        // TODO

        // Set stream.[[reader]] to undefined.
        Stream.Reader = null;
        // Set reader.[[stream]] to undefined.
        Stream = null;
    }
}
