using System;
using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Promise;
using Yilduz.Streams.ReadableStream;
using Yilduz.Utils;

namespace Yilduz.Streams;

public abstract class ReadableStreamGenericReaderInstance : ObjectInstance
{
    private protected ReadableStreamGenericReaderInstance(Engine engine)
        : base(engine)
    {
        ClosedPromise = engine.Advanced.RegisterPromise();
    }

    public abstract JsValue Closed { get; }
    public abstract JsValue Cancel(JsValue reason);
    internal ReadableStreamInstance? Stream { get; set; }
    internal ManualPromise ClosedPromise { get; set; }
    internal bool PromiseIsHandled { get; set; }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-reader-generic-initialize
    /// </summary>
    [MemberNotNull(nameof(Stream), nameof(ClosedPromise))]
    private protected void GenericInitialize(ReadableStreamInstance stream)
    {
        // Set reader.[[stream]] to stream.
        Stream = stream;
        // Set stream.[[reader]] to reader.
        // Note: This will need to be modified when ReadableStreamInstance supports generic readers
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
                throw new NotSupportedException($"Unknown stream state: {stream.State}");
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-reader-generic-release
    /// </summary>
    private protected void GenericRelease()
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
        Stream.Controller?.ReleaseSteps();

        // Set stream.[[reader]] to undefined.
        Stream.Reader = null;
        // Set reader.[[stream]] to undefined.
        Stream = null;
    }
}
