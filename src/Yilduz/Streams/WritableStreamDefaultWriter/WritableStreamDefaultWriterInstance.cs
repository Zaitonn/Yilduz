using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Promise;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStreamDefaultWriter;

/// <summary>
/// WritableStreamDefaultWriter implementation according to WHATWG Streams Standard
/// https://streams.spec.whatwg.org/#writablestreamdefaultwriter
/// </summary>
public sealed class WritableStreamDefaultWriterInstance : ObjectInstance
{
    /// <summary>
    /// Internal slots as defined in the WHATWG Streams specification
    /// https://streams.spec.whatwg.org/#ws-default-writer-internal-slots
    /// </summary>

    /// <summary>
    /// [[closedPromise]] - A promise returned by the writer's closed getter
    /// </summary>
    internal ManualPromise? ClosedPromise { get; set; }

    /// <summary>
    /// [[readyPromise]] - A promise returned by the writer's ready getter
    /// </summary>
    internal ManualPromise? ReadyPromise { get; set; }

    /// <summary>
    /// [[stream]] - A WritableStream instance that owns this reader
    /// </summary>
    internal WritableStreamInstance? Stream { get; set; }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-writer-closed
    /// </summary>
    public JsValue Closed => ClosedPromise?.Promise ?? Null;

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-writer-desired-size
    /// </summary>
    public double? DesiredSize
    {
        get
        {
            if (Stream == null)
            {
                TypeErrorHelper.Throw(Engine, "Writer is released");
            }

            if (
                Stream.State == WritableStreamState.Errored
                || Stream.State == WritableStreamState.Erroring
            )
            {
                return null;
            }

            if (Stream.State == WritableStreamState.Closed)
            {
                return 0;
            }

            return WritableStreamDefaultController.AbstractOperations.WritableStreamDefaultControllerGetDesiredSize(
                Stream.Controller!
            );
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-writer-ready
    /// </summary>
    public JsValue Ready => ReadyPromise?.Promise ?? Undefined;

    internal WritableStreamDefaultWriterInstance(Engine engine)
        : base(engine) { }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-writer-abort
    /// </summary>
    public JsValue Abort(JsValue reason = null!)
    {
        if (Stream == null)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Writer is released")
                )
                .Promise;
        }

        return AbstractOperations.WritableStreamDefaultWriterAbort(this, reason ?? Undefined);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-writer-close
    /// </summary>
    public JsValue Close()
    {
        if (Stream == null)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Writer is released")
                )
                .Promise;
        }

        var stream = Stream;
        if (WritableStream.AbstractOperations.WritableStreamCloseQueuedOrInFlight(stream))
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Stream is already being closed")
                )
                .Promise;
        }

        return AbstractOperations.WritableStreamDefaultWriterClose(this);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-writer-release-lock
    /// </summary>
    public void ReleaseLock()
    {
        if (Stream == null)
        {
            return;
        }

        AbstractOperations.WritableStreamDefaultWriterRelease(this);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-writer-write
    /// </summary>
    public JsValue Write(JsValue chunk = null!)
    {
        if (Stream == null)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Writer is released")
                )
                .Promise;
        }

        return AbstractOperations.WritableStreamDefaultWriterWrite(this, chunk ?? Undefined);
    }
}
