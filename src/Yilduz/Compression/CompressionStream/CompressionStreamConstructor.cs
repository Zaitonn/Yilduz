using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Extensions;

namespace Yilduz.Compression.CompressionStream;

internal sealed class CompressionStreamConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    public CompressionStreamConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(CompressionStream))
    {
        _webApiIntrinsics = webApiIntrinsics;
        PrototypeObject = new(engine, this);

        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public CompressionStreamPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'CompressionStream'");
        var format = arguments[0];

        return new CompressionStreamInstance(Engine, _webApiIntrinsics, format)
        {
            Prototype = PrototypeObject,
        };
    }
}
