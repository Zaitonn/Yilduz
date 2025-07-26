using System;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Events.EventTarget;
using Yilduz.Utils;

namespace Yilduz.Events.Event;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/Event
/// </summary>
public class EventInstance : ObjectInstance
{
    public EventInstance(Engine engine, string type, JsValue options)
        : base(engine)
    {
        Type = type;

        if (options.IsObject())
        {
            Bubbles = options.Get("bubbles").ToBoolean();
            Cancelable = options.Get("cancelable").ToBoolean();
            Composed = options.Get("composed").ToBoolean();
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/bubbles
    /// </summary>
    public bool Bubbles { get; protected set; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/cancelable
    /// </summary>
    public bool Cancelable { get; protected set; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/composed
    /// </summary>
    public bool Composed { get; protected set; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/currentTarget
    /// </summary>
    public object? CurrentTarget { get; protected internal set; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/defaultPrevented
    /// </summary>
    public bool DefaultPrevented { get; protected set; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/eventPhase
    /// </summary>
    public int EventPhase { get; protected internal set; } = EventPhases.NONE;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/isTrusted
    /// </summary>
    public bool IsTrusted { get; protected internal set; } = true;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/target
    /// </summary>
    public object? Target { get; protected internal set; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/timeStamp
    /// </summary>
    public long TimeStamp { get; } = DateTimeOffset.Now.ToUnixTimeMilliseconds();

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/type
    /// </summary>
    public new string Type { get; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/composedPath
    /// </summary>
    /// <returns>An array of <seealso cref="EventTargetInstance"/> objects representing the objects on which an event listener will be invoked.</returns>
    public virtual EventTargetInstance[] ComposedPath()
    {
        return [];
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/preventDefault
    /// </summary>
    public void PreventDefault()
    {
        if (!Cancelable)
        {
            return;
        }

        DefaultPrevented = true;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/stopPropagation
    /// </summary>
    public virtual void StopPropagation()
    {
        if (Bubbles)
        {
            IsPropagationStopped = true;
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/stopImmediatePropagation
    /// </summary>
    public virtual void StopImmediatePropagation()
    {
        StopPropagation();
        IsImmediatePropagationStopped = true;
    }

    protected internal bool IsPropagationStopped { get; protected set; }

    protected internal bool IsImmediatePropagationStopped { get; protected set; }
}
