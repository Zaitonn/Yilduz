using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Extensions;
using Yilduz.Streams.ReadableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamBYOBReader;

internal sealed class ReadableStreamBYOBReaderConstructor : Constructor
{
    public ReadableStreamBYOBReaderConstructor(Engine engine)
        : base(engine, nameof(ReadableStreamBYOBReader))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public ReadableStreamBYOBReaderPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'ReadableStreamBYOBReader'");

        var streamArg = arguments.At(0);
        if (streamArg is not ReadableStreamInstance stream)
        {
            TypeErrorHelper.Throw(
                Engine,
                "Failed to construct 'ReadableStreamBYOBReader': parameter 1 is not of type 'ReadableStream'."
            );
            return null;
        }

        return Construct(stream);
    }

    public ReadableStreamBYOBReaderInstance Construct(ReadableStreamInstance readableStream)
    {
        return new(Engine, readableStream) { Prototype = PrototypeObject };
    }
}
