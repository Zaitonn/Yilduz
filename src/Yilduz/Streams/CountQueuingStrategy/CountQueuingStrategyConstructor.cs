using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Extensions;

namespace Yilduz.Streams.CountQueuingStrategy;

internal sealed class CountQueuingStrategyConstructor : Constructor
{
    public CountQueuingStrategyConstructor(Engine engine)
        : base(engine, nameof(CountQueuingStrategy))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, true));
    }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'CountQueuingStrategy'");
        return new CountQueuingStrategyInstance(Engine, arguments[0])
        {
            Prototype = PrototypeObject,
        };
    }

    public CountQueuingStrategyPrototype PrototypeObject { get; }
}
