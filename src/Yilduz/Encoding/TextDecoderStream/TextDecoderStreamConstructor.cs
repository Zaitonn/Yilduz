using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;

namespace Yilduz.Encoding.TextDecoderStream;

internal sealed class TextDecoderStreamConstructor : Constructor
{
    public TextDecoderStreamConstructor(Engine engine)
        : base(engine, nameof(TextDecoderStream))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public TextDecoderStreamPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        var label = arguments.At(0);
        var options = arguments.At(1);

        return new TextDecoderStreamInstance(Engine, label, options)
        {
            Prototype = PrototypeObject,
        };
    }
}
