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

        SetOwnProperty(
            nameof(XMLHttpRequestReadyState.Unsent).ToUpperInvariant(),
            new((int)XMLHttpRequestReadyState.Unsent, false, false, false)
        );
        SetOwnProperty(
            nameof(XMLHttpRequestReadyState.Opened).ToUpperInvariant(),
            new((int)XMLHttpRequestReadyState.Opened, false, false, false)
        );
        SetOwnProperty(
            nameof(XMLHttpRequestReadyState.Headers_Received).ToUpperInvariant(),
            new((int)XMLHttpRequestReadyState.Headers_Received, false, false, false)
        );
        SetOwnProperty(
            nameof(XMLHttpRequestReadyState.Loading).ToUpperInvariant(),
            new((int)XMLHttpRequestReadyState.Loading, false, false, false)
        );
        SetOwnProperty(
            nameof(XMLHttpRequestReadyState.Done).ToUpperInvariant(),
            new((int)XMLHttpRequestReadyState.Done, false, false, false)
        );
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
