using System.Diagnostics.CodeAnalysis;
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
    internal void ErrorSteps()
    {
        this.ResetQueue();
    }

    internal JsValue AbortSteps(JsValue reason)
    {
        JsValue result;

        try
        {
            result = AbortAlgorithm.Call(reason);
        }
        catch (JavaScriptException e)
        {
            result = PromiseHelper.CreateRejectedPromise(Engine, e.Error).Promise;
        }
        finally
        {
            ClearAlgorithms();
        }

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

    internal bool GetBackpressure()
    {
        return GetDesiredSize() <= 0;
    }

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
            return;
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

    private void ProcessClose()
    {
        MarkCloseRequestInFlight();
        this.DequeueValue();

        if (Queue.Count > 0)
        {
            throw new JavaScriptException("Queue should be empty when processing close");
        }

        try
        {
            CloseAlgorithm.Call(this).UnwrapIfPromise();
            ClearAlgorithms();
            Stream.FinishInFlightClose();
        }
        catch (PromiseRejectedException e)
        {
            Stream.FinishInFlightCloseWithError(e.RejectedValue);
        }
        catch (JavaScriptException e)
        {
            Stream.FinishInFlightCloseWithError(e.Error);
        }
    }

    private readonly object _writeLock = new();

    private void ProcessWrite(JsValue chunk)
    {
        Stream.MarkFirstWriteRequestInFlight();

        try
        {
            lock (_writeLock)
            {
                var sinkWritePromise = WriteAlgorithm.Call(chunk, this);

                if (!sinkWritePromise.IsPromise())
                {
                    OnCompletedSuccessfully();
                    return;
                }

                Task.Run(sinkWritePromise.UnwrapIfPromise)
                    .ContinueWith(
                        t =>
                        {
                            lock (Engine)
                            {
                                switch (t.Status)
                                {
                                    case TaskStatus.RanToCompletion:
                                        OnCompletedSuccessfully();
                                        break;

                                    case TaskStatus.Faulted
                                        when t.Exception?.InnerException
                                            is PromiseRejectedException e:
                                        OnError(e.RejectedValue);
                                        break;
                                }
                            }
                        },
                        TaskContinuationOptions.ExecuteSynchronously
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
            Stream.FinishInFlightWrite();
            var state = Stream.State;
            if (state != WritableStreamState.Erroring && state != WritableStreamState.Errored)
            {
                this.DequeueValue();
                if (!Stream.IsCloseQueuedOrInFlight && state == WritableStreamState.Writable)
                {
                    var backpressure = GetBackpressure();
                    Stream.UpdateBackpressure(backpressure);
                }

                AdvanceQueueIfNeeded();
            }
        }

        void OnError(JsValue error)
        {
            if (Stream.State == WritableStreamState.Writable)
            {
                ClearAlgorithms();
            }

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
            this.EnqueueValueWithSize(Engine, chunk, size);
        }
        catch (JavaScriptException e)
        {
            ErrorIfNeeded(e.Error);
            return;
        }

        if (!Stream.IsCloseQueuedOrInFlight && Stream.State == WritableStreamState.Writable)
        {
            var backpressure = GetBackpressure();
            Stream.UpdateBackpressure(backpressure);
        }

        AdvanceQueueIfNeeded();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-controller-error-if-needed
    /// </summary>
    private void ErrorIfNeeded(JsValue error)
    {
        if (Stream.State == WritableStreamState.Writable)
        {
            ErrorInternal(error);
        }
    }

    internal void ErrorInternal(JsValue error)
    {
        if (Stream.State != WritableStreamState.Writable)
        {
            return;
        }

        ClearAlgorithms();
        Stream.StartErroring(error);
    }

    internal void CloseInternal()
    {
        this.EnqueueValueWithSize(Engine, CloseQueuedRecord.Instance, 0);
        AdvanceQueueIfNeeded();
    }
}
