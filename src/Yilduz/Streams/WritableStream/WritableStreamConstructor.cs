using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;

namespace Yilduz.Streams.WritableStream;

internal sealed class WritableStreamConstructor : Constructor
{
    public WritableStreamConstructor(Engine engine)
        : base(engine, nameof(WritableStream))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public WritableStreamPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return Construct(arguments.At(0), arguments.At(1));
    }

    public WritableStreamInstance Construct(JsValue underlyingSink, JsValue strategy)
    {
        return new WritableStreamInstance(Engine, underlyingSink, strategy)
        {
            Prototype = PrototypeObject,
        };
    }
}
