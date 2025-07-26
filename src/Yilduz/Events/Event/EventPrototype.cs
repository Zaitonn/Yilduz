using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Utils;

namespace Yilduz.Events.Event;

public class EventPrototype : ObjectInstance
{
    internal EventPrototype(Engine engine)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(Event));
        FastSetProperty(
            nameof(EventInstance.Bubbles).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.Bubbles).ToJsGetterName(),
                    GetBubbles
                ),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.Cancelable).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.Cancelable).ToJsGetterName(),
                    GetCancelable
                ),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.Composed).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.Composed).ToJsGetterName(),
                    GetComposed
                ),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.CurrentTarget).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.CurrentTarget).ToJsGetterName(),
                    GetCurrentTarget
                ),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.DefaultPrevented).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.DefaultPrevented).ToJsGetterName(),
                    GetDefaultPrevented
                ),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.EventPhase).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.EventPhase).ToJsGetterName(),
                    GetEventPhase
                ),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.IsTrusted).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.IsTrusted).ToJsGetterName(),
                    GetIsTrusted
                ),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.Target).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.Target).ToJsGetterName(),
                    GetTarget
                ),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.TimeStamp).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.TimeStamp).ToJsGetterName(),
                    GetTimeStamp
                ),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.Type).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.Type).ToJsGetterName(),
                    GetEventType
                ),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            nameof(ComposedPath).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(ComposedPath).ToJsStyleName(), ComposedPath),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(PreventDefault).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(PreventDefault).ToJsStyleName(), PreventDefault),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(StopPropagation).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(StopPropagation).ToJsStyleName(), StopPropagation),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(StopImmediatePropagation).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(StopImmediatePropagation), StopImmediatePropagation),
                false,
                false,
                true
            )
        );

        FastSetProperty(nameof(EventPhases.NONE), new(EventPhases.NONE, true, false, true));
        FastSetProperty(
            nameof(EventPhases.CAPTURING_PHASE),
            new(EventPhases.CAPTURING_PHASE, false, false, true)
        );
        FastSetProperty(
            nameof(EventPhases.AT_TARGET),
            new(EventPhases.AT_TARGET, false, false, true)
        );
        FastSetProperty(
            nameof(EventPhases.BUBBLING_PHASE),
            new(EventPhases.BUBBLING_PHASE, false, false, true)
        );
    }

    protected internal EventPrototype(Engine engine, ObjectInstance ctor)
        : this(engine)
    {
        FastSetProperty("constructor", new(ctor, false, false, true));
    }

    private JsValue ComposedPath(JsValue thisObject, JsValue[] arguments)
    {
        return FromObject(Engine, thisObject.EnsureThisObject<EventInstance>().ComposedPath());
    }

    private JsValue PreventDefault(JsValue thisObject, JsValue[] arguments)
    {
        thisObject.EnsureThisObject<EventInstance>().PreventDefault();
        return Undefined;
    }

    private JsValue StopPropagation(JsValue thisObject, JsValue[] arguments)
    {
        thisObject.EnsureThisObject<EventInstance>().StopPropagation();
        return Undefined;
    }

    private JsValue StopImmediatePropagation(JsValue thisObject, JsValue[] arguments)
    {
        thisObject.EnsureThisObject<EventInstance>().StopImmediatePropagation();
        return Undefined;
    }

    private JsValue GetBubbles(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<EventInstance>().Bubbles;
    }

    private JsValue GetCancelable(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<EventInstance>().Cancelable;
    }

    private JsValue GetComposed(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<EventInstance>().Composed;
    }

    private JsValue GetCurrentTarget(JsValue thisObject, JsValue[] arguments)
    {
        return FromObject(Engine, thisObject.EnsureThisObject<EventInstance>().CurrentTarget);
    }

    private JsValue GetDefaultPrevented(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<EventInstance>().DefaultPrevented;
    }

    private JsValue GetEventPhase(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<EventInstance>().EventPhase;
    }

    private JsValue GetIsTrusted(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<EventInstance>().IsTrusted;
    }

    private JsValue GetTarget(JsValue thisObject, JsValue[] arguments)
    {
        return FromObject(Engine, thisObject.EnsureThisObject<EventInstance>().Target);
    }

    private JsValue GetTimeStamp(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<EventInstance>().TimeStamp;
    }

    private JsValue GetEventType(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<EventInstance>().Type;
    }
}
