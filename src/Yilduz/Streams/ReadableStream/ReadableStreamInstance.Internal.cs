using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Streams.Queue;
using Yilduz.Streams.ReadableByteStreamController;
using Yilduz.Streams.ReadableStreamBYOBReader;
using Yilduz.Streams.ReadableStreamDefaultReader;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStream;

public sealed partial class ReadableStreamInstance
{
    private readonly WebApiIntrinsics _webApiIntrinsics;
    internal ReadableStreamGenericReaderInstance? Reader { get; set; }
    internal ReadableStreamController Controller { get; private set; }
    internal ReadableStreamState State { get; private set; } = ReadableStreamState.Readable;
    internal bool Detached { get; private set; }
    internal bool Disturbed { get; set; }
    internal JsValue StoredError { get; private set; } = Undefined;

    [MemberNotNullWhen(true, nameof(Reader))]
    internal bool HasDefaultReader => Reader is ReadableStreamDefaultReaderInstance;

    /// <summary>
    /// https://streams.spec.whatwg.org/#initialize-readable-stream
    /// </summary>
    private void InitializeReadableStream()
    {
        State = ReadableStreamState.Readable;
        Reader = null;
        StoredError = Undefined;
        Disturbed = false;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-close
    /// </summary>
    internal void CloseInternal()
    {
        State = ReadableStreamState.Closed;

        if (Reader is not null && Reader is ReadableStreamDefaultReaderInstance reader)
        {
            if (reader.ReadRequests.Count > 0)
            {
                // Fulfill all pending read requests with { value: undefined, done: true }
                while (reader.ReadRequests.Count > 0)
                {
                    FulfillReadRequest(Undefined, true);
                }
            }
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-error
    /// </summary>
    internal void ErrorInternal(JsValue error)
    {
        if (State != ReadableStreamState.Readable)
        {
            return;
        }

        State = ReadableStreamState.Errored;
        StoredError = error;

        if (Reader is not null && Reader is ReadableStreamDefaultReaderInstance reader)
        {
            // Reject all pending read requests
            while (reader.ReadRequests.Count > 0)
            {
                var readRequest = reader.ReadRequests[0];
                reader.ReadRequests.RemoveAt(0);
                readRequest.ErrorSteps(error);
            }
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-fulfill-read-request
    /// </summary>
    internal void FulfillReadRequest(JsValue chunk, bool done)
    {
        if (Reader is not ReadableStreamDefaultReaderInstance reader)
        {
            return;
        }

        var readRequest = reader.ReadRequests[0];
        reader.ReadRequests.RemoveAt(0);

        if (done)
        {
            readRequest.CloseSteps(Undefined);
        }
        else
        {
            readRequest.ChunkSteps(chunk);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#acquire-readable-stream-default-reader
    /// </summary>
    private ReadableStreamDefaultReaderInstance AcquireDefaultReader()
    {
        return _webApiIntrinsics.ReadableStreamDefaultReader.Construct(this);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#acquire-readable-stream-byob-reader
    /// </summary>
    private ReadableStreamBYOBReaderInstance AcquireBYOBReader()
    {
        return _webApiIntrinsics.ReadableStreamBYOBReader.Construct(this);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-cancel
    /// </summary>
    internal JsValue CancelInternal(JsValue reason)
    {
        // Set stream.[[disturbed]] to true.
        Disturbed = true;

        // If stream.[[state]] is "closed", return a promise resolved with undefined.
        if (State == ReadableStreamState.Closed)
        {
            return PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise;
        }
        // If stream.[[state]] is "errored", return a promise rejected with stream.[[storedError]].
        if (State == ReadableStreamState.Errored)
        {
            return PromiseHelper.CreateRejectedPromise(Engine, StoredError).Promise;
        }

        // Perform ! ReadableStreamClose(stream).
        CloseInternal();

        // Let reader be stream.[[reader]].
        // If reader is not undefined and reader implements ReadableStreamBYOBReader,
        if (Reader is not null && Reader is ReadableStreamBYOBReaderInstance reader)
        {
            // Let readIntoRequests be reader.[[readIntoRequests]].
            // Set reader.[[readIntoRequests]] to an empty list.

            // For each readIntoRequest of readIntoRequests,
            // Perform readIntoRequest’s close steps, given undefined.
            while (reader.ReadIntoRequests.Count > 0)
            {
                var readRequest = reader.ReadIntoRequests[0];
                reader.ReadIntoRequests.RemoveAt(0);
                readRequest.ErrorSteps(Undefined);
            }
        }

        var cancelPromise = PromiseHelper.CreateResolvedPromise(Engine, Undefined);

        // Return the result of reacting to sourceCancelPromise with a fulfillment step that returns undefined.
        if (Controller?.CancelAlgorithm is not null)
        {
            try
            {
                // Let sourceCancelPromise be ! stream.[[controller]].[[CancelSteps]](reason).
                var result = Controller.CancelAlgorithm.Call(Undefined, [reason]);
                if (result is not null && PromiseHelper.IsPromise(result))
                {
                    cancelPromise = PromiseHelper.CreateResolvedPromise(Engine, result);
                }
            }
            catch (JavaScriptException ex)
            {
                return PromiseHelper.CreateRejectedPromise(Engine, ex.Error).Promise;
            }
        }

        return cancelPromise.Promise;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#set-up-readable-stream-default-controller-from-underlying-source
    /// </summary>
    [MemberNotNull(nameof(Controller))]
    private void SetUpControllerFromUnderlyingSource(
        JsValue underlyingSource,
        ObjectInstance? underlyingSourceDict,
        double highWaterMark,
        Function? sizeAlgorithm
    )
    {
        var controller = _webApiIntrinsics.ReadableStreamDefaultController.Construct(
            this,
            highWaterMark,
            sizeAlgorithm
        );

        Controller = controller;

        // Extract algorithms from underlying source
        Function? startAlgorithm = null;
        Function? pullAlgorithm = null;
        Function? cancelAlgorithm = null;

        if (underlyingSourceDict is not null)
        {
            var start = underlyingSourceDict.Get("start");
            if (start is Function startFunc)
            {
                startAlgorithm = startFunc;
            }

            var pull = underlyingSourceDict.Get("pull");
            if (pull is Function pullFunc)
            {
                pullAlgorithm = pullFunc;
            }

            var cancel = underlyingSourceDict.Get("cancel");
            if (cancel is Function cancelFunc)
            {
                cancelAlgorithm = cancelFunc;
            }
        }

        controller.PullAlgorithm = pullAlgorithm;
        controller.CancelAlgorithm = cancelAlgorithm;

        // Call start algorithm if provided
        if (startAlgorithm is not null)
        {
            try
            {
                var result = startAlgorithm.Call(Undefined, [controller]);
                controller.Started = true;

                if (PromiseHelper.IsPromise(result))
                {
                    // Handle promise-based start
                    // For now, just mark as started
                    controller.CallPullIfNeeded();
                }
                else
                {
                    controller.CallPullIfNeeded();
                }
            }
            catch (JavaScriptException ex)
            {
                controller.ErrorInternal(ex.Error);
            }
        }
        else
        {
            controller.Started = true;
            controller.CallPullIfNeeded();
        }
    }

    /// <summary>
    /// Add a read request to the stream's reader
    /// https://streams.spec.whatwg.org/#readable-stream-add-read-request
    /// </summary>
    internal void AddReadRequest(ReadRequest readRequest)
    {
        // Assert: stream.[[reader]] implements ReadableStreamDefaultReader.
        // Assert: stream.[[state]] is "readable".
        if (
            Reader
            is not ReadableStreamDefaultReaderInstance
            {
                Stream.State: ReadableStreamState.Readable
            } reader
        )
        {
            return;
        }

        // Append readRequest to stream.[[reader]].[[readRequests]].
        reader.ReadRequests.Add(readRequest);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#set-up-readable-byte-stream-controller-from-underlying-source
    /// </summary>
    [MemberNotNull(nameof(Controller))]
    private void SetUpByteControllerFromUnderlyingSource(
        JsValue underlyingSource,
        ObjectInstance? underlyingSourceDict,
        double highWaterMark
    )
    {
        // Let controller be a new ReadableByteStreamController.
        var controller = _webApiIntrinsics.ReadableByteStreamController.Construct(this);

        // Let startAlgorithm be an algorithm that returns undefined.
        Function startAlgorithm = new ClrFunction(Engine, string.Empty, (_, _) => Undefined);

        var emptyPromiseFunction = new ClrFunction(
            Engine,
            string.Empty,
            (_, _) => PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise
        );
        // Let pullAlgorithm be an algorithm that returns a promise resolved with undefined.
        Function pullAlgorithm = emptyPromiseFunction;

        // Let cancelAlgorithm be an algorithm that returns a promise resolved with undefined.
        Function cancelAlgorithm = emptyPromiseFunction;

        double? autoAllocateChunkSize = null;
        if (underlyingSource.IsObject())
        {
            // If underlyingSourceDict["start"] exists, then set startAlgorithm to an algorithm which returns the result of invoking underlyingSourceDict["start"] with argument list « controller » and callback this value underlyingSource.
            var start = underlyingSource.Get("start");
            if (!start.IsUndefined())
            {
                startAlgorithm = start.AsFunctionInstance();
            }

            // If underlyingSourceDict["pull"] exists, then set pullAlgorithm to an algorithm which returns the result of invoking underlyingSourceDict["pull"] with argument list « controller » and callback this value underlyingSource.
            var pull = underlyingSource.Get("pull");
            if (!pull.IsUndefined())
            {
                pullAlgorithm = pull.AsFunctionInstance();
            }

            // If underlyingSourceDict["cancel"] exists, then set cancelAlgorithm to an algorithm which takes an argument reason and returns the result of invoking underlyingSourceDict["cancel"] with argument list « reason » and callback this value underlyingSource.
            var cancel = underlyingSource.Get("cancel");
            if (!cancel.IsUndefined())
            {
                cancelAlgorithm = cancel.AsFunctionInstance();
            }

            // Let autoAllocateChunkSize be underlyingSourceDict["autoAllocateChunkSize"], if it exists, or undefined otherwise.
            var autoAllocateChunkSizeProperty = underlyingSource.Get("autoAllocateChunkSize");
            if (!autoAllocateChunkSizeProperty.IsUndefined())
            {
                autoAllocateChunkSize = autoAllocateChunkSizeProperty.AsNumber();
            }
        }

        // If autoAllocateChunkSize is 0, then throw a TypeError exception.
        if (autoAllocateChunkSize == 0)
        {
            TypeErrorHelper.Throw(Engine, "autoAllocateChunkSize cannot be 0");
        }

        // Perform ? SetUpReadableByteStreamController(stream, controller, startAlgorithm, pullAlgorithm, cancelAlgorithm, highWaterMark, autoAllocateChunkSize).
        SetUpReadableByteStreamController(
            controller,
            startAlgorithm,
            pullAlgorithm,
            cancelAlgorithm,
            highWaterMark,
            autoAllocateChunkSize
        );
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#set-up-readable-byte-stream-controller
    /// </summary>
    [MemberNotNull(nameof(Controller))]
    private void SetUpReadableByteStreamController(
        ReadableByteStreamControllerInstance controller,
        Function startAlgorithm,
        Function pullAlgorithm,
        Function cancelAlgorithm,
        double highWaterMark,
        double? autoAllocateChunkSize
    )
    {
        // Assert: stream.[[controller]] is undefined.

        // If autoAllocateChunkSize is not undefined,
        if (autoAllocateChunkSize is not null)
        {
            // Assert: ! IsInteger(autoAllocateChunkSize) is true.
            if ((long)autoAllocateChunkSize.Value != autoAllocateChunkSize.Value)
            {
                TypeErrorHelper.Throw(Engine, "autoAllocateChunkSize must be an integer");
            }

            // Assert: autoAllocateChunkSize is positive.
            if (autoAllocateChunkSize.Value <= 0)
            {
                TypeErrorHelper.Throw(Engine, "autoAllocateChunkSize must be positive");
            }
        }

        // Set controller.[[stream]] to stream.
        // Set controller.[[pullAgain]] and controller.[[pulling]] to false.
        // Set controller.[[byobRequest]] to null.

        // Perform ! ResetQueue(controller).
        controller.ResetQueue();

        // Set controller.[[closeRequested]] and controller.[[started]] to false.

        // Set controller.[[strategyHWM]] to highWaterMark.
        controller.StrategyHWM = highWaterMark;
        // Set controller.[[pullAlgorithm]] to pullAlgorithm.
        controller.PullAlgorithm = pullAlgorithm;
        // Set controller.[[cancelAlgorithm]] to cancelAlgorithm.
        controller.CancelAlgorithm = cancelAlgorithm;
        // Set controller.[[autoAllocateChunkSize]] to autoAllocateChunkSize.
        controller.AutoAllocateChunkSize = autoAllocateChunkSize ?? 0;
        // Set controller.[[pendingPullIntos]] to a new empty list.

        // Set stream.[[controller]] to controller.
        Controller = controller;

        try
        {
            // Let startResult be the result of performing startAlgorithm.
            startAlgorithm.Call(Controller, [Controller]);

            // Let startPromise be a promise resolved with startResult.

            // Upon fulfillment of startPromise,
            // Set controller.[[started]] to true.
            Controller.Started = true;

            // Assert: controller.[[pulling]] is false.
            // Assert: controller.[[pullAgain]] is false.

            // Perform ! ReadableByteStreamControllerCallPullIfNeeded(controller).
            Controller.CallPullIfNeeded();
        }
        catch (JavaScriptException e)
        {
            // Upon rejection of startPromise with reason r,
            // Perform ! ReadableByteStreamControllerError(controller, r).
            Controller.ErrorInternal(e.Error);
        }
    }
}
