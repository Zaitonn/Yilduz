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
        QueueTotalSize = 0;
        Queue.Clear();
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

    internal void AdvanceQueueIfNeeded()
    {
        if (!Started)
        {
            return;
        }

        if (Stream?.InFlightWriteRequest != null)
        {
            return;
        }

        var state = Stream?.State;
        if (state == WritableStreamState.Erroring || state == WritableStreamState.Errored)
        {
            return;
        }

        if (state == WritableStreamState.Closed)
        {
            return;
        }

        if (Queue.Count == 0)
        {
            return;
        }

        var value = Queue[0].Value;
        if (value == CloseQueuedRecord.Instance)
        {
            ProcessClose();
        }
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

    private void ProcessWrite(JsValue chunk)
    {
        Stream.MarkFirstWriteRequestInFlight();

        try
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
                        switch (t.Status)
                        {
                            case TaskStatus.RanToCompletion:
                                OnCompletedSuccessfully();
                                break;

                            case TaskStatus.Faulted
                                when t.Exception?.InnerException is PromiseRejectedException e:
                                OnError(e.RejectedValue);
                                break;
                        }
                    },
                    TaskContinuationOptions.ExecuteSynchronously
                );
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
