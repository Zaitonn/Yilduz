using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;

namespace Yilduz.Streams.TransformStream;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/TransformStream/TransformStream
/// </summary>
public sealed class TransformStreamConstructor : Constructor
{
    internal TransformStreamConstructor(Engine engine)
        : base(engine, nameof(TransformStream))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private TransformStreamPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        var transformer = arguments.At(0);
        var writableStrategy = arguments.At(1);
        var readableStrategy = arguments.At(2);

        return CreateInstance(transformer, writableStrategy, readableStrategy);
    }

    internal TransformStreamInstance CreateInstance(
        JsValue transformer,
        JsValue writableStrategy,
        JsValue readableStrategy
    )
    {
        return new TransformStreamInstance(Engine, transformer, writableStrategy, readableStrategy)
        {
            Prototype = PrototypeObject,
        };
    }
}
