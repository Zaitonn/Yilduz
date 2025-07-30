using System;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStreamDefaultWriter;

/// <summary>
/// WritableStreamDefaultWriter implementation according to WHATWG Streams Standard
/// https://streams.spec.whatwg.org/#writablestreamdefaultwriter
/// </summary>
public sealed partial class WritableStreamDefaultWriterInstance : ObjectInstance
{
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

            return Stream.Controller.GetDesiredSize();
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-writer-ready
    /// </summary>
    public JsValue Ready =>
        Stream is not null
            ? ReadyPromise?.Promise ?? Undefined
            : PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    Engine.Intrinsics.TypeError.Construct(
                        "This writable stream writer has been released and cannot be used to monitor the stream's state"
                    )
                )
                .Promise;

    internal WritableStreamDefaultWriterInstance(Engine engine)
        : base(engine) { }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-writer-abort
    /// </summary>
    public JsValue Abort(JsValue reason = null!)
    {
        if (Stream == null)
        {
            TypeErrorHelper.Throw(Engine, "Writer is released");
        }

        return AbortInternal(reason);
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

        if (Stream.IsCloseQueuedOrInFlight)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Stream is already being closed")
                )
                .Promise;
        }

        return Stream.CloseInternal();
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

        Release();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-writer-write
    /// </summary>
    public JsValue Write(JsValue chunk)
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

        try
        {
            var result = WriteInternal(chunk);

            return result.IsPromise()
                ? result
                : PromiseHelper.CreateResolvedPromise(Engine, result).Promise;
        }
        catch (JavaScriptException e)
        {
            return PromiseHelper.CreateRejectedPromise(Engine, e.Error).Promise;
        }
        catch (Exception e)
        {
            return PromiseHelper.CreateRejectedPromise(Engine, e.Message).Promise;
        }
    }
}
