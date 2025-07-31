using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Utils;

namespace Yilduz.Network.XMLHttpRequestEventTarget;

internal sealed class XMLHttpRequestEventTargetConstructor : Constructor
{
    public XMLHttpRequestEventTargetConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(XMLHttpRequestEventTarget))
    {
        PrototypeObject = new(_engine, this)
        {
            Prototype = webApiIntrinsics.EventTarget.PrototypeObject,
        };
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        TypeErrorHelper.Throw(
            Engine,
            "Failed to construct 'XMLHttpRequestEventTarget': Illegal constructor"
        );
        return null!;
    }

    public XMLHttpRequestEventTargetPrototype PrototypeObject { get; }
}
