using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Events.EventTarget;

public sealed class EventTargetConstructor : Constructor
{
    public EventTargetConstructor(Engine engine)
        : base(engine, nameof(EventTarget))
    {
        PrototypeObject = new(engine, this);

        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public EventTargetPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new EventTargetInstance(Engine) { Prototype = PrototypeObject };
    }
}
