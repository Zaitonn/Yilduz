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
        if (strategy is QueuingStrategyInstance queuingStrategy)
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
        // If strategy["size"] does not exist, return an algorithm that returns 1.
        if (strategy.IsObject())
        {
            var sizeProperty = strategy.Get("size");
            if (!sizeProperty.IsUndefined())
            {
                // Return an algorithm that performs the following steps, taking a chunk argument:
                //   Return the result of invoking strategy["size"] with argument list « chunk ».
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
        // If strategy["highWaterMark"] does not exist, return defaultHWM.
        if (strategy.IsObject())
        {
            var highWaterMarkProperty = strategy.Get("highWaterMark");
            if (!highWaterMarkProperty.IsUndefined())
            {
                // Let highWaterMark be strategy["highWaterMark"].
                var number = highWaterMarkProperty.AsNumber();

                // If highWaterMark is NaN or highWaterMark < 0, throw a RangeError exception.
                // Return highWaterMark.
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
