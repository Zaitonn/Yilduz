using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Encoding.TextEncoder;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/TextEncoder/TextEncoder
/// </summary>
public sealed class TextEncoderConstructor : Constructor
{
    internal TextEncoderConstructor(Engine engine)
        : base(engine, nameof(TextEncoder))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private TextEncoderPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new TextEncoderInstance(Engine) { Prototype = PrototypeObject };
    }
}
