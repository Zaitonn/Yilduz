using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Utils;

namespace Yilduz.Network.XMLHttpRequestUpload;

internal sealed class XMLHttpRequestUploadConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    public XMLHttpRequestUploadConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(XMLHttpRequestUpload))
    {
        _webApiIntrinsics = webApiIntrinsics;

        PrototypeObject = new(engine, this)
        {
            Prototype = _webApiIntrinsics.XMLHttpRequestEventTarget.PrototypeObject,
        };
        SetOwnProperty("prototype", new(PrototypeObject, false, false, true));
    }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        TypeErrorHelper.ThrowForIllegalConstructor(Engine, nameof(XMLHttpRequestUpload));
        return null;
    }

    public XMLHttpRequestUploadPrototype PrototypeObject { get; }

    public XMLHttpRequestUploadInstance CreateInstance()
    {
        return new(Engine, _webApiIntrinsics) { Prototype = PrototypeObject };
    }
}
