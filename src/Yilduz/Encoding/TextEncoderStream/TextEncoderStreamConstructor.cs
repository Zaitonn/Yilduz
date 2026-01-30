using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Encoding.TextEncoderStream;

internal sealed class TextEncoderStreamConstructor : Constructor
{
    public TextEncoderStreamConstructor(Engine engine)
        : base(engine, nameof(TextEncoderStream))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public TextEncoderStreamPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new TextEncoderStreamInstance(Engine) { Prototype = PrototypeObject };
    }
}
