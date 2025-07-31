using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Network.XMLHttpRequest;

internal sealed class XMLHttpRequestConstructor : Constructor
{
    public XMLHttpRequestConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(XMLHttpRequest))
    {
        PrototypeObject = new(_engine, this)
        {
            Prototype = webApiIntrinsics.XMLHttpRequestEventTarget.PrototypeObject,
        };
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new XMLHttpRequestInstance(Engine) { Prototype = PrototypeObject };
    }

    public XMLHttpRequestPrototype PrototypeObject { get; }
}
