using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;

namespace Yilduz.Aborting.AbortSignal;

internal sealed class AbortSignalPrototype : PrototypeBase<AbortSignalInstance>
{
    public AbortSignalPrototype(Engine engine, AbortSignalConstructor ctor)
        : base(engine, nameof(AbortSignal), ctor)
    {
        RegisterMethod("throwIfAborted", ThrowIfAborted);

        RegisterProperty("aborted", signal => signal.Aborted);
        RegisterProperty("reason", signal => signal.Reason);
        RegisterProperty("onabort", GetOnabort, SetOnabort, Types.Object);
    }

    private static JsValue ThrowIfAborted(AbortSignalInstance thisObject, JsValue[] arguments)
    {
        thisObject.ThrowIfAborted();
        return Undefined;
    }

    private static JsValue GetOnabort(AbortSignalInstance thisObject)
    {
        return thisObject.OnAbort;
    }

    private static JsValue SetOnabort(AbortSignalInstance thisObject, JsValue argument)
    {
        thisObject.OnAbort = argument;
        return thisObject.OnAbort;
    }
}
