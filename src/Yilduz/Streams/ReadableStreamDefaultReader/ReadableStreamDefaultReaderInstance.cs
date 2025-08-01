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
public sealed partial class ReadableStreamDefaultReaderInstance
    : ReadableStreamGenericReaderInstance
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

        return Stream.Cancel(reason);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#default-reader-read
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamDefaultReader/read
    /// </summary>
    public JsValue Read()
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

        var promise = Engine.Advanced.RegisterPromise();

        var readRequest = new ReadRequest(
            ChunkSteps: (chunk) =>
            {
                var result = Engine.Intrinsics.Object.Construct(Arguments.Empty);
                result.Set("value", chunk);
                result.Set("done", JsBoolean.False);
                promise.Resolve(result);
            },
            CloseSteps: () =>
            {
                var result = Engine.Intrinsics.Object.Construct(Arguments.Empty);
                result.Set("value", Undefined);
                result.Set("done", JsBoolean.True);
                promise.Resolve(result);
            },
            ErrorSteps: (error) => promise.Reject(error)
        );

        ReadRequests.Add(readRequest);
        Stream.Controller?.CallPullIfNeeded();

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
