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

        startAlgorithm
            .Call(Controller)
            .Then(
                onFulfilled: _ =>
                {
                    Controller.Started = true;
                    Controller.AdvanceQueueIfNeeded();
                    return Undefined;
                },
                onRejected: e =>
                {
                    Controller.Started = true;
                    DealWithRejection(e);
                    return Undefined;
                }
            );
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
        // Assert: stream.[[storedError]] is undefined.
        // Assert: stream.[[state]] is "writable".
        // Let controller be stream.[[controller]].
        // Assert: controller is not undefined.
        if (Controller is null)
        {
            return;
        }

        // Set stream.[[state]] to "erroring".
        State = WritableStreamState.Erroring;

        // Set stream.[[storedError]] to reason.
        StoredError = reason;

        // Let writer be stream.[[writer]].
        // If writer is not undefined, perform ! WritableStreamDefaultWriterEnsureReadyPromiseRejected(writer, reason).
        Writer?.EnsureReadyPromiseRejected(reason);

        // If ! WritableStreamHasOperationMarkedInFlight(stream) is false and controller.[[started]] is true, perform ! WritableStreamFinishErroring(stream).
        if (!HasOperationMarkedInFlight() && Controller.Started)
        {
            FinishErroring();
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-finish-erroring
    /// </summary>
    internal void FinishErroring()
    {
        // Assert: stream.[[state]] is "erroring".
        // Assert: ! WritableStreamHasOperationMarkedInFlight(stream) is false.

        // Set stream.[[state]] to "errored".
        State = WritableStreamState.Errored;

        // Perform ! stream.[[controller]].[[ErrorSteps]]().
        Controller?.ErrorSteps();

        // Let storedError be stream.[[storedError]].
        var storedError = StoredError;
        // For each writeRequest of stream.[[writeRequests]]:
        foreach (var writeRequest in WriteRequests)
        {
            // Reject writeRequest with storedError.
            writeRequest.Reject(storedError);
        }
        // Set stream.[[writeRequests]] to an empty list.
        WriteRequests.Clear();

        // If stream.[[pendingAbortRequest]] is undefined,
        if (PendingAbortRequest == null)
        {
            // Perform ! WritableStreamRejectCloseAndClosedPromiseIfNeeded(stream).
            // Return.
            RejectCloseAndClosedPromiseIfNeeded();
            return;
        }

        // Let abortRequest be stream.[[pendingAbortRequest]].
        // Set stream.[[pendingAbortRequest]] to undefined.
        var abortRequest = PendingAbortRequest;
        PendingAbortRequest = null;

        // If abortRequest’s was already erroring is true,
        if (abortRequest.Value.WasAlreadyErroring)
        {
            // Reject abortRequest’s promise with storedError.
            abortRequest.Value.Promise.Reject(storedError);

            // Perform ! WritableStreamRejectCloseAndClosedPromiseIfNeeded(stream).
            RejectCloseAndClosedPromiseIfNeeded();

            // Return.
            return;
        }

        // Let promise be ! stream.[[controller]].[[AbortSteps]](abortRequest’s reason).
        var promise = Controller?.AbortSteps(abortRequest.Value.Reason);

        promise?.Then(
            onFulfilled: _ =>
            {
                // Upon fulfillment of promise,
                // Resolve abortRequest’s promise with undefined.
                abortRequest.Value.Promise.Resolve(Undefined);

                // Perform ! WritableStreamRejectCloseAndClosedPromiseIfNeeded(stream).
                RejectCloseAndClosedPromiseIfNeeded();

                return Undefined;
            },
            onRejected: e =>
            {
                // Upon rejection of promise with reason reason,
                // Reject abortRequest’s promise with reason.
                abortRequest.Value.Promise.Reject(e);

                // Perform ! WritableStreamRejectCloseAndClosedPromiseIfNeeded(stream).
                RejectCloseAndClosedPromiseIfNeeded();

                return Undefined;
            }
        );
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-reject-close-and-closed-promise-if-needed
    /// </summary>
    private void RejectCloseAndClosedPromiseIfNeeded()
    {
        // Assert: stream.[[state]] is "errored".

        // If stream.[[closeRequest]] is not undefined,
        //   Assert: stream.[[inFlightCloseRequest]] is undefined.
        //   Reject stream.[[closeRequest]] with stream.[[storedError]].
        //   Set stream.[[closeRequest]] to undefined.
        CloseRequest?.Reject(StoredError);
        CloseRequest = null;

        // Let writer be stream.[[writer]].
        // If writer is not undefined,
        // Reject writer.[[closedPromise]] with stream.[[storedError]].
        // Set writer.[[closedPromise]].[[PromiseIsHandled]] to true.
        Writer?.ClosedPromise?.Reject(StoredError);
    }

    private bool HasOperationMarkedInFlight()
    {
        return !(InFlightWriteRequest == null && InFlightCloseRequest == null);
    }

    internal void MarkFirstWriteRequestInFlight()
    {
        lock (WriteRequests)
        {
            if (WriteRequests.Count == 0)
            {
                throw new JavaScriptException("No write requests in queue");
            }

            InFlightWriteRequest = WriteRequests[0];
            WriteRequests.RemoveAt(0);
        }
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

    internal WritableStreamDefaultWriterInstance AcquireWriter()
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
