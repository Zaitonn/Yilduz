using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Extensions;

namespace Yilduz.Streams.ByteLengthQueuingStrategy;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/ByteLengthQueuingStrategy/ByteLengthQueuingStrategy
/// </summary>
public sealed class ByteLengthQueuingStrategyConstructor : Constructor
{
    internal ByteLengthQueuingStrategyConstructor(Engine engine)
        : base(engine, nameof(ByteLengthQueuingStrategy))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, true));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCountForConstructor(Engine, 1, nameof(ByteLengthQueuingStrategy));
        return new ByteLengthQueuingStrategyInstance(Engine, arguments[0])
        {
            Prototype = PrototypeObject,
        };
    }

    private ByteLengthQueuingStrategyPrototype PrototypeObject { get; }
}
