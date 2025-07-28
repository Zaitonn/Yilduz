using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Encoding.TextDecoder;

internal sealed class TextDecoderConstructor : Constructor
{
    public TextDecoderConstructor(Engine engine)
        : base(engine, nameof(TextDecoder))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public TextDecoderPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        var label = arguments.Length > 0 ? arguments[0] : JsValue.Undefined;
        var options = arguments.Length > 1 ? arguments[1] : JsValue.Undefined;

        return new TextDecoderInstance(Engine, label, options) { Prototype = PrototypeObject };
    }
}
