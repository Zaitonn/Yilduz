using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;

namespace Yilduz.Network.Headers;

internal class HeadersConstructor : Constructor
{
    public HeadersConstructor(Engine engine)
        : base(engine, nameof(Headers))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public HeadersPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return CreateInstance(arguments.At(0), Guard.None);
    }

    public HeadersInstance CreateInstance(JsValue init, Guard guard)
    {
        return new HeadersInstance(Engine, init, guard) { Prototype = PrototypeObject };
    }

    public HeadersInstance CreateInstance(HeaderList list, Guard guard)
    {
        return new HeadersInstance(Engine, list, guard) { Prototype = PrototypeObject };
    }
}
