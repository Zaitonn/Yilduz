using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Utils;

namespace Yilduz.Events.Event;

public class EventPrototype : ObjectInstance
{
    protected internal EventPrototype(Engine engine, ObjectInstance ctor)
        : base(engine)
    {
        FastSetProperty("constructor", new(ctor, false, false, true));

        FastSetProperty(
            nameof(EventInstance.Bubbles).ToJsStylePropertyName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.Bubbles).ToJsGetterName(),
                    GetBubbles
                ),
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.Cancelable).ToJsStylePropertyName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.Cancelable).ToJsGetterName(),
                    GetCancelable
                ),
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.Composed).ToJsStylePropertyName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.Composed).ToJsGetterName(),
                    GetComposed
                ),
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.CurrentTarget).ToJsStylePropertyName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.CurrentTarget).ToJsGetterName(),
                    GetCurrentTarget
                ),
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.DefaultPrevented).ToJsStylePropertyName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.DefaultPrevented).ToJsGetterName(),
                    GetDefaultPrevented
                ),
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.EventPhase).ToJsStylePropertyName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.EventPhase).ToJsGetterName(),
                    GetEventPhase
                ),
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.IsTrusted).ToJsStylePropertyName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.IsTrusted).ToJsGetterName(),
                    GetIsTrusted
                ),
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.Target).ToJsStylePropertyName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.Target).ToJsGetterName(),
                    GetTarget
                ),
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.TimeStamp).ToJsStylePropertyName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.TimeStamp).ToJsGetterName(),
                    GetTimeStamp
                ),
                false,
                true
            )
        );
        FastSetProperty(
            nameof(EventInstance.Type).ToJsStylePropertyName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(EventInstance.Type).ToJsGetterName(),
                    GetEventType
                ),
                false,
                true
            )
        );

        FastSetProperty(
            nameof(ComposedPath).ToJsStylePropertyName(),
            new(new ClrFunction(Engine, nameof(ComposedPath), ComposedPath), false, false, true)
        );
        FastSetProperty(
            nameof(PreventDefault).ToJsStylePropertyName(),
            new(new ClrFunction(Engine, nameof(PreventDefault), PreventDefault), false, false, true)
        );
        FastSetProperty(
            nameof(StopPropagation).ToJsStylePropertyName(),
            new(
                new ClrFunction(Engine, nameof(StopPropagation), StopPropagation),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(StopImmediatePropagation).ToJsStylePropertyName(),
            new(
                new ClrFunction(Engine, nameof(StopImmediatePropagation), StopImmediatePropagation),
                false,
                false,
                true
            )
        );
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
