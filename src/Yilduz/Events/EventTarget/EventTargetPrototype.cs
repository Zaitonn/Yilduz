using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Aborting.AbortSignal;
using Yilduz.Events.Event;
using Yilduz.Utils;

namespace Yilduz.Events.EventTarget;

#pragma warning disable IDE0046

public class EventTargetPrototype : ObjectInstance
{
    protected string Name => _name ??= GetOwnProperty("name")?.Value?.ToString() ?? "EventTarget";
    private protected string? _name;

    protected internal EventTargetPrototype(Engine engine, ObjectInstance ctor)
        : base(engine)
    {
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

        arguments.EnsureCount(2, Engine, "addEventListener", Name);

        var type = arguments[0].AsString();
        var listener = arguments[1];
        var options = arguments.At(2);

        if (!listener.IsObject())
        {
            throw new JavaScriptException(
                Engine.Intrinsics.TypeError,
                "parameter 2 is not of type 'Object'."
            );
        }

        eventTarget.AddEventListener(
            type,
            listener,
            options.IsObject()
                ? new()
                {
                    Capture = options.Get("capture").ToBoolean(),
                    Once = options.Get("once").ToBoolean(),
                    Passive = options.Get("passive").ToBoolean(),
                    Signal = options.Get("signal") as AbortSignalInstance,
                }
                : default
        );

        return Undefined;
    }

    private JsValue RemoveEventListener(JsValue thisObject, JsValue[] arguments)
    {
        var eventTarget = thisObject.EnsureThisObject<EventTargetInstance>();

        arguments.EnsureCount(2, Engine, "removeEventListener", Name);

        var type = arguments[0].AsString();
        var listener = arguments[1];
        var options = arguments.At(2);

        if (!listener.IsObject())
        {
            TypeErrorHelper.Throw(
                "parameter 2 is not of type 'Object'.",
                Engine,
                "removeEventListener",
                Name
            );
        }

        eventTarget.RemoveEventListener(
            type,
            listener,
            options.IsObject()
                ? new()
                {
                    Capture = options.Get("capture").ToBoolean(),
                    Once = options.Get("once").ToBoolean(),
                    Passive = options.Get("passive").ToBoolean(),
                    Signal = options.Get("signal") as AbortSignalInstance,
                }
                : default
        );

        return Undefined;
    }

    private JsValue DispatchEvent(JsValue thisObject, JsValue[] arguments)
    {
        var eventTarget = thisObject.EnsureThisObject<EventTargetInstance>();

        arguments.EnsureCount(1, Engine, "dispatchEvent", Name);

        if (arguments[0] is not EventInstance evt)
        {
            TypeErrorHelper.Throw(
                "parameter 1 is not of type 'Event'.",
                Engine,
                "dispatchEvent",
                Name
            );

            return Undefined;
        }

        return eventTarget.DispatchEvent(evt);
    }
}
