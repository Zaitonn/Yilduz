using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Encoding.TextDecoder;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/TextDecoder/TextDecoder
/// </summary>
public sealed class TextDecoderConstructor : Constructor
{
    internal TextDecoderConstructor(Engine engine)
        : base(engine, nameof(TextDecoder))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private TextDecoderPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        var label = arguments.Length > 0 ? arguments[0] : Undefined;
        var options = arguments.Length > 1 ? arguments[1] : Undefined;

        return new TextDecoderInstance(Engine, label, options) { Prototype = PrototypeObject };
    }
}
