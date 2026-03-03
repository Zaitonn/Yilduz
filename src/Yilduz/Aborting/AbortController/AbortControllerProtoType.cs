using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;

namespace Yilduz.Aborting.AbortController;

internal sealed class AbortControllerPrototype : PrototypeBase<AbortControllerInstance>
{
    internal AbortControllerPrototype(Engine engine, AbortControllerConstructor ctor)
        : base(engine, nameof(AbortController), ctor)
    {
        RegisterMethod("abort", Abort);
        RegisterProperty("signal", controller => controller.Signal);
    }

    private static JsValue Abort(AbortControllerInstance thisObject, JsValue[] arguments)
    {
        thisObject.Abort(arguments.At(0));

        return Undefined;
    }
}
