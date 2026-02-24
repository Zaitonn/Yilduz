using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Streams.ReadableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamDefaultReader;

internal sealed class ReadableStreamDefaultReaderConstructor : Constructor
{
    public ReadableStreamDefaultReaderConstructor(Engine engine)
        : base(engine, nameof(ReadableStreamDefaultReader))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public ReadableStreamDefaultReaderPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        TypeErrorHelper.Throw(Engine, "Illegal constructor");
        return null;
    }

    public ReadableStreamDefaultReaderInstance Construct(ReadableStreamInstance readableStream)
    {
        return new(Engine, readableStream) { Prototype = PrototypeObject };
    }
}
