using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Streams.ReadableByteStreamController;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamBYOBRequest;

internal sealed class ReadableStreamBYOBRequestConstructor : Constructor
{
    public ReadableStreamBYOBRequestConstructor(Engine engine)
        : base(engine, nameof(ReadableStreamBYOBRequest))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public ReadableStreamBYOBRequestPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        TypeErrorHelper.Throw(Engine, "Illegal constructor");
        return null;
    }

    internal ReadableStreamBYOBRequestInstance Construct(
        ReadableByteStreamControllerInstance controller,
        JsValue view
    )
    {
        return new(Engine, controller, view) { Prototype = PrototypeObject };
    }
}
