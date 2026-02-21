using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Streams.Queue;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStreamDefaultController;

public sealed partial class WritableStreamDefaultControllerInstance
{
    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-controller-private-error
    /// </summary>
    internal void ErrorSteps()
    {
        // Perform ! ResetQueue(this).
        this.ResetQueue();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#ws-default-controller-private-abort
    /// </summary>
    internal JsValue AbortSteps(JsValue reason)
    {
        JsValue result;

        try
        {
            // Let result be the result of performing this.[[abortAlgorithm]], passing reason.
            result = AbortAlgorithm.Call(reason);
        }
        catch (JavaScriptException e)
        {
            // Convert any exceptions into rejected promises.
            result = PromiseHelper.CreateRejectedPromise(Engine, e.Error).Promise;
        }
        finally
        {
            // Perform ! WritableStreamDefaultControllerClearAlgorithms(this).
            ClearAlgorithms();
        }

        // Return result.
        return result;
    }

    /// <summary>
    /// WritableStreamDefaultControllerClearAlgorithms
    /// </summary>
    [MemberNotNull(nameof(WriteAlgorithm), nameof(CloseAlgorithm), nameof(AbortAlgorithm))]
    internal void ClearAlgorithms()
    {
        var emptyPromiseFunction = new ClrFunction(
            Engine,
            string.Empty,
            (_, _) => PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise
        );
        WriteAlgorithm = emptyPromiseFunction;
        CloseAlgorithm = emptyPromiseFunction;
        AbortAlgorithm = emptyPromiseFunction;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-get-backpressure
    /// </summary>
    internal bool GetBackpressure()
    {
        return GetDesiredSize() <= 0;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-get-desired-size
    /// </summary>
    internal double GetDesiredSize()
    {
        return StrategyHWM - QueueTotalSize;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-advance-queue-if-needed
    /// </summary>
    internal void AdvanceQueueIfNeeded()
    {
        // Let stream be controller.[[stream]].
        // If controller.[[started]] is false, return.
        if (!Started)
        {
            return;
        }

        // If stream.[[inFlightWriteRequest]] is not undefined, return.
        if (Stream?.InFlightWriteRequest != null)
        {
            return;
        }

        // Let state be stream.[[state]].
        // Assert: state is not "closed" or "errored".
        if (
            Stream?.State == WritableStreamState.Closed
            || Stream?.State == WritableStreamState.Errored
        )
        {
            throw new InvalidOperationException("Stream state should not be closed or errored");
        }

        // If state is "erroring",
        if (Stream?.State == WritableStreamState.Erroring)
        {
            Stream.FinishErroring();
            return;
        }

        // If controller.[[queue]] is empty, return.
        if (Queue.Count == 0)
        {
            return;
        }

        // Let value be ! PeekQueueValue(controller).
        var value = this.PeekQueueValue();

        // If value is the close sentinel, perform ! WritableStreamDefaultControllerProcessClose(controller).
        if (value == CloseQueuedRecord.Instance)
        {
            ProcessClose();
        }
        // Otherwise, perform ! WritableStreamDefaultControllerProcessWrite(controller, value).
        else
        {
            ProcessWrite(value);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-process-close
    /// </summary>
    private void ProcessClose()
    {
        // Let stream be controller.[[stream]].

        // Perform ! WritableStreamMarkCloseRequestInFlight(stream).
        MarkCloseRequestInFlight();

        // Perform ! DequeueValue(controller).
        this.DequeueValue();

        // Assert: controller.[[queue]] is empty.
        if (Queue.Count > 0)
        {
            throw new JavaScriptException("Queue should be empty when processing close");
        }

        try
        {
            var sinkClosePromise = CloseAlgorithm.Call(this);

            sinkClosePromise.Then(
                onFulfilled: _ =>
                {
                    // Upon fulfillment of sinkClosePromise,
                    // Perform ! WritableStreamFinishInFlightClose(stream).
                    Stream.FinishInFlightClose();
                    return Undefined;
                },
                onRejected: reason =>
                {
                    // Upon rejection of sinkClosePromise with reason reason,
                    // Perform ! WritableStreamFinishInFlightCloseWithError(stream, reason).
                    Stream.FinishInFlightCloseWithError(reason);
                    return Undefined;
                }
            );
        }
        catch (JavaScriptException e)
        {
            // Perform ! WritableStreamFinishInFlightCloseWithError(stream, reason).
            Stream.FinishInFlightCloseWithError(e.Error);
        }
        finally
        {
            // Perform ! WritableStreamDefaultControllerClearAlgorithms(controller).
            ClearAlgorithms();
        }
    }

    private readonly object _writeLock = new();

    private CancellationTokenSource? _writeCancellationTokenSource;

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-process-write
    /// </summary>
    private void ProcessWrite(JsValue chunk)
    {
        // Let stream be controller.[[stream]].

        // Perform ! WritableStreamMarkFirstWriteRequestInFlight(stream).
        Stream.MarkFirstWriteRequestInFlight();

        try
        {
            lock (_writeLock)
            {
                // Let sinkWritePromise be the result of performing controller.[[writeAlgorithm]], passing in chunk.
                var sinkWritePromise = WriteAlgorithm.Call(chunk, this);

                if (!sinkWritePromise.IsPromise())
                {
                    OnCompletedSuccessfully();
                    return;
                }

                var eventLoop = Engine.GetWebApiIntrinsics().EventLoop;

                if (!sinkWritePromise.IsPendingPromise())
                {
                    if (sinkWritePromise.TryGetRejectedValue(out var rejected))
                    {
                        eventLoop.QueueMacrotask(() => OnError(rejected));
                    }
                    else
                    {
                        eventLoop.QueueMacrotask(OnCompletedSuccessfully);
                    }

                    return;
                }

                if (_writeCancellationTokenSource is null)
                {
                    _writeCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
                        _engine.GetWebApiIntrinsics().Options.CancellationToken
                    );
                }

                sinkWritePromise.Then(
                    onFulfilled: _ =>
                    {
                        // Upon fulfillment of sinkWritePromise
                        OnCompletedSuccessfully();
                        return Undefined;
                    },
                    onRejected: reason =>
                    {
                        // Upon rejection of sinkWritePromise with reason
                        OnError(reason);
                        return Undefined;
                    },
                    _writeCancellationTokenSource.Token
                );
            }
        }
        catch (JavaScriptException e)
        {
            OnError(e.Error);
            throw;
        }

        void OnCompletedSuccessfully()
        {
            // Perform ! WritableStreamFinishInFlightWrite(stream).
            Stream.FinishInFlightWrite();

            // Let state be stream.[[state]].
            var state = Stream.State;

            // Assert: state is "writable" or "erroring".
            if (state != WritableStreamState.Writable && state != WritableStreamState.Erroring)
            {
                throw new InvalidOperationException("Stream state should be writable or erroring");
            }

            // Perform ! DequeueValue(controller).
            this.DequeueValue();

            // If ! WritableStreamCloseQueuedOrInFlight(stream) is false and state is "writable",
            if (!Stream.IsCloseQueuedOrInFlight && state == WritableStreamState.Writable)
            {
                // Let backpressure be ! WritableStreamDefaultControllerGetBackpressure(controller).
                var backpressure = GetBackpressure();

                // Perform ! WritableStreamUpdateBackpressure(stream, backpressure).
                Stream.UpdateBackpressure(backpressure);
            }

            // Perform ! WritableStreamDefaultControllerAdvanceQueueIfNeeded(controller).
            AdvanceQueueIfNeeded();
        }

        void OnError(JsValue error)
        {
            // If stream.[[state]] is "writable", perform ! WritableStreamDefaultControllerClearAlgorithms(controller).
            if (Stream.State == WritableStreamState.Writable)
            {
                ClearAlgorithms();
            }

            // Perform ! WritableStreamFinishInFlightWriteWithError(stream, reason).
            Stream.FinishInFlightWriteWithError(error);
        }
    }

    private void MarkCloseRequestInFlight()
    {
        Stream.InFlightCloseRequest = Stream.CloseRequest;
        Stream.CloseRequest = null;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-write
    /// </summary>
    internal void Write(JsValue chunk, double size)
    {
        try
        {
            // Let enqueueResult be EnqueueValueWithSize(controller, chunk, chunkSize).
            this.EnqueueValueWithSize(Engine, chunk, size);
        }
        catch (JavaScriptException e)
        {
            // If enqueueResult is an abrupt completion,
            // Perform ! WritableStreamDefaultControllerErrorIfNeeded(controller, enqueueResult.[[Value]]).
            ErrorIfNeeded(e.Error);

            // Return.
            return;
        }

        // Let stream be controller.[[stream]].
        // If ! WritableStreamCloseQueuedOrInFlight(stream) is false and stream.[[state]] is "writable",
        if (!Stream.IsCloseQueuedOrInFlight && Stream.State == WritableStreamState.Writable)
        {
            // Let backpressure be ! WritableStreamDefaultControllerGetBackpressure(controller).
            var backpressure = GetBackpressure();

            // Perform ! WritableStreamUpdateBackpressure(stream, backpressure).
            Stream.UpdateBackpressure(backpressure);
        }

        // Perform ! WritableStreamDefaultControllerAdvanceQueueIfNeeded(controller).
        AdvanceQueueIfNeeded();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-error-if-needed
    /// </summary>
    internal void ErrorIfNeeded(JsValue error)
    {
        // If controller.[[stream]].[[state]] is "writable",
        //  perform ! WritableStreamDefaultControllerError(controller, error).
        if (Stream.State == WritableStreamState.Writable)
        {
            ErrorInternal(error);
        }
    }

    /// <summary>
    /// WritableStreamDefaultControllerError
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-error
    /// </summary>
    internal void ErrorInternal(JsValue error)
    {
        // Let stream be controller.[[stream]].
        // Assert: stream.[[state]] is "writable".
        if (Stream.State != WritableStreamState.Writable)
        {
            throw new InvalidOperationException("Stream state should be writable");
        }

        // Perform ! WritableStreamDefaultControllerClearAlgorithms(controller).
        ClearAlgorithms();

        // Perform ! WritableStreamStartErroring(stream, error).
        Stream.StartErroring(error);

        if (_writeCancellationTokenSource is { IsCancellationRequested: false })
        {
            _writeCancellationTokenSource.Cancel();
        }
    }

    internal void CloseInternal()
    {
        this.EnqueueValueWithSize(Engine, CloseQueuedRecord.Instance, 0);
        AdvanceQueueIfNeeded();
    }
}
