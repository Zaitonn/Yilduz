using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Extensions;

namespace Yilduz.Events.Event;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/Event/Event
/// </summary>
public class EventConstructor : Constructor
{
    internal EventConstructor(Engine engine, string name = nameof(Event))
        : base(engine, name)
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));

        SetOwnProperty(nameof(EventPhase.NONE), new((int)EventPhase.NONE, true, false, true));
        SetOwnProperty(
            nameof(EventPhase.CAPTURING_PHASE),
            new((int)EventPhase.CAPTURING_PHASE, false, false, true)
        );
        SetOwnProperty(
            nameof(EventPhase.AT_TARGET),
            new((int)EventPhase.AT_TARGET, false, false, true)
        );
        SetOwnProperty(
            nameof(EventPhase.BUBBLING_PHASE),
            new((int)EventPhase.BUBBLING_PHASE, false, false, true)
        );
    }

    internal EventPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'Event'");

        return CreateInstanceWithEventName(arguments.At(0).ToString(), arguments.At(1));
    }

    /// <summary>
    /// Creates an instance of the Event object with the specified event name and options.
    /// </summary>
    public EventInstance CreateInstanceWithEventName(string eventName, JsValue options)
    {
        return new(Engine, eventName, options) { Prototype = PrototypeObject };
    }
}
