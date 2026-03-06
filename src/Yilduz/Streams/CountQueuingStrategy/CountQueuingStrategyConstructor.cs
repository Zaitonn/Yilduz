using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Extensions;

namespace Yilduz.Streams.CountQueuingStrategy;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/CountQueuingStrategy/CountQueuingStrategy
/// </summary>
public sealed class CountQueuingStrategyConstructor : Constructor
{
    internal CountQueuingStrategyConstructor(Engine engine)
        : base(engine, nameof(CountQueuingStrategy))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, true));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'CountQueuingStrategy'");
        return new CountQueuingStrategyInstance(Engine, arguments[0])
        {
            Prototype = PrototypeObject,
        };
    }

    private CountQueuingStrategyPrototype PrototypeObject { get; }
}
