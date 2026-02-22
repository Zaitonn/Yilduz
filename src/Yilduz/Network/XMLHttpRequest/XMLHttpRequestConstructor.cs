using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Network.XMLHttpRequest;

internal sealed class XMLHttpRequestConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    public XMLHttpRequestConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(XMLHttpRequest))
    {
        _webApiIntrinsics = webApiIntrinsics;
        PrototypeObject = new(_engine, this)
        {
            Prototype = webApiIntrinsics.XMLHttpRequestEventTarget.PrototypeObject,
        };
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new XMLHttpRequestInstance(Engine, _webApiIntrinsics)
        {
            Prototype = PrototypeObject,
        };
    }

    public XMLHttpRequestPrototype PrototypeObject { get; }
}
