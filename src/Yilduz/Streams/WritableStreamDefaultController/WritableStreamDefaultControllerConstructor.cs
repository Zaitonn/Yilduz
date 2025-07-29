using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStreamDefaultController;

internal sealed class WritableStreamDefaultControllerConstructor : Constructor
{
    public WritableStreamDefaultControllerConstructor(Engine engine)
        : base(engine, "WritableStreamDefaultController")
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public WritableStreamDefaultControllerPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        TypeErrorHelper.Throw(Engine, "Illegal constructor");
        return null!;
    }

    public WritableStreamDefaultControllerInstance Construct()
    {
        return new(Engine) { Prototype = PrototypeObject };
    }
}
