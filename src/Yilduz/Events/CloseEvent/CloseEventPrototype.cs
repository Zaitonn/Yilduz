using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Events.CloseEvent;

/// <summary>
/// Prototype for <see cref="CloseEventInstance"/>.
/// https://websockets.spec.whatwg.org/#the-closeevent-interface
/// </summary>
internal sealed class CloseEventPrototype : ObjectInstance
{
    public CloseEventPrototype(Engine engine, CloseEventConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(CloseEvent));
        FastSetProperty("constructor", new(constructor, false, false, true));

        FastSetProperty(
            "wasClean",
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, "get wasClean", GetWasClean),
                set: null,
                enumerable: true,
                configurable: true
            )
        );

        FastSetProperty(
            "code",
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, "get code", GetCode),
                set: null,
                enumerable: true,
                configurable: true
            )
        );

        FastSetProperty(
            "reason",
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, "get reason", GetReason),
                set: null,
                enumerable: true,
                configurable: true
            )
        );
    }

    private static JsValue GetWasClean(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<CloseEventInstance>().WasClean;
    }

    private static JsValue GetCode(JsValue thisObject, JsValue[] arguments)
    {
        return (int)thisObject.EnsureThisObject<CloseEventInstance>().Code;
    }

    private static JsValue GetReason(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<CloseEventInstance>().Reason;
    }
}
