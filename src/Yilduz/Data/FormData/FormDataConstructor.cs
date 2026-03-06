using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Data.FormData;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/FormData/FormData
/// </summary>
public sealed class FormDataConstructor : Constructor
{
    internal FormDataConstructor(Engine engine)
        : base(engine, nameof(FormData))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new FormDataInstance(Engine) { Prototype = PrototypeObject };
    }

    private FormDataPrototype PrototypeObject { get; }
}
