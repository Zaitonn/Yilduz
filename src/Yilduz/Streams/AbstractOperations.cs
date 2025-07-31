using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Streams.QueuingStrategy;
using Yilduz.Utils;

namespace Yilduz.Streams;

internal static class AbstractOperations
{
    public static (double, Function) ExtractQueuingStrategy(
        Engine engine,
        JsValue strategy,
        double defaultHWM
    )
    {
        if (strategy is QueuingStrategyBase queuingStrategy)
        {
            return (queuingStrategy.HighWaterMark, queuingStrategy.SizeAlgorithm);
        }

        return (
            ExtractHighWaterMark(engine, strategy, defaultHWM),
            ExtractSizeAlgorithm(engine, strategy)
        );
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#extract-size-algorithm
    /// </summary>
    public static Function ExtractSizeAlgorithm(Engine engine, JsValue strategy)
    {
        if (strategy.IsObject())
        {
            var sizeProperty = strategy.Get("size");
            if (!sizeProperty.IsUndefined())
            {
                return sizeProperty.AsFunctionInstance();
            }
        }

        return new ClrFunction(engine, string.Empty, (_, _) => 1);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#extract-high-water-mark
    /// </summary>
    public static double ExtractHighWaterMark(Engine engine, JsValue strategy, double defaultHWM)
    {
        if (strategy.IsObject())
        {
            var highWaterMarkProperty = strategy.Get("highWaterMark");
            if (!highWaterMarkProperty.IsUndefined())
            {
                var number = highWaterMarkProperty.AsNumber();

                return double.IsNaN(number) || number < 0
                    ? throw new JavaScriptException(
                        ErrorHelper.Create(engine, "RangeError", "Invalid highWaterMark value")
                    )
                    : number;
            }
        }

        return defaultHWM;
    }
}
