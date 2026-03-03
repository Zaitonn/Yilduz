using Jint;
using Jint.Native;
using Yilduz.Models;

namespace Yilduz.Streams.QueuingStrategy;

internal abstract class QueuingStrategyPrototype<T> : PrototypeBase<T>
    where T : QueuingStrategyInstance
{
    protected QueuingStrategyPrototype(Engine engine, string name, Constructor constructor)
        : base(engine, name, constructor)
    {
        RegisterMethod("size", Size, 1);
        RegisterProperty("highWaterMark", strategy => strategy.HighWaterMark);
    }

    private JsValue Size(QueuingStrategyInstance queuingStrategy, JsValue[] arguments)
    {
        return queuingStrategy.Size(arguments[0]);
    }
}
