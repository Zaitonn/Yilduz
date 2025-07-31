using Jint;
using Jint.Native.Object;
using Jint.Native.Symbol;

namespace Yilduz.Network.XMLHttpRequest;

internal sealed class XMLHttpRequestPrototype : ObjectInstance
{
    public XMLHttpRequestPrototype(Engine engine, XMLHttpRequestConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(XMLHttpRequest));
        SetOwnProperty("constructor", new(constructor, false, false, true));
    }
}
