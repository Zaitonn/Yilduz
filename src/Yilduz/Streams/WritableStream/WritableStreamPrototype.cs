using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStream;

internal sealed class WritableStreamPrototype : PrototypeBase<WritableStreamInstance>
{
    public WritableStreamPrototype(Engine engine, WritableStreamConstructor constructor)
        : base(engine, "WritableStream", constructor)
    {
        RegisterProperty("locked", instance => instance.Locked);
        RegisterMethod("abort", Abort);
        RegisterMethod("close", Close);
        RegisterMethod("getWriter", GetWriter);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStream/abort
    /// </summary>
    private static JsValue Abort(WritableStreamInstance instance, JsValue[] arguments)
    {
        var reason = arguments.At(0);
        return instance.Abort(reason);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStream/close
    /// </summary>
    private JsValue Close(WritableStreamInstance instance, JsValue[] arguments)
    {
        instance.Close();
        return PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/WritableStream/getWriter
    /// </summary>
    private static JsValue GetWriter(WritableStreamInstance instance, JsValue[] arguments)
    {
        return instance.GetWriter();
    }
}
