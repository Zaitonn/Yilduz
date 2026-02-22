using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Aborting.AbortSignal;

internal sealed class AbortSignalPrototype : ObjectInstance
{
    private static readonly string AbortedName = nameof(AbortSignalInstance.Aborted)
        .ToJsStyleName();
    private static readonly string ReasonName = nameof(AbortSignalInstance.Reason).ToJsStyleName();
    private static readonly string ThrowIfAbortedName = nameof(AbortSignalInstance.ThrowIfAborted)
        .ToJsStyleName();
    private static readonly string OnabortName = nameof(AbortSignalInstance.OnAbort)
        .ToLowerInvariant();

    public AbortSignalPrototype(Engine engine, AbortSignalConstructor ctor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(AbortSignal));
        FastSetProperty("constructor", new(ctor, false, false, true));

        FastSetProperty(
            ThrowIfAbortedName,
            new(new ClrFunction(engine, ThrowIfAbortedName, ThrowIfAborted), false, false, true)
        );
        FastSetProperty(
            AbortedName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, AbortedName.ToJsGetterName(), GetAborted),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            ReasonName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ReasonName.ToJsGetterName(), GetReason),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            OnabortName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, OnabortName.ToJsGetterName(), GetOnabort),
                set: new ClrFunction(engine, OnabortName.ToJsSetterName(), SetOnabort),
                false,
                true
            )
        );
    }

    private static JsValue ThrowIfAborted(JsValue thisObject, JsValue[] arguments)
    {
        thisObject.EnsureThisObject<AbortSignalInstance>().ThrowIfAborted();
        return Undefined;
    }

    private static JsValue GetReason(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<AbortSignalInstance>().Reason;
    }

    private static JsValue GetAborted(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<AbortSignalInstance>().Aborted;
    }

    private static JsValue GetOnabort(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<AbortSignalInstance>().OnAbort;
    }

    private static JsValue SetOnabort(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<AbortSignalInstance>();
        instance.OnAbort = arguments.At(0);
        return instance.OnAbort;
    }
}
