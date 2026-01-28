using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;

namespace Yilduz.Streams.ReadableStream;

internal sealed class ReadableStreamConstructor : Constructor
{
    public ReadableStreamConstructor(Engine engine)
        : base(engine, nameof(ReadableStream))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public ReadableStreamPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return Construct(arguments.At(0), arguments.At(1));
    }

    public ReadableStreamInstance Construct(JsValue underlyingSource, JsValue strategy)
    {
        return new ReadableStreamInstance(Engine, underlyingSource, strategy)
        {
            Prototype = PrototypeObject,
        };
    }
}
