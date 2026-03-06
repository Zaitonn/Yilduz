using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Events.EventTarget;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/EventTarget/EventTarget
/// </summary>
public sealed class EventTargetConstructor : Constructor
{
    internal EventTargetConstructor(Engine engine)
        : base(engine, nameof(EventTarget))
    {
        PrototypeObject = new(engine, this);

        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public EventTargetPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new EventTargetInstance(Engine) { Prototype = PrototypeObject };
    }
}
