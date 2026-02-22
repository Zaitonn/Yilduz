using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Aborting.AbortSignal;
using Yilduz.Events.Event;
using Yilduz.Extensions;
using Yilduz.Utils;

namespace Yilduz.Events.EventTarget;

#pragma warning disable IDE0046

/// <summary>
/// Represents the prototype for EventTarget instances.
/// </summary>
public class EventTargetPrototype : ObjectInstance
{
    private string Name =>
        _name ??= GetOwnProperty("name")?.Value?.ToString() ?? nameof(EventTarget);
    private string? _name;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventTargetPrototype"/> class.
    /// </summary>
    protected internal EventTargetPrototype(Engine engine, ObjectInstance ctor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(EventTarget));
        FastSetProperty("constructor", new(ctor, false, false, true));

        FastSetProperty(
            nameof(AddEventListener).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(AddEventListener), AddEventListener),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(RemoveEventListener).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(RemoveEventListener), RemoveEventListener),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(DispatchEvent).ToJsStyleName(),
            new(new ClrFunction(Engine, nameof(DispatchEvent), DispatchEvent), false, false, true)
        );
    }

    private JsValue AddEventListener(JsValue thisObject, JsValue[] arguments)
    {
        var eventTarget = thisObject.EnsureThisObject<EventTargetInstance>();

        arguments.EnsureCount(Engine, 2, "addEventListener", Name);

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

    private JsValue RemoveEventListener(JsValue thisObject, JsValue[] arguments)
    {
        var eventTarget = thisObject.EnsureThisObject<EventTargetInstance>();

        arguments.EnsureCount(Engine, 2, "removeEventListener", Name);

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

    private JsValue DispatchEvent(JsValue thisObject, JsValue[] arguments)
    {
        var eventTarget = thisObject.EnsureThisObject<EventTargetInstance>();

        arguments.EnsureCount(Engine, 1, "dispatchEvent", Name);

        if (arguments[0] is not EventInstance evt)
        {
            TypeErrorHelper.Throw(
                Engine,
                "parameter 1 is not of type 'Event'.",
                "dispatchEvent",
                Name
            );

            return Undefined;
        }

        return eventTarget.DispatchEvent(evt);
    }
}
