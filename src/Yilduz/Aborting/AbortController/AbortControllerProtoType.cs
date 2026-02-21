using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Aborting.AbortSignal;
using Yilduz.Extensions;

namespace Yilduz.Aborting.AbortController;

internal sealed class AbortControllerPrototype : ObjectInstance
{
    internal AbortControllerPrototype(Engine engine, AbortControllerConstructor ctor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(AbortController));
        FastSetProperty("constructor", new(ctor, false, false, true));

        FastSetProperty(
            nameof(AbortControllerInstance.Signal).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    Engine,
                    nameof(AbortControllerInstance.Signal).ToJsGetterName(),
                    GetSignal
                ),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(Abort).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(Abort).ToJsStyleName(), Abort), false, false, true)
        );
    }

    private static AbortSignalInstance GetSignal(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<AbortControllerInstance>().Signal;
    }

    private static JsValue Abort(JsValue thisObject, params JsValue[] arguments)
    {
        thisObject.EnsureThisObject<AbortControllerInstance>().Abort(arguments.At(0));

        return Undefined;
    }
}
