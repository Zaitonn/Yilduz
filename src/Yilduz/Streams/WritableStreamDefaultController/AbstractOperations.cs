using System;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStreamDefaultController;

internal static class AbstractOperations
{
    public static void SetUpWritableStreamDefaultControllerFromUnderlyingSink(
        WritableStreamInstance stream,
        JsValue underlyingSink,
        object? underlyingSinkDict,
        double highWaterMark,
        Func<JsValue, double> sizeAlgorithm
    )
    {
        var controller = stream
            .Engine.GetWebApiIntrinsics()
            .WritableStreamDefaultController.Construct();

        controller.ControlledWritableStream = stream;

        Func<JsValue> startAlgorithm = () => JsValue.Undefined;
        Func<JsValue, JsValue> writeAlgorithm = _ =>
            PromiseHelper.CreateResolvedPromise(stream.Engine, JsValue.Undefined).Promise;
        Func<JsValue> closeAlgorithm = () =>
            PromiseHelper.CreateResolvedPromise(stream.Engine, JsValue.Undefined).Promise;
        Func<JsValue, JsValue> abortAlgorithm = _ =>
            PromiseHelper.CreateResolvedPromise(stream.Engine, JsValue.Undefined).Promise;

        if (underlyingSink.IsObject())
        {
            var sinkObj = underlyingSink.AsObject();

            // Extract start method
            var start = sinkObj.Get("start");
            if (!start.IsUndefined())
            {
                startAlgorithm = () =>
                {
                    try
                    {
                        return stream.Engine.Call(
                            start,
                            underlyingSink,
                            [JsValue.FromObject(stream.Engine, controller)]
                        );
                    }
                    catch (Exception ex)
                    {
                        throw new JavaScriptException(
                            ErrorHelper.Create(stream.Engine, "TypeError", ex.Message)
                        );
                    }
                };
            }

            // Extract write method
            var write = sinkObj.Get("write");
            if (!write.IsUndefined())
            {
                writeAlgorithm = chunk =>
                {
                    var manualPromise = stream.Engine.Advanced.RegisterPromise();
                    try
                    {
                        manualPromise.Resolve(
                            stream.Engine.Call(
                                write,
                                underlyingSink,
                                [chunk, JsValue.FromObject(stream.Engine, controller)]
                            )
                        );
                    }
                    catch (Exception ex)
                    {
                        manualPromise.Reject(
                            ErrorHelper.Create(stream.Engine, "TypeError", ex.Message)
                        );
                    }

                    return manualPromise.Promise;
                };
            }

            // Extract close method
            var close = sinkObj.Get("close");
            if (!close.IsUndefined())
            {
                closeAlgorithm = () =>
                {
                    var manualPromise = stream.Engine.Advanced.RegisterPromise();

                    try
                    {
                        manualPromise.Resolve(stream.Engine.Call(close, underlyingSink, []));
                    }
                    catch (Exception ex)
                    {
                        manualPromise.Reject(
                            ErrorHelper.Create(stream.Engine, "TypeError", ex.Message)
                        );
                    }

                    return manualPromise.Promise;
                };
            }

            // Extract abort method
            var abort = sinkObj.Get("abort");
            if (!abort.IsUndefined())
            {
                abortAlgorithm = reason =>
                {
                    var manualPromise = stream.Engine.Advanced.RegisterPromise();

                    try
                    {
                        manualPromise.Resolve(stream.Engine.Call(abort, underlyingSink, [reason]));
                    }
                    catch (Exception ex)
                    {
                        manualPromise.Reject(
                            ErrorHelper.Create(stream.Engine, "TypeError", ex.Message)
                        );
                    }

                    return manualPromise.Promise;
                };
            }
        }

        WritableStream.AbstractOperations.SetUpWritableStreamDefaultController(
            stream,
            controller,
            startAlgorithm,
            writeAlgorithm,
            closeAlgorithm,
            abortAlgorithm,
            highWaterMark,
            sizeAlgorithm
        );
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-close
    /// </summary>
    public static void WritableStreamDefaultControllerClose(
        WritableStreamDefaultControllerInstance controller
    )
    {
        EnqueueValueWithSize(controller, CloseQueuedRecord.Instance, 0);
        WritableStreamDefaultControllerAdvanceQueueIfNeeded(controller);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-error
    /// </summary>
    public static void WritableStreamDefaultControllerError(
        WritableStreamDefaultControllerInstance controller,
        JsValue error
    )
    {
        var stream = controller.ControlledWritableStream!;
        if (stream.State != WritableStreamState.Writable)
        {
            return;
        }

        WritableStreamDefaultControllerClearAlgorithms(controller);
        WritableStream.AbstractOperations.WritableStreamStartErroring(stream, error);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-clear-algorithms
    /// </summary>
    public static void WritableStreamDefaultControllerClearAlgorithms(
        WritableStreamDefaultControllerInstance controller
    )
    {
        controller.WriteAlgorithm = _ =>
            PromiseHelper
                .CreateResolvedPromise(
                    controller.ControlledWritableStream!.Engine,
                    JsValue.Undefined
                )
                .Promise;
        controller.CloseAlgorithm = () =>
            PromiseHelper
                .CreateResolvedPromise(
                    controller.ControlledWritableStream!.Engine,
                    JsValue.Undefined
                )
                .Promise;
        controller.AbortAlgorithm = _ =>
            PromiseHelper
                .CreateResolvedPromise(
                    controller.ControlledWritableStream!.Engine,
                    JsValue.Undefined
                )
                .Promise;
        controller.StrategySizeAlgorithm = _ => 1;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-advance-queue-if-needed
    /// </summary>
    public static void WritableStreamDefaultControllerAdvanceQueueIfNeeded(
        WritableStreamDefaultControllerInstance controller
    )
    {
        var stream = controller.ControlledWritableStream;
        if (!controller.Started)
        {
            return;
        }

        if (stream?.InFlightWriteRequest != null)
        {
            return;
        }

        var state = stream?.State;
        if (state == WritableStreamState.Erroring || state == WritableStreamState.Errored)
        {
            return;
        }

        if (state == WritableStreamState.Closed)
        {
            return;
        }

        if (controller.Queue.Count == 0)
        {
            return;
        }

        var value = controller.Queue[0].Value;
        if (value == CloseQueuedRecord.Instance)
        {
            WritableStreamDefaultControllerProcessClose(controller);
        }
        else
        {
            WritableStreamDefaultControllerProcessWrite(controller, value);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-process-close
    /// </summary>
    public static void WritableStreamDefaultControllerProcessClose(
        WritableStreamDefaultControllerInstance controller
    )
    {
        var stream = controller.ControlledWritableStream!;
        WritableStreamMarkCloseRequestInFlight(stream);
        DequeueValue(controller);

        if (controller.Queue.Count > 0)
        {
            throw new InvalidOperationException("Queue should be empty when processing close");
        }

        var sinkClosePromise = controller.CloseAlgorithm();
        WritableStreamDefaultControllerClearAlgorithms(controller);

        try
        {
            sinkClosePromise.UnwrapIfPromise();
            WritableStream.AbstractOperations.WritableStreamFinishInFlightClose(stream);
        }
        catch (PromiseRejectedException e)
        {
            WritableStream.AbstractOperations.WritableStreamFinishInFlightCloseWithError(
                stream,
                e.RejectedValue
            );
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-process-write
    /// </summary>
    public static void WritableStreamDefaultControllerProcessWrite(
        WritableStreamDefaultControllerInstance controller,
        JsValue chunk
    )
    {
        var stream = controller.ControlledWritableStream!;
        WritableStreamMarkFirstWriteRequestInFlight(stream);
        var sinkWritePromise = controller.WriteAlgorithm(chunk);

        try
        {
            sinkWritePromise.UnwrapIfPromise();

            WritableStream.AbstractOperations.WritableStreamFinishInFlightWrite(stream);
            var state = stream.State;
            if (state != WritableStreamState.Erroring && state != WritableStreamState.Errored)
            {
                DequeueValue(controller);
                if (
                    !WritableStreamCloseQueuedOrInFlight(stream)
                    && state == WritableStreamState.Writable
                )
                {
                    var backpressure = WritableStreamDefaultControllerGetBackpressure(controller);
                    WritableStream.AbstractOperations.WritableStreamUpdateBackpressure(
                        stream,
                        backpressure
                    );
                }

                WritableStreamDefaultControllerAdvanceQueueIfNeeded(controller);
            }
        }
        catch (PromiseRejectedException e)
        {
            if (stream.State == WritableStreamState.Writable)
            {
                WritableStreamDefaultControllerClearAlgorithms(controller);
            }
            WritableStream.AbstractOperations.WritableStreamFinishInFlightWriteWithError(
                stream,
                e.RejectedValue
            );
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-write
    /// </summary>
    public static void WritableStreamDefaultControllerWrite(
        WritableStreamDefaultControllerInstance controller,
        JsValue chunk,
        double size
    )
    {
        try
        {
            EnqueueValueWithSize(controller, chunk, size);
        }
        catch (JavaScriptException e)
        {
            WritableStreamDefaultControllerErrorIfNeeded(controller, e.Error);
            return;
        }

        var stream = controller.ControlledWritableStream!;

        if (
            !WritableStreamCloseQueuedOrInFlight(stream)
            && stream.State == WritableStreamState.Writable
        )
        {
            var backpressure = WritableStreamDefaultControllerGetBackpressure(controller);
            WritableStream.AbstractOperations.WritableStreamUpdateBackpressure(
                stream,
                backpressure
            );
        }

        WritableStreamDefaultControllerAdvanceQueueIfNeeded(controller);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-error-if-needed
    /// </summary>
    public static void WritableStreamDefaultControllerErrorIfNeeded(
        WritableStreamDefaultControllerInstance controller,
        JsValue error
    )
    {
        if (controller.Stream?.State == WritableStreamState.Writable)
        {
            WritableStreamDefaultControllerError(controller, error);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-get-backpressure
    /// </summary>
    public static bool WritableStreamDefaultControllerGetBackpressure(
        WritableStreamDefaultControllerInstance controller
    )
    {
        var desiredSize = WritableStreamDefaultControllerGetDesiredSize(controller);
        return desiredSize <= 0;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-get-desired-size
    /// </summary>
    public static double WritableStreamDefaultControllerGetDesiredSize(
        WritableStreamDefaultControllerInstance controller
    )
    {
        return controller.StrategyHWM - controller.QueueTotalSize;
    }

    #region Helper Methods

    private static void EnqueueValueWithSize(
        WritableStreamDefaultControllerInstance controller,
        JsValue value,
        double size
    )
    {
        if (!IsNonNegativeNumber(size) || double.IsInfinity(size))
        {
            throw new ArgumentOutOfRangeException(nameof(size), "Invalid size");
        }

        controller.Queue.Add(
            new WritableStreamDefaultControllerInstance.QueueEntry { Value = value, Size = size }
        );
        controller.QueueTotalSize += size;
    }

    private static JsValue DequeueValue(WritableStreamDefaultControllerInstance controller)
    {
        if (controller.Queue.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }

        var pair = controller.Queue[0];
        controller.Queue.RemoveAt(0);
        controller.QueueTotalSize -= pair.Size;

        if (controller.QueueTotalSize < 0)
        {
            controller.QueueTotalSize = 0;
        }

        return pair.Value;
    }

    private static bool IsNonNegativeNumber(double number)
    {
        return !double.IsNaN(number) && number >= 0;
    }

    private static void WritableStreamMarkCloseRequestInFlight(WritableStreamInstance stream)
    {
        stream.InFlightCloseRequest = stream.CloseRequest;
        stream.CloseRequest = null;
    }

    private static void WritableStreamMarkFirstWriteRequestInFlight(WritableStreamInstance stream)
    {
        if (stream.WriteRequests.Count == 0)
        {
            throw new InvalidOperationException("No write requests in queue");
        }

        stream.InFlightWriteRequest = stream.WriteRequests[0];
        stream.WriteRequests.RemoveAt(0);
    }

    private static bool WritableStreamCloseQueuedOrInFlight(WritableStreamInstance stream)
    {
        return stream.CloseRequest != null || stream.InFlightCloseRequest != null;
    }

    #endregion
}
