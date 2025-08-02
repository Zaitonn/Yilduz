using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Yilduz.Streams.ReadableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamDefaultController;

internal sealed class ReadableStreamDefaultControllerConstructor : Constructor
{
    public ReadableStreamDefaultControllerConstructor(Engine engine)
        : base(engine, nameof(ReadableStreamDefaultController))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public ReadableStreamDefaultControllerPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        TypeErrorHelper.Throw(Engine, "Illegal constructor");
        return null!;
    }

    public ReadableStreamDefaultControllerInstance Construct(
        ReadableStreamInstance readableStreamInstance,
        double highWaterMark,
        Function? sizeAlgorithm
    )
    {
        return new(Engine, readableStreamInstance, highWaterMark, sizeAlgorithm)
        {
            Prototype = PrototypeObject,
        };
    }
}
