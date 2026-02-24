using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Streams.ReadableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableByteStreamController;

internal sealed class ReadableByteStreamControllerConstructor : Constructor
{
    public ReadableByteStreamControllerConstructor(Engine engine)
        : base(engine, nameof(ReadableByteStreamController))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public ReadableByteStreamControllerPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        TypeErrorHelper.Throw(Engine, "Illegal constructor");
        return null;
    }

    public ReadableByteStreamControllerInstance Construct(
        ReadableStreamInstance readableStreamInstance
    )
    {
        return new(Engine, readableStreamInstance) { Prototype = PrototypeObject };
    }
}
