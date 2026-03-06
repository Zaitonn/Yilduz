using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;

namespace Yilduz.Streams.ReadableStream;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream/ReadableStream
/// </summary>
public sealed class ReadableStreamConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    internal ReadableStreamConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(ReadableStream))
    {
        _webApiIntrinsics = webApiIntrinsics;
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private ReadableStreamPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return CreateInstance(arguments.At(0), arguments.At(1));
    }

    internal ReadableStreamInstance CreateInstance(JsValue underlyingSource, JsValue strategy)
    {
        return new(_webApiIntrinsics, Engine, underlyingSource, strategy)
        {
            Prototype = PrototypeObject,
        };
    }
}
