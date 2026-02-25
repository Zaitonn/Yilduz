using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Data.FormData;

internal sealed class FormDataConstructor : Constructor
{
    public FormDataConstructor(Engine engine)
        : base(engine, nameof(FormData))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new FormDataInstance(Engine) { Prototype = PrototypeObject };
    }

    public FormDataPrototype PrototypeObject { get; }
}
