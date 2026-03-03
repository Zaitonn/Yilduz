using Jint;
using Jint.Native;
using Yilduz.Models;

namespace Yilduz.Events.Event;

public class EventPrototype : PrototypeBase<EventInstance>
{
    internal EventPrototype(Engine engine, EventConstructor constructor)
        : base(engine, nameof(Event), constructor)
    {
        RegisterProperty("bubbles", e => e.Bubbles);
        RegisterProperty("cancelable", e => e.Cancelable);
        RegisterProperty("composed", e => e.Composed);
        RegisterProperty("currentTarget", e => FromObject(Engine, e.CurrentTarget));
        RegisterProperty("defaultPrevented", e => e.DefaultPrevented);
        RegisterProperty("eventPhase", e => e.EventPhase);
        RegisterProperty("isTrusted", e => e.IsTrusted);
        RegisterProperty("target", e => FromObject(Engine, e.Target));
        RegisterProperty("timeStamp", e => e.TimeStamp);
        RegisterProperty("type", e => e.Type);

        RegisterMethod("composedPath", (e, _) => FromObject(Engine, e.ComposedPath()));
        RegisterMethod("preventDefault", PreventDefault);
        RegisterMethod("stopPropagation", StopPropagation);
        RegisterMethod("stopImmediatePropagation", StopImmediatePropagation);

        RegisterConstant(nameof(EventPhases.NONE), EventPhases.NONE);
        RegisterConstant(nameof(EventPhases.CAPTURING_PHASE), EventPhases.CAPTURING_PHASE);
        RegisterConstant(nameof(EventPhases.AT_TARGET), EventPhases.AT_TARGET);
        RegisterConstant(nameof(EventPhases.BUBBLING_PHASE), EventPhases.BUBBLING_PHASE);
    }

    private static JsValue PreventDefault(EventInstance evt, JsValue[] arguments)
    {
        evt.PreventDefault();
        return Undefined;
    }

    private static JsValue StopPropagation(EventInstance evt, JsValue[] arguments)
    {
        evt.StopPropagation();
        return Undefined;
    }

    private static JsValue StopImmediatePropagation(EventInstance evt, JsValue[] arguments)
    {
        evt.StopImmediatePropagation();
        return Undefined;
    }
}
