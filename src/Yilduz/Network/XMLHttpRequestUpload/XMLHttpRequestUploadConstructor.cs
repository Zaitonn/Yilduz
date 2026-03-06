using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Utils;

namespace Yilduz.Network.XMLHttpRequestUpload;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequestUpload
/// </summary>
public sealed class XMLHttpRequestUploadConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    internal XMLHttpRequestUploadConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(XMLHttpRequestUpload))
    {
        _webApiIntrinsics = webApiIntrinsics;

        PrototypeObject = new(engine, this)
        {
            Prototype = _webApiIntrinsics.XMLHttpRequestEventTarget.PrototypeObject,
        };
        SetOwnProperty("prototype", new(PrototypeObject, false, false, true));
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        TypeErrorHelper.ThrowForIllegalConstructor(Engine, nameof(XMLHttpRequestUpload));
        return null;
    }

    private XMLHttpRequestUploadPrototype PrototypeObject { get; }

    internal XMLHttpRequestUploadInstance CreateInstance()
    {
        return new(Engine, _webApiIntrinsics) { Prototype = PrototypeObject };
    }
}
