using Jint;
using Yilduz.Streams.QueuingStrategy;

namespace Yilduz.Streams.ByteLengthQueuingStrategy;

internal sealed class ByteLengthQueuingStrategyPrototype(
    Engine engine,
    ByteLengthQueuingStrategyConstructor constructor
)
    : QueuingStrategyPrototype<ByteLengthQueuingStrategyInstance>(
        engine,
        nameof(ByteLengthQueuingStrategy),
        constructor
    ) { }
