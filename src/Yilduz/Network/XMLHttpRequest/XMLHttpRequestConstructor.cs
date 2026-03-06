using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Network.XMLHttpRequest;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/XMLHttpRequest
/// </summary>
public sealed class XMLHttpRequestConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    internal XMLHttpRequestConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(XMLHttpRequest))
    {
        _webApiIntrinsics = webApiIntrinsics;
        PrototypeObject = new(_engine, this)
        {
            Prototype = webApiIntrinsics.XMLHttpRequestEventTarget.PrototypeObject,
        };

        SetOwnProperty(
            nameof(XMLHttpRequestReadyState.UNSENT),
            new((int)XMLHttpRequestReadyState.UNSENT, false, false, false)
        );
        SetOwnProperty(
            nameof(XMLHttpRequestReadyState.OPENED),
            new((int)XMLHttpRequestReadyState.OPENED, false, false, false)
        );
        SetOwnProperty(
            nameof(XMLHttpRequestReadyState.HEADERS_RECEIVED),
            new((int)XMLHttpRequestReadyState.HEADERS_RECEIVED, false, false, false)
        );
        SetOwnProperty(
            nameof(XMLHttpRequestReadyState.LOADING),
            new((int)XMLHttpRequestReadyState.LOADING, false, false, false)
        );
        SetOwnProperty(
            nameof(XMLHttpRequestReadyState.DONE),
            new((int)XMLHttpRequestReadyState.DONE, false, false, false)
        );
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new XMLHttpRequestInstance(Engine, _webApiIntrinsics)
        {
            Prototype = PrototypeObject,
        };
    }

    private XMLHttpRequestPrototype PrototypeObject { get; }
}
