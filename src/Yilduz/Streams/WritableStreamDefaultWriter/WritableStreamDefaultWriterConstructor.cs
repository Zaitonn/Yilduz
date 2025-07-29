using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Extensions;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStreamDefaultWriter;

internal sealed class WritableStreamDefaultWriterConstructor : Constructor
{
    public WritableStreamDefaultWriterConstructor(Engine engine)
        : base(engine, "WritableStreamDefaultWriter")
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public WritableStreamDefaultWriterPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'WritableStreamDefaultWriter'");

        var streamArg = arguments.At(0);
        if (streamArg is not WritableStreamInstance stream)
        {
            TypeErrorHelper.Throw(
                Engine,
                "Failed to construct 'WritableStreamDefaultWriter': parameter 1 is not of type 'WritableStream'."
            );
            return null!;
        }

        return stream.GetWriter();
    }

    public WritableStreamDefaultWriterInstance Construct()
    {
        return new(Engine) { Prototype = PrototypeObject };
    }
}
