using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.URLs.URL;

namespace Yilduz.URLs.URLSearchParams;

internal sealed class URLSearchParamsConstructor : Constructor
{
    public URLSearchParamsConstructor(Engine engine)
        : base(engine, nameof(URLSearchParams))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public URLSearchParamsPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new URLSearchParamsInstance(Engine, arguments.At(0)) { Prototype = PrototypeObject };
    }

    public URLSearchParamsInstance ConstructLinkedInstance(URLInstance urlInstance)
    {
        return new(Engine, urlInstance) { Prototype = PrototypeObject };
    }
}
