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
    internal abstract JsValue CancelSteps(JsValue reason);
    internal abstract void PullSteps(ReadRequest readRequest);
    internal abstract void ReleaseSteps();

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-default-controller-clear-algorithms
    /// </summary>
    protected internal void ClearAlgorithms()
    {
        // Set controller.[[pullAlgorithm]] to undefined.
        PullAlgorithm = null;
        // Set controller.[[cancelAlgorithm]] to undefined.
        CancelAlgorithm = null;
        // Set controller.[[strategySizeAlgorithm]] to undefined.
        StrategySizeAlgorithm = null;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-default-controller-enqueue
    /// </summary>
    internal abstract void EnqueueInternal(JsValue chunk);
}
