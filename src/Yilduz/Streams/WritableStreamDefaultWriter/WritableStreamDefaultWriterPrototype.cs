using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStreamDefaultWriter;

internal sealed class WritableStreamDefaultWriterPrototype
    : PrototypeBase<WritableStreamDefaultWriterInstance>
{
    public WritableStreamDefaultWriterPrototype(
        Engine engine,
        WritableStreamDefaultWriterConstructor constructor
    )
        : base(engine, nameof(WritableStreamDefaultWriter), constructor)
    {
        RegisterProperty("closed", instance => instance.Closed);
        RegisterProperty("ready", instance => instance.Ready);
        RegisterProperty("desiredSize", GetDesiredSize);

        RegisterMethod("abort", Abort);
        RegisterMethod("close", Close);
        RegisterMethod("releaseLock", ReleaseLock);
        RegisterMethod("write", Write);
    }

    private static JsValue GetDesiredSize(WritableStreamDefaultWriterInstance instance)
    {
        var size = instance.DesiredSize;
        return size ?? Null;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStreamDefaultWriter/abort
    /// </summary>
    private JsValue Abort(WritableStreamDefaultWriterInstance instance, JsValue[] arguments)
    {
        var reason = arguments.At(0);

        try
        {
            var result = instance.Abort(reason);

            return result.IsPromise()
                ? result
                : PromiseHelper.CreateResolvedPromise(Engine, result).Promise;
        }
        catch (JavaScriptException e)
        {
            return PromiseHelper.CreateRejectedPromise(Engine, e.Error).Promise;
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStreamDefaultWriter/close
    /// </summary>
    private static JsValue Close(WritableStreamDefaultWriterInstance instance, JsValue[] arguments)
    {
        return instance.Close();
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStreamDefaultWriter/releaseLock
    /// </summary>
    private static JsValue ReleaseLock(
        WritableStreamDefaultWriterInstance instance,
        JsValue[] arguments
    )
    {
        instance.ReleaseLock();
        return Undefined;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStreamDefaultWriter/write
    /// </summary>
    private static JsValue Write(WritableStreamDefaultWriterInstance instance, JsValue[] arguments)
    {
        var chunk = arguments.At(0);
        return instance.Write(chunk);
    }
}
