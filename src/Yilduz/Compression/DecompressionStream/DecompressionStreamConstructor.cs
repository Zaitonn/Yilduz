using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Extensions;

namespace Yilduz.Compression.DecompressionStream;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/DecompressionStream/DecompressionStream
/// </summary>
public sealed class DecompressionStreamConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    internal DecompressionStreamConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(DecompressionStream))
    {
        _webApiIntrinsics = webApiIntrinsics;
        PrototypeObject = new(engine, this);

        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private DecompressionStreamPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCountForConstructor(Engine, 1, nameof(DecompressionStream));
        var format = arguments[0];

        return new DecompressionStreamInstance(Engine, _webApiIntrinsics, format)
        {
            Prototype = PrototypeObject,
        };
    }
}
