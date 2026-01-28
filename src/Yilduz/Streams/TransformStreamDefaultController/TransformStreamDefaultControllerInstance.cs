using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Native.Promise;
using Yilduz.Streams.ReadableStreamDefaultController;
using Yilduz.Streams.TransformStream;

namespace Yilduz.Streams.TransformStreamDefaultController;

/// <summary>
/// TransformStreamDefaultController implementation according to WHATWG Streams Standard
/// https://streams.spec.whatwg.org/#transformstreamdefaultcontroller
/// </summary>
public sealed partial class TransformStreamDefaultControllerInstance : ObjectInstance
{
    /// <summary>
    /// Returns the desired size to fill the controlled stream's internal queue.
    /// https://streams.spec.whatwg.org/#transformstreamdefaultcontroller-desiredsize
    /// </summary>
    public double? DesiredSize
    {
        get
        {
            // Let readableController be this.[[stream]].[[readable]].[[controller]].
            var readableController =
                Stream?.Readable.Controller as ReadableStreamDefaultControllerInstance;

            // Return ! ReadableStreamDefaultControllerGetDesiredSize(readableController).
            return readableController?.DesiredSize;
        }
    }

    /// <summary>
    /// A promise-returning algorithm, taking one argument (the reason for cancellation), which communicates a requested cancellation to the transformer
    /// https://streams.spec.whatwg.org/#transformstreamdefaultcontroller-cancelalgorithm
    /// </summary>
    internal Function? CancelAlgorithm { get; set; }

    /// <summary>
    /// A promise which resolves on completion of either the [[cancelAlgorithm]] or the [[flushAlgorithm]]
    /// https://streams.spec.whatwg.org/#transformstreamdefaultcontroller-finishpromise
    /// </summary>
    internal ManualPromise? FinishPromise { get; set; }

    /// <summary>
    /// A promise-returning algorithm which communicates a requested close to the transformer
    /// https://streams.spec.whatwg.org/#transformstreamdefaultcontroller-flushalgorithm
    /// </summary>
    internal Function? FlushAlgorithm { get; set; }

    /// <summary>
    /// The TransformStream instance controlled
    /// https://streams.spec.whatwg.org/#transformstreamdefaultcontroller-stream
    /// </summary>
    internal TransformStreamInstance? Stream { get; set; }

    /// <summary>
    /// A promise-returning algorithm, taking one argument (the chunk to transform), which requests the transformer perform its transformation
    /// https://streams.spec.whatwg.org/#transformstreamdefaultcontroller-transformalgorithm
    /// </summary>
    internal Function? TransformAlgorithm { get; set; }

    internal TransformStreamDefaultControllerInstance(Engine engine, TransformStreamInstance stream)
        : base(engine)
    {
        Stream = stream;
    }

    /// <summary>
    /// Enqueues the given chunk chunk in the readable side of the controlled transform stream.
    /// https://streams.spec.whatwg.org/#transformstreamdefaultcontroller-enqueue
    /// </summary>
    public void Enqueue(JsValue chunk)
    {
        // Perform ? TransformStreamDefaultControllerEnqueue(this, chunk).
        EnqueueInternal(chunk);
    }

    /// <summary>
    /// Errors both the readable side and the writable side of the controlled transform stream, making all future interactions with it fail with the given error e.
    /// https://streams.spec.whatwg.org/#transformstreamdefaultcontroller-error
    /// </summary>
    public void Error(JsValue error)
    {
        // Perform ? TransformStreamDefaultControllerError(this, e).
        ErrorInternal(error);
    }

    /// <summary>
    /// Closes the readable side and errors the writable side of the controlled transform stream.
    /// https://streams.spec.whatwg.org/#transformstreamdefaultcontroller-terminate
    /// </summary>
    public void Terminate()
    {
        // Perform ? TransformStreamDefaultControllerTerminate(this).
        TerminateInternal();
    }
}
