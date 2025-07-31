using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Yilduz.Streams.Queue;
using Yilduz.Streams.ReadableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamDefaultController;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamDefaultController
/// </summary>
public sealed partial class ReadableStreamDefaultControllerInstance
    : ObjectInstance,
        IQueueEntriesContainer
{
    /// <summary>
    /// Internal slots for ReadableStreamDefaultController
    /// https://streams.spec.whatwg.org/#rs-default-controller-internal-slots
    /// </summary>
    internal ReadableStreamDefaultControllerInstance(
        Engine engine,
        ReadableStreamInstance stream,
        Function? pullAlgorithm,
        Function? cancelAlgorithm,
        double highWaterMark,
        Function? sizeAlgorithm
    )
        : base(engine)
    {
        Stream = stream;
        PullAlgorithm = pullAlgorithm;
        CancelAlgorithm = cancelAlgorithm;
        StrategySizeAlgorithm = sizeAlgorithm;
        StrategyHWM = highWaterMark;
        this.ResetQueue();
        CloseRequested = false;
        PullAgain = false;
        Pulling = false;
        Started = false;
    }

    /// <summary>
    /// Returns the desired size to fill the controlled stream's internal queue.
    /// https://streams.spec.whatwg.org/#rs-default-controller-desired-size
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamDefaultController/desiredSize
    /// </summary>
    public double? DesiredSize
    {
        get
        {
            // Let state be controller.[[stream]].[[state]].

            // If state is "errored", return null.
            if (Stream.State == ReadableStreamState.Errored)
            {
                return null;
            }

            // If state is "closed", return 0.
            if (Stream.State == ReadableStreamState.Closed)
            {
                return 0;
            }

            // Return controller.[[strategyHWM]] − controller.[[queueTotalSize]].
            return StrategyHWM - QueueTotalSize;
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#rs-default-controller-close
    /// <br />
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamDefaultController/close
    /// </summary>
    public void Close()
    {
        // If ! ReadableStreamDefaultControllerCanCloseOrEnqueue(this) is false, throw a TypeError exception.
        if (!CanCloseOrEnqueue())
        {
            TypeErrorHelper.Throw(Engine, "Cannot close stream");
        }

        // Perform ! ReadableStreamDefaultControllerClose(this).
        CloseInternal();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#rs-default-controller-enqueue
    /// <br />
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamDefaultController/enqueue
    /// </summary>
    public void Enqueue(JsValue chunk)
    {
        // If ! ReadableStreamDefaultControllerCanCloseOrEnqueue(this) is false, throw a TypeError exception.
        if (!CanCloseOrEnqueue())
        {
            TypeErrorHelper.Throw(Engine, "Cannot enqueue chunk");
        }

        // Perform ? ReadableStreamDefaultControllerEnqueue(this, chunk).
        EnqueueInternal(chunk);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#rs-default-controller-error
    /// <br />
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableStreamDefaultController/error
    /// </summary>
    public void Error(JsValue error)
    {
        // Perform ! ReadableStreamDefaultControllerError(this, e).
        ErrorInternal(error);
    }
}
