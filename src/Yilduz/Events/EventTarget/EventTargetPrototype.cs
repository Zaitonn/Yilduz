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
    protected internal EventTargetPrototype(Engine engine, ObjectInstance ctor)
        : base(engine)
    {
        FastSetProperty("constructor", new(ctor, false, false, true));

        FastSetProperty(
            nameof(AddEventListener).ToJsStylePropertyName(),
            new(
                new ClrFunction(Engine, nameof(AddEventListener), AddEventListener),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(RemoveEventListener).ToJsStylePropertyName(),
            new(
                new ClrFunction(Engine, nameof(RemoveEventListener), RemoveEventListener),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            nameof(DispatchEvent).ToJsStylePropertyName(),
            new(new ClrFunction(Engine, nameof(DispatchEvent), DispatchEvent), false, false, true)
        );
    }

    private JsValue AddEventListener(JsValue thisObject, JsValue[] arguments)
    {
        var eventTarget = thisObject.EnsureThisObject<EventTargetInstance>();

        if (arguments.Length < 2)
        {
            throw new JavaScriptException(
                $"Failed to execute 'addEventListener' on 'EventTarget': 2 arguments required, but only {arguments.Length} present."
            );
        }

        var type = arguments[0].AsString();
        var listener = arguments[1];
        var options = arguments.At(2);

        if (!listener.IsObject())
        {
            throw new JavaScriptException("parameter 2 is not of type 'Object'.");
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

        if (arguments.Length < 2)
        {
            throw new JavaScriptException(
                $"Failed to execute 'removeEventListener' on 'EventTarget': 2 arguments required, but only {arguments.Length} present."
            );
        }

        var type = arguments[0].AsString();
        var listener = arguments[1];
        var options = arguments.At(2);

        if (!listener.IsObject())
        {
            throw new JavaScriptException("parameter 2 is not of type 'Object'.");
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
        if (arguments.Length < 1)
        {
            throw new JavaScriptException(
                "Failed to execute 'dispatchEvent' on 'EventTarget': 1 argument required, but 0 present."
            );
        }

        if (arguments[0] is not EventInstance evt)
        {
            throw new JavaScriptException("parameter 1 is not of type 'Event'.");
        }

        return eventTarget.DispatchEvent(evt);
    }
}
