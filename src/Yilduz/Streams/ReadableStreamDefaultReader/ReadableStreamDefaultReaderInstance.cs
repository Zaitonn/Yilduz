using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Streams.ReadableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamDefaultReader;

/// <summary>
/// https://streams.spec.whatwg.org/#default-reader-class
/// <br/>
/// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamDefaultReader
/// </summary>
public sealed partial class ReadableStreamDefaultReaderInstance : ReadableStreamReader
{
    /// <summary>
    /// https://streams.spec.whatwg.org/#default-reader-constructor
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamDefaultReader/ReadableStreamDefaultReader
    /// </summary>
    internal ReadableStreamDefaultReaderInstance(Engine engine, ReadableStreamInstance stream)
        : base(engine)
    {
        SetUpDefaultReader(stream);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#default-reader-closed
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamDefaultReader/closed
    /// </summary>
    public override JsValue Closed => ClosedPromise?.Promise ?? Null;

    /// <summary>
    /// https://streams.spec.whatwg.org/#default-reader-cancel
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamDefaultReader/cancel
    /// </summary>
    public override JsValue Cancel(JsValue reason)
    {
        if (Stream == null)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    Engine.Intrinsics.TypeError.Construct("Reader is not attached to a stream")
                )
                .Promise;
        }

        return Stream.CancelInternal(reason);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#default-reader-read
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamDefaultReader/read
    /// </summary>
    public JsValue Read()
    {
        // If this.[[stream]] is undefined, return a promise rejected with a TypeError exception.
        if (Stream == null)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    Engine.Intrinsics.TypeError.Construct("Reader is not attached to a stream")
                )
                .Promise;
        }

        if (Stream.State == ReadableStreamState.Closed)
        {
            var result = Engine.Intrinsics.Object.Construct(Arguments.Empty);
            result.Set("value", Undefined);
            result.Set("done", true);
            return PromiseHelper.CreateResolvedPromise(Engine, result).Promise;
        }

        if (Stream.State == ReadableStreamState.Errored)
        {
            return PromiseHelper.CreateRejectedPromise(Engine, Stream.StoredError).Promise;
        }

        // Let promise be a new promise.
        var promise = Engine.Advanced.RegisterPromise();

        // Let readRequest be a new read request with the following items:
        var readRequest = new ReadRequest(
            ChunkSteps: (chunk) =>
            {
                // Resolve promise with «[ "value" → chunk, "done" → false ]».
                var result = Engine.Intrinsics.Object.Construct(Arguments.Empty);
                result.Set("value", chunk);
                result.Set("done", JsBoolean.False);
                promise.Resolve(result);
            },
            CloseSteps: (_) =>
            {
                // Resolve promise with «[ "value" → undefined, "done" → true ]».
                var result = Engine.Intrinsics.Object.Construct(Arguments.Empty);
                result.Set("value", Undefined);
                result.Set("done", JsBoolean.True);
                promise.Resolve(result);
            },
            // Reject promise with e.
            ErrorSteps: (e) => promise.Reject(e)
        );

        // Perform ! ReadableStreamDefaultReaderRead(this, readRequest).
        ReadInternal(readRequest);

        // Return promise.
        return promise.Promise;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#default-reader-release-lock
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamDefaultReader/releaseLock
    /// </summary>
    public void ReleaseLock()
    {
        if (Stream == null)
        {
            return;
        }

        Release();
    }
}
