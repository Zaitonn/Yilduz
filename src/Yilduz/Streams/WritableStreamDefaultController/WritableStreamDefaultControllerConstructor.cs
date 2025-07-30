using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStreamDefaultController;

internal sealed class WritableStreamDefaultControllerConstructor : Constructor
{
    public WritableStreamDefaultControllerConstructor(Engine engine)
        : base(engine, "WritableStreamDefaultController")
    {
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
    }

    public WritableStreamDefaultControllerPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        TypeErrorHelper.Throw(Engine, "Illegal constructor");
        return null!;
    }

    public WritableStreamDefaultControllerInstance Construct(
        WritableStreamInstance writableStreamInstance,
        Function writeAlgorithm,
        Function closeAlgorithm,
        Function abortAlgorithm,
        double highWaterMark,
        Function sizeAlgorithm
    )
    {
        return new(Engine, writableStreamInstance)
        {
            Prototype = PrototypeObject,
            Queue = [],
            QueueTotalSize = 0,
            Started = false,
            StrategySizeAlgorithm = sizeAlgorithm,
            StrategyHWM = highWaterMark,
            WriteAlgorithm = writeAlgorithm,
            CloseAlgorithm = closeAlgorithm,
            AbortAlgorithm = abortAlgorithm,
        };
    }
}
