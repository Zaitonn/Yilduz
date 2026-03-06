using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Aborting.AbortSignal;
using Yilduz.Events.Event;
using Yilduz.Extensions;
using Yilduz.Models;
using Yilduz.Utils;

namespace Yilduz.Events.EventTarget;

/// <summary>
/// Represents the prototype for EventTarget instances.
/// </summary>
public class EventTargetPrototype : PrototypeBase<EventTargetInstance>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventTargetPrototype"/> class.
    /// </summary>
    protected internal EventTargetPrototype(Engine engine, Constructor ctor)
        : base(engine, nameof(EventTarget), ctor)
    {
        RegisterMethod("addEventListener", AddEventListener, 2);
        RegisterMethod("removeEventListener", RemoveEventListener, 2);
        RegisterMethod("dispatchEvent", DispatchEvent, 1);

        RegisterConstant(nameof(EventPhase.AT_TARGET), EventPhase.AT_TARGET);
        RegisterConstant(nameof(EventPhase.BUBBLING_PHASE), EventPhase.BUBBLING_PHASE);
        RegisterConstant(nameof(EventPhase.CAPTURING_PHASE), EventPhase.CAPTURING_PHASE);
        RegisterConstant(nameof(EventPhase.NONE), EventPhase.NONE);
    }

    private static JsValue AddEventListener(EventTargetInstance eventTarget, JsValue[] arguments)
    {
        var type = arguments.At(0).ToString();
        var listener = arguments.At(1);
        var options = arguments.At(2);

        if (!listener.IsObject())
        {
            return Undefined;
        }

        eventTarget.AddEventListener(
            type,
            listener,
            options.IsObject()
                ? new()
                {
                    Capture = options.Get("capture").ConvertToBoolean(),
                    Once = options.Get("once").ConvertToBoolean(),
                    Passive = options.Get("passive").ConvertToBoolean(),
                    Signal = options.Get("signal") as AbortSignalInstance,
                }
                : default
        );

        return Undefined;
    }

    private static JsValue RemoveEventListener(EventTargetInstance eventTarget, JsValue[] arguments)
    {
        var type = arguments.At(0).AsString();
        var listener = arguments.At(1);
        var options = arguments.At(2);

        if (!listener.IsObject())
        {
            return Undefined;
        }

        eventTarget.RemoveEventListener(
            type,
            listener,
            options.IsObject()
                ? new()
                {
                    Capture = options.Get("capture").ConvertToBoolean(),
                    Once = options.Get("once").ConvertToBoolean(),
                    Passive = options.Get("passive").ConvertToBoolean(),
                    Signal = options.Get("signal") as AbortSignalInstance,
                }
                : default
        );

        return Undefined;
    }

    private JsValue DispatchEvent(EventTargetInstance eventTarget, JsValue[] arguments)
    {
        if (arguments[0] is not EventInstance evt)
        {
            TypeErrorHelper.Throw(
                Engine,
                "parameter 1 is not of type 'Event'.",
                "dispatchEvent",
                nameof(EventTarget)
            );

            return Undefined;
        }

        return eventTarget.DispatchEvent(evt);
    }
}
