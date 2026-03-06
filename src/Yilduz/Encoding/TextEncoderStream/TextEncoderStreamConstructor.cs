using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Encoding.TextEncoderStream;

/// <summary>
/// https://dev.mozilla.org/en-US/docs/Web/API/TextEncoderStream/TextEncoderStream
/// </summary>
public sealed class TextEncoderStreamConstructor : Constructor
{
    internal TextEncoderStreamConstructor(Engine engine)
        : base(engine, nameof(TextEncoderStream))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private TextEncoderStreamPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new TextEncoderStreamInstance(Engine) { Prototype = PrototypeObject };
    }
}
