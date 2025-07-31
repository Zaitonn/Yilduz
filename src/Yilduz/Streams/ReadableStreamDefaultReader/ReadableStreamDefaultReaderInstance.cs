using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Promise;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Streams.ReadableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamDefaultReader;

/// <summary>
/// https://streams.spec.whatwg.org/#default-reader-class
/// <br/>
/// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamDefaultReader
/// </summary>
public sealed partial class ReadableStreamDefaultReaderInstance : ObjectInstance
{
    internal readonly List<ReadRequest> ReadRequests = [];
    internal ReadableStreamInstance? Stream { get; set; }
    internal ManualPromise ClosedPromise { get; set; }
    internal bool PromiseIsHandled { get; set; }

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
    public JsValue Closed => ClosedPromise?.Promise ?? Null;

    /// <summary>
    /// https://streams.spec.whatwg.org/#default-reader-cancel
    /// <br/>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamDefaultReader/cancel
    /// </summary>
    public JsValue Cancel(JsValue reason)
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
            ChunkSteps: new ClrFunction(
                Engine,
                "chunkSteps",
                (_, args) =>
                {
                    var chunk = args.Length > 0 ? args[0] : Undefined;
                    var result = Engine.Intrinsics.Object.Construct([]);
                    result.Set("value", chunk);
                    result.Set("done", JsBoolean.False);
                    promise.Resolve(result);
                    return Undefined;
                }
            ),
            CloseSteps: new ClrFunction(
                Engine,
                "closeSteps",
                (_, _) =>
                {
                    var result = Engine.Intrinsics.Object.Construct([]);
                    result.Set("value", Undefined);
                    result.Set("done", JsBoolean.True);
                    promise.Resolve(result);
                    return Undefined;
                }
            ),
            ErrorSteps: new ClrFunction(
                Engine,
                "errorSteps",
                (_, args) =>
                {
                    var error = args.Length > 0 ? args[0] : Undefined;
                    promise.Reject(error);
                    return Undefined;
                }
            )
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
