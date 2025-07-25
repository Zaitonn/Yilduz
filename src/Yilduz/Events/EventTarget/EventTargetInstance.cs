using System.Collections.Generic;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Events.Event;

namespace Yilduz.Events.EventTarget;

#pragma warning disable IDE0046

public class EventTargetInstance : ObjectInstance
{
    protected readonly Dictionary<
        string,
        List<(JsValue Listener, EventTargetOptions Options)>
    > _listeners = [];

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
        EventTargetOptions? options
    )
    {
        if (_listeners.TryGetValue(type, out var listeners))
        {
            var i = listeners.FindIndex(l => l.Listener == listener && l.Options.Equals(options));

            if (i >= 0)
            {
                listeners.RemoveAt(i);
            }

            if (listeners.Count == 0)
            {
                _listeners.Remove(type);
            }
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/dispatchEvent
    /// </summary>
    public bool DispatchEvent(EventInstance evt)
    {
        if (_listeners.TryGetValue(evt.Type, out var listeners))
        {
            var list = new List<(JsValue Listener, EventTargetOptions Options)>(listeners);

            lock (listeners)
            {
                foreach (var pair in listeners)
                {
                    if (pair.Listener is ObjectInstance objectInstance1)
                    {
                        objectInstance1.Engine.Invoke(objectInstance1, this, evt);
                    }

                    if (pair.Options.Once)
                    {
                        list.Add(pair);
                    }
                }
            }

            list.ForEach(pair => listeners.Remove(pair));
        }

        if (this["on" + evt.Type] is ObjectInstance objectInstance2)
        {
            Engine.Invoke(objectInstance2, this, [evt]);
        }

        return !(evt.Cancelable && evt.DefaultPrevented);
    }
}
