using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Streams.TransformStream;
using Yilduz.Utils;

namespace Yilduz.Streams.TransformStreamDefaultController;

internal sealed class TransformStreamDefaultControllerConstructor : Constructor
{
    public TransformStreamDefaultControllerConstructor(Engine engine)
        : base(engine, nameof(TransformStreamDefaultController))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public TransformStreamDefaultControllerPrototype PrototypeObject { get; }

    public TransformStreamDefaultControllerInstance Construct(TransformStreamInstance stream)
    {
        return new TransformStreamDefaultControllerInstance(Engine, stream)
        {
            Prototype = PrototypeObject,
        };
    }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        TypeErrorHelper.Throw(
            Engine,
            "Failed to construct 'TransformStreamDefaultController': Illegal constructor"
        );
        return null!;
    }
}
