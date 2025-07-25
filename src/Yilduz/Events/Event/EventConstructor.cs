using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;

namespace Yilduz.Events.Event;

internal class EventConstructor : Constructor
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
        return arguments.Length == 0
            ? throw new JavaScriptException(
                "Failed to construct 'Event': 1 argument required, but only 0 present."
            )
            : new EventInstance(Engine, arguments.At(0).ToString(), arguments.At(1))
            {
                Prototype = PrototypeObject,
            };
    }
}
