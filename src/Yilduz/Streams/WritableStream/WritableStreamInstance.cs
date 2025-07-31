using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Streams.QueuingStrategy;
using Yilduz.Streams.WritableStreamDefaultWriter;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStream;

/// <summary>
/// WritableStream implementation according to WHATWG Streams Standard
/// https://streams.spec.whatwg.org/#ws-class
/// </summary>
public sealed partial class WritableStreamInstance : ObjectInstance
{
    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-locked
    /// </summary>
    public bool Locked => Writer != null;

    /// <summary>
    /// Constructor for user-created WritableStream instances
    /// https://streams.spec.whatwg.org/#ws-constructor
    /// </summary>
    public WritableStreamInstance(Engine engine, JsValue underlyingSink, JsValue strategy)
        : base(engine)
    {
        var underlyingSinkDict = underlyingSink.IsObject() ? underlyingSink.AsObject() : null;

        // Step 3: If underlyingSinkDict["type"] exists, throw a RangeError exception
        if (underlyingSinkDict?.HasProperty("type") == true)
        {
            throw new JavaScriptException(
                ErrorHelper.Create(
                    Engine,
                    "RangeError",
                    "WritableStream constructor does not support a type property"
                )
            );
        }

        // Step 4: Perform ! InitializeWritableStream(this)
        // AbstractOperations.InitializeWritableStream(this);

        // Step 5: Let sizeAlgorithm be ! ExtractSizeAlgorithm(strategy)
        // Step 6: Let highWaterMark be ? ExtractHighWaterMark(strategy, 1)
        var (highWaterMark, sizeAlgorithm) = AbstractOperations.ExtractQueuingStrategy(
            engine,
            strategy,
            1
        );

        // Step 7: Perform ? SetUpWritableStreamDefaultControllerFromUnderlyingSink
        SetUpControllerFromUnderlyingSink(
            underlyingSink,
            underlyingSinkDict,
            highWaterMark,
            sizeAlgorithm
        );
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-abort
    /// </summary>
    public JsValue Abort(JsValue reason)
    {
        if (Locked)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Stream is locked")
                )
                .Promise;
        }

        return AbortInternal(reason);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-close
    /// </summary>
    public JsValue Close()
    {
        if (!Locked)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Stream is locked")
                )
                .Promise;
        }

        if (IsCloseQueuedOrInFlight)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Stream is already being closed")
                )
                .Promise;
        }

        return CloseInternal();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-get-writer
    /// </summary>
    public WritableStreamDefaultWriterInstance GetWriter()
    {
        return AcquireWriter();
    }
}
