using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;

namespace Yilduz.Streams.ReadableStream;

internal sealed class ReadableStreamPrototype : PrototypeBase<ReadableStreamInstance>
{
    public ReadableStreamPrototype(Engine engine, ReadableStreamConstructor constructor)
        : base(engine, nameof(ReadableStream), constructor)
    {
        RegisterProperty("locked", stream => stream.Locked);

        RegisterMethod("cancel", Cancel);
        RegisterMethod("getReader", GetReader);
        RegisterMethod("pipeTo", PipeTo, 1);
        RegisterMethod("pipeThrough", PipeThrough, 1);
        RegisterMethod("tee", Tee);
    }

    private static JsValue Cancel(ReadableStreamInstance stream, JsValue[] arguments)
    {
        var reason = arguments.At(0);
        return stream.Cancel(reason);
    }

    private static ReadableStreamReader GetReader(
        ReadableStreamInstance stream,
        JsValue[] arguments
    )
    {
        var options = arguments.At(0);

        return stream.GetReader(options);
    }

    private static JsValue PipeTo(ReadableStreamInstance stream, JsValue[] arguments)
    {
        var destination = arguments.At(0).AsObject();
        var options = arguments.At(1);

        return stream.PipeTo(destination, options);
    }

    private static JsValue PipeThrough(ReadableStreamInstance stream, JsValue[] arguments)
    {
        var transform = arguments.At(0).AsObject();
        var options = arguments.At(1);
        return stream.PipeThrough(transform, options);
    }

    private JsArray Tee(ReadableStreamInstance stream, JsValue[] arguments)
    {
        var streams = stream.Tee();
        return Engine.Intrinsics.Array.Construct([streams.Item1, streams.Item2]);
    }
}
