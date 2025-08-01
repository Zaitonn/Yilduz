using Jint;
using Jint.Native;
using Yilduz.Streams.QueuingStrategy;

namespace Yilduz.Streams.CountQueuingStrategy;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/CountQueuingStrategy
/// </summary>
public sealed class CountQueuingStrategyInstance : QueuingStrategyInstance
{
    internal CountQueuingStrategyInstance(Engine engine, JsValue options)
        : base(engine, options) { }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override JsValue Size(JsValue chunk)
    {
        return 1;
    }
}
