using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Yilduz.Streams.WritableStream;

namespace Yilduz.Streams.WritableStreamDefaultController;

/// <summary>
/// https://streams.spec.whatwg.org/#writablestreamdefaultcontroller
/// </summary>
public sealed partial class WritableStreamDefaultControllerInstance : ObjectInstance
{
    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-controller-signal
    /// </summary>
    public JsValue Signal => AbortController.Signal;

    internal WritableStreamDefaultControllerInstance(
        Engine engine,
        WritableStreamInstance stream,
        Function sizeAlgorithm
    )
        : base(engine)
    {
        Stream = stream;
        AbortController = engine.GetWebApiIntrinsics().AbortController.Construct();
        StrategySizeAlgorithm = sizeAlgorithm;

        ClearAlgorithms();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-controller-error
    /// </summary>
    public void Error(JsValue error)
    {
        // Let state be this.[[stream]].[[state]].
        // If state is not "writable", return.
        if (Stream?.State != WritableStreamState.Writable)
        {
            return;
        }

        // Perform ! WritableStreamDefaultControllerError(this, e).
        ErrorInternal(error);
    }
}
