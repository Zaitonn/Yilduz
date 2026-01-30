using System;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Streams.ReadableStreamDefaultController;
using Yilduz.Utils;

namespace Yilduz.Streams.TransformStreamDefaultController;

/// <summary>
/// Internal methods for TransformStreamDefaultControllerInstance
/// </summary>
public sealed partial class TransformStreamDefaultControllerInstance
{
    /// <summary>
    /// TransformStreamDefaultControllerError
    /// <br/>
    /// https://streams.spec.whatwg.org/#transform-stream-default-controller-error
    /// </summary>
    private void ErrorInternal(JsValue e)
    {
        if (Stream is null)
        {
            throw new JavaScriptException(
                ErrorHelper.Create(Engine, "TypeError", "Controller has no associated stream")
            );
        }

        // Perform ! ReadableStreamDefaultControllerError(stream.[[readable]].[[controller]], e).
        var readableController = (ReadableStreamDefaultControllerInstance)
            Stream.Readable.Controller;
        readableController.Error(e);

        Stream.ErrorWritableAndUnblockWrite(e);
    }

    /// <summary>
    /// TransformStreamDefaultControllerEnqueue
    /// <br/>
    /// https://streams.spec.whatwg.org/#transform-stream-default-controller-enqueue
    /// </summary>
    private void EnqueueInternal(JsValue chunk)
    {
        // Let stream be controller.[[stream]].
        var stream =
            Stream
            ?? throw new JavaScriptException(
                ErrorHelper.Create(Engine, "TypeError", "Controller has no associated stream")
            );

        // Let readableController be stream.[[readable]].[[controller]].
        var readableController = (ReadableStreamDefaultControllerInstance)
            stream.Readable.Controller;

        // If ! ReadableStreamDefaultControllerCanCloseOrEnqueue(readableController) is false, throw a TypeError exception.
        if (!readableController.CanCloseOrEnqueue())
        {
            throw new JavaScriptException(
                ErrorHelper.Create(Engine, "TypeError", "Cannot enqueue chunk")
            );
        }

        try
        {
            // Let enqueueResult be ReadableStreamDefaultControllerEnqueue(readableController, chunk).
            readableController.Enqueue(chunk);

            // Let backpressure be ! ReadableStreamDefaultControllerHasBackpressure(readableController).
            var backpressure = readableController.HasBackpressure();

            // If backpressure is not stream.[[backpressure]],
            if (backpressure != stream.Backpressure)
            {
                // Assert: backpressure is true.
                // if (!backpressure)
                // {
                //     throw new InvalidOperationException("Backpressure should be true");
                // }

                // Perform ! TransformStreamSetBackpressure(stream, true).
                stream.SetBackpressure(backpressure);
            }
        }
        catch (JavaScriptException e)
        {
            // If enqueueResult is an abrupt completion,
            // Perform ! TransformStreamErrorWritableAndUnblockWrite(stream, enqueueResult.[[Value]]).
            stream.ErrorWritableAndUnblockWrite(e.Error);

            // Throw stream.[[readable]].[[storedError]].
            throw new JavaScriptException(stream.Readable.StoredError);
        }
    }

    /// <summary>
    /// TransformStreamDefaultControllerTerminate
    /// <br/>
    /// https://streams.spec.whatwg.org/#transform-stream-default-controller-terminate
    /// </summary>
    private void TerminateInternal()
    {
        // Let stream be controller.[[stream]].
        var stream =
            Stream
            ?? throw new JavaScriptException(
                ErrorHelper.Create(Engine, "TypeError", "Controller has no associated stream")
            );

        // Let readableController be stream.[[readable]].[[controller]].
        var readableController =
            (ReadableStreamDefaultControllerInstance)stream.Readable.Controller
            ?? throw new JavaScriptException(
                ErrorHelper.Create(Engine, "TypeError", "Stream readable side has no controller")
            );

        // Perform ! ReadableStreamDefaultControllerClose(readableController).
        readableController.CloseInternal();

        // Let error be a TypeError exception indicating that the stream has been terminated.
        var error = ErrorHelper.Create(Engine, "TypeError", "Transform stream has been terminated");

        // Perform ! TransformStreamErrorWritableAndUnblockWrite(stream, error).
        stream?.ErrorWritableAndUnblockWrite(error);
    }

    /// <summary>
    /// TransformStreamDefaultControllerClearAlgorithms
    /// <br/>
    /// https://streams.spec.whatwg.org/#transform-stream-default-controller-clear-algorithms
    /// </summary>
    internal void ClearAlgorithms()
    {
        // Set controller.[[transformAlgorithm]] to undefined.
        TransformAlgorithm = null;

        // Set controller.[[flushAlgorithm]] to undefined.
        FlushAlgorithm = null;

        // Set controller.[[cancelAlgorithm]] to undefined.
        CancelAlgorithm = null;
    }

    /// <summary>
    /// TransformStreamDefaultControllerPerformTransform
    /// <br/>
    /// https://streams.spec.whatwg.org/#transform-stream-default-controller-perform-transform
    /// </summary>
    internal JsValue PerformTransform(JsValue chunk)
    {
        // Let transformPromise be the result of performing controller.[[transformAlgorithm]], passing chunk.
        if (TransformAlgorithm is null)
        {
            throw new InvalidOperationException("Transform algorithm is not set");
        }

        try
        {
            // Return the result of reacting to transformPromise with the following rejection steps given the argument r:
            var transformPromise = TransformAlgorithm.Call(Undefined, [chunk]);
            return transformPromise.IsPromise()
                ? transformPromise.Then(
                    onFulfilled: null,
                    onRejected: (r) =>
                    {
                        // Perform ! TransformStreamError(controller.[[stream]], r).
                        Stream?.ErrorInternal(r);

                        return r;
                    }
                )
                : transformPromise;
        }
        catch (JavaScriptException e)
        {
            // Perform ! TransformStreamError(controller.[[stream]], r).
            Stream?.ErrorInternal(e.Error);
            throw;
        }
    }
}
