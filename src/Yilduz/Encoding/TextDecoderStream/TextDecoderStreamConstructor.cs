using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;

namespace Yilduz.Encoding.TextDecoderStream;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/TextDecoderStream/TextDecoderStream
/// </summary>
public sealed class TextDecoderStreamConstructor : Constructor
{
    internal TextDecoderStreamConstructor(Engine engine)
        : base(engine, nameof(TextDecoderStream))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private TextDecoderStreamPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
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
