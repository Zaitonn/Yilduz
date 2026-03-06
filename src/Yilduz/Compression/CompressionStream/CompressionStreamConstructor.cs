using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Extensions;

namespace Yilduz.Compression.CompressionStream;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/CompressionStream/CompressionStream
/// </summary>
public sealed class CompressionStreamConstructor : Constructor
{
    private readonly WebApiIntrinsics _webApiIntrinsics;

    internal CompressionStreamConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(CompressionStream))
    {
        _webApiIntrinsics = webApiIntrinsics;
        PrototypeObject = new(engine, this);

        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    private CompressionStreamPrototype PrototypeObject { get; }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCountForConstructor(Engine, 1, nameof(CompressionStream));
        var format = arguments[0];

        return new CompressionStreamInstance(Engine, _webApiIntrinsics, format)
        {
            Prototype = PrototypeObject,
        };
    }
}
