using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Extensions;

namespace Yilduz.Compression.DecompressionStream;

internal sealed class DecompressionStreamConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    public DecompressionStreamConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(DecompressionStream))
    {
        _webApiIntrinsics = webApiIntrinsics;
        PrototypeObject = new(engine, this);

        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public DecompressionStreamPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'DecompressionStream'");
        var format = arguments[0];

        return new DecompressionStreamInstance(Engine, _webApiIntrinsics, format)
        {
            Prototype = PrototypeObject,
        };
    }
}
