using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Extensions;
using Yilduz.Utils;

namespace Yilduz.Streams.QueuingStrategy;

public abstract class QueuingStrategyBase : ObjectInstance
{
    private ClrFunction? _sizeAlgorithm;

    private protected QueuingStrategyBase(Engine engine, JsValue options)
        : base(engine)
    {
        var highWaterMark = options.AsObject().Get("highWaterMark");
        if (!highWaterMark.IsUndefined())
        {
            HighWaterMark = highWaterMark.AsNumber();
        }
        else
        {
            TypeErrorHelper.Throw(engine, "HighWaterMark is required for QueuingStrategy");
        }
    }

    internal ClrFunction SizeAlgorithm =>
        _sizeAlgorithm ??= new ClrFunction(
            Engine,
            nameof(Size).ToJsStyleName(),
            (_, args) => Size(args.At(0))
        );

    public double HighWaterMark { get; protected set; }

    public abstract JsValue Size(JsValue chunk);
}
