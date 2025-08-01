using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Promise;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Streams.WritableStreamDefaultController;
using Yilduz.Streams.WritableStreamDefaultWriter;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStream;

public sealed partial class WritableStreamInstance
{
    internal bool Backpressure { get; private set; }
    internal ManualPromise? CloseRequest { get; set; }
    internal WritableStreamDefaultControllerInstance Controller { get; private set; }
    internal bool Detached { get; set; }
    internal ManualPromise? InFlightWriteRequest { get; set; }
    internal ManualPromise? InFlightCloseRequest { get; set; }
    internal PendingAbortRequest? PendingAbortRequest { get; set; }
    internal WritableStreamState State { get; private set; }
    internal JsValue StoredError { get; private set; } = Undefined;
    internal WritableStreamDefaultWriterInstance? Writer { get; set; }
    internal List<ManualPromise> WriteRequests { get; private set; } = [];
    internal bool IsCloseQueuedOrInFlight =>
        !(CloseRequest == null && InFlightCloseRequest == null);

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-abort
    /// </summary>
    internal JsValue AbortInternal(JsValue reason)
    {
        if (State == WritableStreamState.Closed || State == WritableStreamState.Errored)
        {
            return PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise;
        }

        // Signal abort on stream.[[controller]].[[abortController]] with reason
        Controller.AbortController.Abort(reason);

        if (State == WritableStreamState.Closed || State == WritableStreamState.Errored)
        {
            return PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise;
        }

        if (PendingAbortRequest is not null)
        {
            return PendingAbortRequest.Value.Promise.Promise;
        }

        if (State is not WritableStreamState.Writable and not WritableStreamState.Erroring)
        {
            throw new InvalidOperationException();
        }

        var wasAlreadyErroring = false;
        if (State == WritableStreamState.Erroring)
        {
            wasAlreadyErroring = true;
            reason = Undefined;
        }

        var promise = Engine.Advanced.RegisterPromise();
        PendingAbortRequest = new PendingAbortRequest
        {
            Promise = promise,
            Reason = reason,
            WasAlreadyErroring = wasAlreadyErroring,
        };

        if (!wasAlreadyErroring)
        {
            StartErroring(reason);
        }

        return promise.Promise;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-close
    /// </summary>
    internal JsValue CloseInternal()
    {
        if (State == WritableStreamState.Closed || State == WritableStreamState.Errored)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Stream is not in a writable state")
                )
                .Promise;
        }

        if (IsCloseQueuedOrInFlight)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Stream is already being closed")
                )
                .Promise;
        }

        var promise = Engine.Advanced.RegisterPromise();
        CloseRequest = promise;

        if (Writer != null && Backpressure && State == WritableStreamState.Writable)
        {
            Writer.ReadyPromise?.Resolve(Undefined);
        }

        Controller.CloseInternal();
        return promise.Promise;
    }

    /// <summary>
    /// SetUpWritableStreamDefaultControllerFromUnderlyingSink
    /// </summary>
    [MemberNotNull(nameof(Controller))]
    private void SetUpControllerFromUnderlyingSink(
        JsValue underlyingSink,
        object? _, // underlyingSinkDict
        double highWaterMark,
        Function sizeAlgorithm
    )
    {
        var emptyPromiseFunction = new ClrFunction(
            Engine,
            string.Empty,
            (_, _) => PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise
        );

        Function startAlgorithm = new ClrFunction(Engine, string.Empty, (_, _) => Undefined);
        Function writeAlgorithm = emptyPromiseFunction;
        Function closeAlgorithm = emptyPromiseFunction;
        Function abortAlgorithm = emptyPromiseFunction;

        if (underlyingSink.IsObject())
        {
            var sinkObj = underlyingSink.AsObject();

            // Extract start method
            var start = sinkObj.Get("start");
            if (!start.IsUndefined())
            {
                startAlgorithm = start.AsFunctionInstance();
            }

            // Extract write method
            var write = sinkObj.Get("write");
            if (!write.IsUndefined())
            {
                writeAlgorithm = write.AsFunctionInstance();
            }

            // Extract close method
            var close = sinkObj.Get("close");
            if (!close.IsUndefined())
            {
                closeAlgorithm = close.AsFunctionInstance();
            }

            // Extract abort method
            var abort = sinkObj.Get("abort");
            if (!abort.IsUndefined())
            {
                abortAlgorithm = abort.AsFunctionInstance();
            }
        }

        SetUpController(
            startAlgorithm,
            writeAlgorithm,
            closeAlgorithm,
            abortAlgorithm,
            highWaterMark,
            sizeAlgorithm
        );
    }

    /// <summary>
    /// SetUpWritableStreamDefaultController
    /// </summary>
    [MemberNotNull(nameof(Controller))]
    private void SetUpController(
        Function startAlgorithm,
        Function writeAlgorithm,
        Function closeAlgorithm,
        Function abortAlgorithm,
        double highWaterMark,
        Function sizeAlgorithm
    )
    {
        Controller = Engine
            .GetWebApiIntrinsics()
            .WritableStreamDefaultController.Construct(
                this,
                writeAlgorithm,
                closeAlgorithm,
                abortAlgorithm,
                highWaterMark,
                sizeAlgorithm
            );

        var backpressure = Controller.GetBackpressure();
        UpdateBackpressure(backpressure);

        try
        {
            startAlgorithm.Call(Controller).UnwrapIfPromise();

            Controller.Started = true;
            Controller.AdvanceQueueIfNeeded();
        }
        catch (PromiseRejectedException e)
        {
            Controller.Started = true;
            DealWithRejection(e.RejectedValue);
        }
        catch (JavaScriptException e)
        {
            Controller.Started = true;
            DealWithRejection(e.Error);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-update-backpressure
    /// </summary>
    internal void UpdateBackpressure(bool backpressure)
    {
        if (State != WritableStreamState.Writable)
        {
            return;
        }

        if (IsCloseQueuedOrInFlight)
        {
            return;
        }

        if (Writer != null && backpressure != Backpressure)
        {
            if (backpressure)
            {
                Writer.ReadyPromise = Engine.Advanced.RegisterPromise();
            }
            else
            {
                Writer.ReadyPromise?.Resolve(Undefined);
            }
        }

        Backpressure = backpressure;
    }

    private void DealWithRejection(JsValue error)
    {
        var state = State;
        if (state == WritableStreamState.Writable)
        {
            StartErroring(error);
            return;
        }

        FinishErroring();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-start-erroring
    /// </summary>
    internal void StartErroring(JsValue reason)
    {
        StoredError = reason;
        State = WritableStreamState.Erroring;

        Writer?.EnsureReadyPromiseRejected(reason);

        if (!HasOperationMarkedInFlight() && (Controller?.Started ?? false))
        {
            FinishErroring();
        }
    }

    internal void FinishErroring()
    {
        State = WritableStreamState.Errored;
        Controller?.ErrorSteps();

        var storedError = StoredError;
        foreach (var writeRequest in WriteRequests)
        {
            writeRequest.Reject(storedError);
        }
        WriteRequests.Clear();

        if (PendingAbortRequest == null)
        {
            RejectCloseAndClosedPromiseIfNeeded();
            return;
        }

        var abortRequest = PendingAbortRequest;
        PendingAbortRequest = null;

        if (abortRequest.Value.WasAlreadyErroring)
        {
            abortRequest.Value.Promise.Reject(storedError);
            RejectCloseAndClosedPromiseIfNeeded();
            return;
        }

        var promise = Controller?.AbortSteps(abortRequest.Value.Reason);

        try
        {
            promise?.UnwrapIfPromise();
            abortRequest.Value.Promise.Resolve(Undefined);
            RejectCloseAndClosedPromiseIfNeeded();
        }
        catch (PromiseRejectedException e)
        {
            abortRequest.Value.Promise.Reject(e.RejectedValue);
            RejectCloseAndClosedPromiseIfNeeded();
        }
    }

    private void RejectCloseAndClosedPromiseIfNeeded()
    {
        CloseRequest?.Reject(StoredError);
        CloseRequest = null;
        Writer?.ClosedPromise?.Reject(StoredError);
    }

    private bool HasOperationMarkedInFlight()
    {
        return !(InFlightWriteRequest == null && InFlightCloseRequest == null);
    }

    internal void MarkFirstWriteRequestInFlight()
    {
        if (WriteRequests.Count == 0)
        {
            throw new JavaScriptException("No write requests in queue");
        }

        InFlightWriteRequest = WriteRequests[0];
        WriteRequests.RemoveAt(0);
    }

    internal void FinishInFlightWrite()
    {
        InFlightWriteRequest?.Resolve(Undefined);
        InFlightWriteRequest = null;
    }

    internal void FinishInFlightCloseWithError(JsValue error)
    {
        InFlightCloseRequest?.Reject(error);
        InFlightCloseRequest = null;

        PendingAbortRequest?.Promise.Reject(error);
        PendingAbortRequest = null;

        DealWithRejection(error);
    }

    internal void FinishInFlightClose()
    {
        InFlightCloseRequest?.Resolve(Undefined);
        InFlightCloseRequest = null;
        State = WritableStreamState.Closed;

        Writer?.ClosedPromise?.Resolve(Undefined);
    }

    internal void FinishInFlightWriteWithError(JsValue error)
    {
        InFlightWriteRequest?.Reject(error);
        InFlightWriteRequest = null;

        if (State is WritableStreamState.Writable or WritableStreamState.Erroring)
        {
            PendingAbortRequest?.Promise.Reject(error);
            PendingAbortRequest = null;

            DealWithRejection(error);
        }
    }

    private WritableStreamDefaultWriterInstance AcquireWriter()
    {
        var writer = Engine.GetWebApiIntrinsics().WritableStreamDefaultWriter.Construct();
        SetUpWriter(writer);
        return writer;
    }

    private void SetUpWriter(WritableStreamDefaultWriterInstance writer)
    {
        if (Locked && Writer != writer)
        {
            TypeErrorHelper.Throw(Engine, "Stream is already locked");
        }

        writer.Stream = this;
        Writer = writer;

        if (State == WritableStreamState.Writable)
        {
            if (!IsCloseQueuedOrInFlight && Backpressure)
            {
                writer.ReadyPromise = Engine.Advanced.RegisterPromise();
            }
            else
            {
                writer.ReadyPromise = PromiseHelper.CreateResolvedPromise(Engine, Undefined);
            }
            writer.ClosedPromise = Engine.Advanced.RegisterPromise();
        }
        else if (State == WritableStreamState.Erroring)
        {
            writer.ReadyPromise = PromiseHelper.CreateRejectedPromise(Engine, StoredError);
            writer.ClosedPromise = Engine.Advanced.RegisterPromise();
        }
        else if (State == WritableStreamState.Closed)
        {
            writer.ReadyPromise = PromiseHelper.CreateResolvedPromise(Engine, Undefined);
            writer.ClosedPromise = PromiseHelper.CreateResolvedPromise(Engine, Undefined);
        }
        else
        {
            // errored
            var storedError = StoredError;
            writer.ReadyPromise = PromiseHelper.CreateRejectedPromise(Engine, storedError);
            writer.ClosedPromise = PromiseHelper.CreateRejectedPromise(Engine, storedError);
        }
    }
}
