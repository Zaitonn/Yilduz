using System;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Streams.WritableStreamDefaultController;
using Yilduz.Streams.WritableStreamDefaultWriter;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStream;

/// <summary>
/// Abstract operations for WritableStream as defined in WHATWG Streams Standard
/// https://streams.spec.whatwg.org/#ws-all-abstract-ops
/// </summary>
internal static class AbstractOperations
{
    /// <summary>
    /// https://streams.spec.whatwg.org/#acquire-writable-stream-default-writer
    /// </summary>
    public static WritableStreamDefaultWriterInstance AcquireWritableStreamDefaultWriter(
        WritableStreamInstance stream
    )
    {
        var writer = stream.Engine.GetWebApiIntrinsics().WritableStreamDefaultWriter.Construct();
        SetUpWritableStreamDefaultWriter(writer, stream);
        return writer;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#create-writable-stream
    /// </summary>
    public static WritableStreamInstance CreateWritableStream(
        Func<JsValue> startAlgorithm,
        Func<JsValue, JsValue> writeAlgorithm,
        Func<JsValue> closeAlgorithm,
        Func<JsValue, JsValue> abortAlgorithm,
        double highWaterMark,
        Func<JsValue, double> sizeAlgorithm,
        Engine engine
    )
    {
        if (!IsNonNegativeNumber(highWaterMark))
        {
            throw new JavaScriptException(
                ErrorHelper.Create(engine, "RangeError", "highWaterMark must be non-negative")
            );
        }

        var stream = new WritableStreamInstance(engine);
        InitializeWritableStream(stream);

        var controller = stream
            .Engine.GetWebApiIntrinsics()
            .WritableStreamDefaultController.Construct();
        controller.ControlledWritableStream = stream;

        SetUpWritableStreamDefaultController(
            stream,
            controller,
            startAlgorithm,
            writeAlgorithm,
            closeAlgorithm,
            abortAlgorithm,
            highWaterMark,
            sizeAlgorithm
        );

        return stream;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#initialize-writable-stream
    /// </summary>
    public static void InitializeWritableStream(WritableStreamInstance stream)
    {
        stream.State = WritableStreamState.Writable;
        stream.StoredError = JsValue.Undefined;
        stream.Writer = null;
        stream.InFlightWriteRequest = null;
        stream.CloseRequest = null;
        stream.InFlightCloseRequest = null;
        stream.WriteRequests = [];
        stream.PendingAbortRequest = null;
        stream.Backpressure = false;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#is-writable-stream-locked
    /// </summary>
    public static bool IsWritableStreamLocked(WritableStreamInstance stream)
    {
        return stream.Writer != null;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#set-up-writable-stream-default-writer
    /// </summary>
    public static void SetUpWritableStreamDefaultWriter(
        WritableStreamDefaultWriterInstance writer,
        WritableStreamInstance stream
    )
    {
        if (IsWritableStreamLocked(stream) && stream.Writer != writer)
        {
            TypeErrorHelper.Throw(stream.Engine, "Stream is already locked");
        }

        writer.Stream = stream;
        stream.Writer = writer;

        var state = stream.State;
        if (state == WritableStreamState.Writable)
        {
            if (!WritableStreamCloseQueuedOrInFlight(stream) && stream.Backpressure)
            {
                writer.ReadyPromise = stream.Engine.Advanced.RegisterPromise();
            }
            else
            {
                writer.ReadyPromise = PromiseHelper.CreateResolvedPromise(
                    stream.Engine,
                    JsValue.Undefined
                );
            }
            writer.ClosedPromise = stream.Engine.Advanced.RegisterPromise();
        }
        else if (state == WritableStreamState.Erroring)
        {
            writer.ReadyPromise = PromiseHelper.CreateRejectedPromise(
                stream.Engine,
                stream.StoredError
            );
            writer.ClosedPromise = stream.Engine.Advanced.RegisterPromise();
        }
        else if (state == WritableStreamState.Closed)
        {
            writer.ReadyPromise = PromiseHelper.CreateResolvedPromise(
                stream.Engine,
                JsValue.Undefined
            );
            writer.ClosedPromise = PromiseHelper.CreateResolvedPromise(
                stream.Engine,
                JsValue.Undefined
            );
        }
        else
        {
            // errored
            var storedError = stream.StoredError;
            writer.ReadyPromise = PromiseHelper.CreateRejectedPromise(stream.Engine, storedError);
            writer.ClosedPromise = PromiseHelper.CreateRejectedPromise(stream.Engine, storedError);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-abort
    /// </summary>
    public static JsValue WritableStreamAbort(WritableStreamInstance stream, JsValue reason)
    {
        if (
            stream.State == WritableStreamState.Closed
            || stream.State == WritableStreamState.Errored
        )
        {
            return PromiseHelper.CreateResolvedPromise(stream.Engine, JsValue.Undefined).Promise;
        }

        // Signal abort on stream.[[controller]].[[abortController]] with reason
        stream.Controller?.AbortController?.Abort(reason);

        var state = stream.State;
        if (state == WritableStreamState.Closed || state == WritableStreamState.Errored)
        {
            return PromiseHelper.CreateResolvedPromise(stream.Engine, JsValue.Undefined).Promise;
        }

        if (stream.PendingAbortRequest is not null)
        {
            return stream.PendingAbortRequest.Value.Promise.Promise;
        }

        var wasAlreadyErroring = state == WritableStreamState.Erroring;
        if (wasAlreadyErroring)
        {
            reason = JsValue.Undefined;
        }

        var promise = stream.Engine.Advanced.RegisterPromise();
        stream.PendingAbortRequest = new PendingAbortRequest
        {
            Promise = promise,
            Reason = reason,
            WasAlreadyErroring = wasAlreadyErroring,
        };

        if (!wasAlreadyErroring)
        {
            WritableStreamStartErroring(stream, reason);
        }

        return promise.Promise;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-close
    /// </summary>
    public static JsValue WritableStreamClose(WritableStreamInstance stream)
    {
        var state = stream.State;
        if (state == WritableStreamState.Closed || state == WritableStreamState.Errored)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    stream.Engine,
                    ErrorHelper.Create(
                        stream.Engine,
                        "TypeError",
                        "Stream is not in a writable state"
                    )
                )
                .Promise;
        }

        if (WritableStreamCloseQueuedOrInFlight(stream))
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    stream.Engine,
                    ErrorHelper.Create(stream.Engine, "TypeError", "Stream is already being closed")
                )
                .Promise;
        }

        var promise = stream.Engine.Advanced.RegisterPromise();
        stream.CloseRequest = promise;

        var writer = stream.Writer;
        if (writer != null && stream.Backpressure && state == WritableStreamState.Writable)
        {
            writer.ReadyPromise?.Resolve(JsValue.Undefined);
        }

        WritableStreamDefaultController.AbstractOperations.WritableStreamDefaultControllerClose(
            stream.Controller!
        );
        return promise.Promise;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-close-queued-or-in-flight
    /// </summary>
    public static bool WritableStreamCloseQueuedOrInFlight(WritableStreamInstance stream)
    {
        return !(stream.CloseRequest == null && stream.InFlightCloseRequest == null);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-start-erroring
    /// </summary>
    public static void WritableStreamStartErroring(WritableStreamInstance stream, JsValue reason)
    {
        stream.StoredError = reason;
        stream.State = WritableStreamState.Erroring;

        var writer = stream.Writer;
        if (writer != null)
        {
            WritableStreamDefaultWriter.AbstractOperations.WritableStreamDefaultWriterEnsureReadyPromiseRejected(
                writer,
                reason
            );
        }

        if (
            !WritableStreamHasOperationMarkedInFlight(stream)
            && (stream.Controller?.Started ?? false)
        )
        {
            WritableStreamFinishErroring(stream);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-finish-erroring
    /// </summary>
    public static void WritableStreamFinishErroring(WritableStreamInstance stream)
    {
        stream.State = WritableStreamState.Errored;
        stream.Controller?.ErrorSteps();

        var storedError = stream.StoredError;
        foreach (var writeRequest in stream.WriteRequests)
        {
            writeRequest.Reject(storedError);
        }
        stream.WriteRequests.Clear();

        if (stream.PendingAbortRequest == null)
        {
            WritableStreamRejectCloseAndClosedPromiseIfNeeded(stream);
            return;
        }

        var abortRequest = stream.PendingAbortRequest;
        stream.PendingAbortRequest = null;

        if (abortRequest.Value.WasAlreadyErroring)
        {
            abortRequest.Value.Promise.Reject(storedError);
            WritableStreamRejectCloseAndClosedPromiseIfNeeded(stream);
            return;
        }

        var promise = stream.Controller?.AbortSteps(abortRequest.Value.Reason);

        try
        {
            promise?.UnwrapIfPromise();
            abortRequest.Value.Promise.Resolve(JsValue.Undefined);
            WritableStreamRejectCloseAndClosedPromiseIfNeeded(stream);
        }
        catch (PromiseRejectedException e)
        {
            abortRequest.Value.Promise.Reject(e.RejectedValue);
            WritableStreamRejectCloseAndClosedPromiseIfNeeded(stream);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-has-operation-marked-in-flight
    /// </summary>
    public static bool WritableStreamHasOperationMarkedInFlight(WritableStreamInstance stream)
    {
        return !(stream.InFlightWriteRequest == null && stream.InFlightCloseRequest == null);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-reject-close-and-closed-promise-if-needed
    /// </summary>
    public static void WritableStreamRejectCloseAndClosedPromiseIfNeeded(
        WritableStreamInstance stream
    )
    {
        if (stream.CloseRequest is not null)
        {
            stream.CloseRequest.Reject(stream.StoredError);
            stream.CloseRequest = null;
        }

        var writer = stream.Writer;
        if (writer?.ClosedPromise is not null)
        {
            writer?.ClosedPromise.Reject(stream.StoredError);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#set-up-writable-stream-default-controller
    /// </summary>
    public static void SetUpWritableStreamDefaultController(
        WritableStreamInstance stream,
        WritableStreamDefaultControllerInstance controller,
        Func<JsValue> startAlgorithm,
        Func<JsValue, JsValue> writeAlgorithm,
        Func<JsValue> closeAlgorithm,
        Func<JsValue, JsValue> abortAlgorithm,
        double highWaterMark,
        Func<JsValue, double> sizeAlgorithm
    )
    {
        controller.Stream = stream;
        stream.Controller = controller;

        controller.Queue = [];
        controller.QueueTotalSize = 0;
        controller.AbortController = stream
            .Engine.GetWebApiIntrinsics()
            .AbortController.Construct();
        controller.Started = false;
        controller.StrategySizeAlgorithm = sizeAlgorithm;
        controller.StrategyHWM = highWaterMark;
        controller.WriteAlgorithm = writeAlgorithm;
        controller.CloseAlgorithm = closeAlgorithm;
        controller.AbortAlgorithm = abortAlgorithm;

        var backpressure =
            WritableStreamDefaultController.AbstractOperations.WritableStreamDefaultControllerGetBackpressure(
                controller
            );
        WritableStreamUpdateBackpressure(stream, backpressure);

        var startResult = startAlgorithm();
        var startPromise = startResult.IsPromise()
            ? startResult
            : PromiseHelper.CreateResolvedPromise(stream.Engine, startResult).Promise;

        try
        {
            startPromise.UnwrapIfPromise();

            controller.Started = true;
            WritableStreamDefaultController.AbstractOperations.WritableStreamDefaultControllerAdvanceQueueIfNeeded(
                controller
            );
        }
        catch (PromiseRejectedException e)
        {
            controller.Started = true;
            WritableStreamDealWithRejection(stream, e.RejectedValue);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-update-backpressure
    /// </summary>
    public static void WritableStreamUpdateBackpressure(
        WritableStreamInstance stream,
        bool backpressure
    )
    {
        if (stream.State != WritableStreamState.Writable)
        {
            return;
        }

        if (WritableStreamCloseQueuedOrInFlight(stream))
        {
            return;
        }

        var writer = stream.Writer;
        if (writer != null && backpressure != stream.Backpressure)
        {
            if (backpressure)
            {
                writer.ReadyPromise = stream.Engine.Advanced.RegisterPromise();
            }
            else
            {
                writer.ReadyPromise?.Resolve(JsValue.Undefined);
            }
        }

        stream.Backpressure = backpressure;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-deal-with-rejection
    /// </summary>
    public static void WritableStreamDealWithRejection(WritableStreamInstance stream, JsValue error)
    {
        var state = stream.State;
        if (state == WritableStreamState.Writable)
        {
            WritableStreamStartErroring(stream, error);
            return;
        }

        WritableStreamFinishErroring(stream);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-finish-in-flight-write
    /// </summary>
    public static void WritableStreamFinishInFlightWrite(WritableStreamInstance stream)
    {
        stream.InFlightWriteRequest?.Resolve(JsValue.Undefined);
        stream.InFlightWriteRequest = null;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-finish-in-flight-write-with-error
    /// </summary>
    public static void WritableStreamFinishInFlightWriteWithError(
        WritableStreamInstance stream,
        JsValue error
    )
    {
        stream.InFlightWriteRequest?.Reject(error);
        stream.InFlightWriteRequest = null;

        if (stream.State == WritableStreamState.Writable)
        {
            WritableStreamDealWithRejection(stream, error);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-finish-in-flight-close
    /// </summary>
    public static void WritableStreamFinishInFlightClose(WritableStreamInstance stream)
    {
        stream.InFlightCloseRequest?.Resolve(JsValue.Undefined);
        stream.InFlightCloseRequest = null;
        stream.State = WritableStreamState.Closed;

        var writer = stream.Writer;
        writer?.ClosedPromise?.Resolve(JsValue.Undefined);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-finish-in-flight-close-with-error
    /// </summary>
    public static void WritableStreamFinishInFlightCloseWithError(
        WritableStreamInstance stream,
        JsValue error
    )
    {
        if (stream.InFlightCloseRequest is { } closePromise)
        {
            closePromise.Reject(error);
        }
        stream.InFlightCloseRequest = null;

        if (stream.PendingAbortRequest != null)
        {
            stream.PendingAbortRequest.Value.Promise.Reject(error);
            stream.PendingAbortRequest = null;
        }

        WritableStreamDealWithRejection(stream, error);
    }

    private static bool IsNonNegativeNumber(double value)
    {
        return !double.IsNaN(value) && value >= 0;
    }
}
