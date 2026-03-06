using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.URLs.URL;

namespace Yilduz.URLs.URLSearchParams;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/URLSearchParams/URLSearchParams
/// </summary>
public sealed class URLSearchParamsConstructor : Constructor
{
    internal URLSearchParamsConstructor(Engine engine)
        : base(engine, nameof(URLSearchParams))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private URLSearchParamsPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return new URLSearchParamsInstance(Engine, arguments.At(0)) { Prototype = PrototypeObject };
    }

    internal URLSearchParamsInstance ConstructLinkedInstance(URLInstance urlInstance)
    {
        return new(Engine, urlInstance) { Prototype = PrototypeObject };
    }
}
