using Jint;
using Jint.Native;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Events.EventTarget;
using Yilduz.Utils;

namespace Yilduz.Aborting.AbortSignal;

internal class AbortSignalPrototype : EventTargetPrototype
{
    internal AbortSignalPrototype(Engine engine, AbortSignalConstructor ctor)
        : base(engine, ctor)
    {
        FastSetProperty(
            nameof(AbortSignalInstance.Aborted).ToJsStylePropertyName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(AbortSignalInstance.Aborted).ToJsGetterName(),
                    GetAborted
                ),
                false,
                true
            )
        );
        FastSetProperty(
            nameof(AbortSignalInstance.Reason).ToJsStylePropertyName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(AbortSignalInstance.Reason).ToJsGetterName(),
                    GetReason
                ),
                false,
                true
            )
        );

        FastSetProperty(
            nameof(ThrowIfAborted).ToJsStylePropertyName(),
            new(
                new ClrFunction(
                    engine,
                    nameof(ThrowIfAborted).ToJsStylePropertyName(),
                    ThrowIfAborted
                ),
                false,
                false,
                true
            )
        );
    }

    private JsValue ThrowIfAborted(JsValue thisObject, JsValue[] arguments)
    {
        thisObject.EnsureThisObject<AbortSignalInstance>().ThrowIfAborted();
        return Undefined;
    }

    private JsValue GetReason(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<AbortSignalInstance>().Reason;
    }

    private JsValue GetAborted(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<AbortSignalInstance>().Aborted;
    }
}
