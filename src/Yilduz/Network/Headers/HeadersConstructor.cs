using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;

namespace Yilduz.Network.Headers;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/Headers/Headers
/// </summary>
public class HeadersConstructor : Constructor
{
    internal HeadersConstructor(Engine engine)
        : base(engine, nameof(Headers))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    internal HeadersPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return CreateInstance(arguments.At(0), Guard.None);
    }

    internal HeadersInstance CreateInstance(JsValue init, Guard guard)
    {
        return new HeadersInstance(Engine, init, guard) { Prototype = PrototypeObject };
    }

    internal HeadersInstance CreateInstance(HeaderList list, Guard guard)
    {
        return new HeadersInstance(Engine, list, guard) { Prototype = PrototypeObject };
    }
}
