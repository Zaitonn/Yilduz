using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;

namespace Yilduz.Streams.TransformStream;

internal sealed class TransformStreamConstructor : Constructor
{
    public TransformStreamConstructor(Engine engine)
        : base(engine, nameof(TransformStream))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public TransformStreamPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        var transformer = arguments.At(0);
        var writableStrategy = arguments.At(1);
        var readableStrategy = arguments.At(2);

        return Construct(transformer, writableStrategy, readableStrategy);
    }

    public TransformStreamInstance Construct(
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
