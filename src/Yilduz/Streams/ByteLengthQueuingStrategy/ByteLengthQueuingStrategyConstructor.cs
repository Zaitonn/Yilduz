using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Extensions;

namespace Yilduz.Streams.ByteLengthQueuingStrategy;

internal sealed class ByteLengthQueuingStrategyConstructor : Constructor
{
    public ByteLengthQueuingStrategyConstructor(Engine engine)
        : base(engine, nameof(ByteLengthQueuingStrategy))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, true));
    }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'ByteLengthQueuingStrategy'");
        return new ByteLengthQueuingStrategyInstance(Engine, arguments[0])
        {
            Prototype = PrototypeObject,
        };
    }

    public ByteLengthQueuingStrategyPrototype PrototypeObject { get; }
}
