using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Utils;

namespace Yilduz.Events.Event;

public class EventConstructor : Constructor
{
    public EventConstructor(Engine engine)
        : base(engine, nameof(Event))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));

        SetOwnProperty(nameof(EventPhases.NONE), new(EventPhases.NONE, true, false, true));
        SetOwnProperty(
            nameof(EventPhases.CAPTURING_PHASE),
            new(EventPhases.CAPTURING_PHASE, false, false, true)
        );
        SetOwnProperty(
            nameof(EventPhases.AT_TARGET),
            new(EventPhases.AT_TARGET, false, false, true)
        );
        SetOwnProperty(
            nameof(EventPhases.BUBBLING_PHASE),
            new(EventPhases.BUBBLING_PHASE, false, false, true)
        );
    }

    public EventPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'Event'");

        return new EventInstance(Engine, arguments.At(0).ToString(), arguments.At(1))
        {
            Prototype = PrototypeObject,
        };
    }
}
