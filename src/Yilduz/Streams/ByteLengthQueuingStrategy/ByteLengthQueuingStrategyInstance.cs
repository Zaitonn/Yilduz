using Jint;
using Jint.Native;
using Yilduz.Streams.QueuingStrategy;

namespace Yilduz.Streams.ByteLengthQueuingStrategy;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/ByteLengthQueuingStrategy
/// </summary>
public sealed class ByteLengthQueuingStrategyInstance : QueuingStrategyInstance
{
    internal ByteLengthQueuingStrategyInstance(Engine engine, JsValue options)
        : base(engine, options) { }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override JsValue Size(JsValue chunk)
    {
        return chunk.Get("byteLength");
    }
}
