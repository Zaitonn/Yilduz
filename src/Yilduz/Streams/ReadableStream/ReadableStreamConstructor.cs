using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;

namespace Yilduz.Streams.ReadableStream;

internal sealed class ReadableStreamConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    public ReadableStreamConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(ReadableStream))
    {
        _webApiIntrinsics = webApiIntrinsics;
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
        return new(_webApiIntrinsics, Engine, underlyingSource, strategy)
        {
            Prototype = PrototypeObject,
        };
    }
}
