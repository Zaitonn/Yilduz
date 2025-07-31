using Jint;
using Yilduz.Streams.QueuingStrategy;

namespace Yilduz.Streams.CountQueuingStrategy;

internal sealed class CountQueuingStrategyPrototype(
    Engine engine,
    CountQueuingStrategyConstructor constructor
)
    : QueuingStrategyPrototype<CountQueuingStrategyInstance>(
        engine,
        nameof(CountQueuingStrategy),
        constructor
    ) { }
