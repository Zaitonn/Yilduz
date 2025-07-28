using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Encoding.TextEncoder;

internal sealed class TextEncoderConstructor : Constructor
{
    public TextEncoderConstructor(Engine engine)
        : base(engine, nameof(TextEncoder))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public TextEncoderPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new TextEncoderInstance(Engine) { Prototype = PrototypeObject };
    }
}
