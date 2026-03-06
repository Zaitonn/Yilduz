using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Yilduz.Extensions;
using Yilduz.Streams.ReadableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamBYOBReader;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamBYOBReader/ReadableStreamBYOBReader
/// </summary>
public sealed class ReadableStreamBYOBReaderConstructor : Constructor
{
    internal ReadableStreamBYOBReaderConstructor(Engine engine)
        : base(engine, nameof(ReadableStreamBYOBReader))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private ReadableStreamBYOBReaderPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCountForConstructor(Engine, 1, nameof(ReadableStreamBYOBReader));

        var streamArg = arguments.At(0);
        if (streamArg is not ReadableStreamInstance stream)
        {
            TypeErrorHelper.Throw(
                Engine,
                "Failed to construct 'ReadableStreamBYOBReader': parameter 1 is not of type 'ReadableStream'."
            );
            return null;
        }

        return CreateInstance(stream);
    }

    internal ReadableStreamBYOBReaderInstance CreateInstance(ReadableStreamInstance readableStream)
    {
        return new(Engine, readableStream) { Prototype = PrototypeObject };
    }
}
