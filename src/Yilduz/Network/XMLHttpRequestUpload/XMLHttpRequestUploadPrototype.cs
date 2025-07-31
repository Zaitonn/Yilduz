using Jint;
using Jint.Native.Object;
using Jint.Native.Symbol;

namespace Yilduz.Network.XMLHttpRequestUpload;

internal sealed class XMLHttpRequestUploadPrototype : ObjectInstance
{
    public XMLHttpRequestUploadPrototype(Engine engine, XMLHttpRequestUploadConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(XMLHttpRequestUpload));
        SetOwnProperty("constructor", new(constructor, false, false, true));
    }
}
