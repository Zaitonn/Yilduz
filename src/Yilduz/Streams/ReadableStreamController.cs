using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Yilduz.Streams.ReadableStream;

namespace Yilduz.Streams;

public abstract class ReadableStreamController : ObjectInstance
{
    private protected ReadableStreamController(Engine engine, ReadableStreamInstance stream)
        : base(engine)
    {
        Stream = stream;
    }

    internal abstract JsValue CancelSteps(JsValue reason);
    internal abstract void PullSteps(ReadRequest readRequest);
    internal abstract void ReleaseSteps();
    internal ReadableStreamInstance Stream { get; private set; }
    internal Function? StrategySizeAlgorithm { get; set; }
    internal double StrategyHWM { get; set; }
    internal bool CloseRequested { get; set; }
    internal bool Started { get; set; }
    internal bool Pulling { get; set; }
    internal bool PullAgain { get; set; }
    internal Function? PullAlgorithm { get; set; }
    internal Function? CancelAlgorithm { get; set; }

    internal abstract void ErrorInternal(JsValue error);
    internal abstract void CloseInternal();
    internal abstract void CallPullIfNeeded();
}
