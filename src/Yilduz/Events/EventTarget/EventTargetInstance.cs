using System.Collections.Generic;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Events.Event;
using EventPair = (
    Jint.Native.JsValue Listener,
    Yilduz.Events.EventTarget.EventTargetOptions Options
);

namespace Yilduz.Events.EventTarget;

#pragma warning disable IDE0046

public class EventTargetInstance : ObjectInstance
{
    protected readonly Dictionary<string, List<EventPair>> _listeners = [];

    protected internal EventTargetInstance(Engine engine)
        : base(engine) { }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/addEventListener
    /// </summary>
    public virtual void AddEventListener(string type, JsValue listener, EventTargetOptions options)
    {
        _listeners.TryGetValue(type, out var listeners);
        if (listeners == null)
        {
            listeners = [];
            _listeners[type] = listeners;
        }

        if (options.Signal is not null)
        {
            if (options.Signal.Aborted)
            {
                return;
            }

            options.Signal.Abort += (_, _) => RemoveEventListener(type, listener, options);
        }

        lock (listeners)
        {
            if (!listeners.Any(l => l.Listener == listener && l.Options.Equals(options)))
            {
                listeners.Add((listener, options));
            }
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/removeEventListener
    /// </summary>
    public virtual void RemoveEventListener(
        string type,
        JsValue listener,
        EventTargetOptions options
    )
    {
        if (_listeners.TryGetValue(type, out var listeners))
        {
            lock (listeners)
            {
                var toRemove = listeners.FirstOrDefault(l =>
                    l.Listener == listener && l.Options.Equals(options)
                );

                if (toRemove != default)
                {
                    listeners.Remove(toRemove);
                }

                if (listeners.Count == 0)
                {
                    _listeners.Remove(type);
                }
            }
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/dispatchEvent
    /// </summary>
    public bool DispatchEvent(EventInstance evt)
    {
        evt.Target = this;
        evt.CurrentTarget = this; // not standard

        if (_listeners.TryGetValue(evt.Type, out var listeners))
        {
            var toRemove = new List<EventPair>();
            IEnumerable<EventPair> copy;

            lock (listeners)
            {
                copy = [.. listeners];
            }

            evt.EventPhase = EventPhases.AT_TARGET;

            foreach (var pair in copy)
            {
                if (pair.Listener is ObjectInstance objectInstance1)
                {
                    try
                    {
                        objectInstance1.Engine.Invoke(objectInstance1, this, [evt]);
                    }
                    catch { }
                }

                if (pair.Options.Once || pair.Options.Signal?.Aborted == true)
                {
                    toRemove.Add(pair);
                }

                if (evt.IsImmediatePropagationStopped)
                {
                    break;
                }
            }

            toRemove.ForEach(pair => listeners.Remove(pair));
        }

        if (this["on" + evt.Type] is ObjectInstance objectInstance2)
        {
            Engine.Invoke(objectInstance2, this, [evt]);
        }

        return !(evt.Cancelable && evt.DefaultPrevented);
    }
}
