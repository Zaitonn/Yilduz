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

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        TypeErrorHelper.ThrowForIllegalConstructor(
            Engine,
            nameof(TransformStreamDefaultController)
        );
        return null;
    }

    public TransformStreamDefaultControllerInstance CreateInstance(TransformStreamInstance stream)
    {
        return new TransformStreamDefaultControllerInstance(Engine, stream)
        {
            Prototype = PrototypeObject,
        };
    }
}
