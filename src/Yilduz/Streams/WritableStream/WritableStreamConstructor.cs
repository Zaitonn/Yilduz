using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;

namespace Yilduz.Streams.WritableStream;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/WritableStream/WritableStream
/// </summary>
public sealed class WritableStreamConstructor : Constructor
{
    internal WritableStreamConstructor(Engine engine)
        : base(engine, nameof(WritableStream))
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private WritableStreamPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc />
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        return CreateInstance(arguments.At(0), arguments.At(1));
    }

    internal WritableStreamInstance CreateInstance(JsValue underlyingSink, JsValue strategy)
    {
        return new WritableStreamInstance(Engine, underlyingSink, strategy)
        {
            Prototype = PrototypeObject,
        };
    }
}
