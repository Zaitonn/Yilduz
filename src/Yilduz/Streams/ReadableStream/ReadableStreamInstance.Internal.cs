using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Aborting.AbortSignal;
using Yilduz.Extensions;
using Yilduz.Streams.Queue;
using Yilduz.Streams.ReadableByteStreamController;
using Yilduz.Streams.ReadableStreamBYOBReader;
using Yilduz.Streams.ReadableStreamDefaultController;
using Yilduz.Streams.ReadableStreamDefaultReader;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStream;

public sealed partial class ReadableStreamInstance
{
    private readonly WebApiIntrinsics _webApiIntrinsics;
    internal ReadableStreamReader? Reader { get; set; }
    internal ReadableStreamController Controller { get; private set; }
    internal ReadableStreamState State { get; private set; } = ReadableStreamState.Readable;
    internal bool Detached { get; private set; }
    internal bool Disturbed { get; set; }
    internal JsValue StoredError { get; private set; } = Undefined;

    [MemberNotNullWhen(true, nameof(Reader))]
    internal bool HasDefaultReader => Reader is ReadableStreamDefaultReaderInstance;

    /// <summary>
    /// https://streams.spec.whatwg.org/#readablestream-enqueue
    /// </summary>
    internal void Enqueue(JsValue chunck)
    {
        switch (Controller)
        {
            // If stream.[[controller]] implements ReadableStreamDefaultController,
            case ReadableStreamDefaultControllerInstance defaultController:
                // Perform ! ReadableStreamDefaultControllerEnqueue(stream.[[controller]], chunk).
                defaultController.EnqueueInternal(chunck);
                break;

            // Otherwise,
            // Assert: stream.[[controller]] implements ReadableByteStreamController.
            case ReadableByteStreamControllerInstance byteController:
                // Assert: chunk is an ArrayBufferView.
                // Let byobView be the current BYOB request view for stream.
                // If byobView is non-null, and chunk.[[ViewedArrayBuffer]] is byobView.[[ViewedArrayBuffer]], then:
                //   Assert: chunk.[[ByteOffset]] is byobView.[[ByteOffset]].
                //   Assert: chunk.[[ByteLength]] ≤ byobView.[[ByteLength]].
                //   Perform ? ReadableByteStreamControllerRespond(stream.[[controller]], chunk.[[ByteLength]]).
                // Otherwise, perform ? ReadableByteStreamControllerEnqueue(stream.[[controller]], chunk).
                // byteController.EnqueueInternal(chunck);

                throw new NotSupportedException();

            default:
                throw new NotSupportedException();
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#initialize-readable-stream
    /// </summary>
    internal void InitializeReadableStream()
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
                if (result is not null && result.IsPromise())
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

        SetUpReadableStreamDefaultController(
            controller,
            startAlgorithm,
            pullAlgorithm,
            cancelAlgorithm,
            highWaterMark,
            sizeAlgorithm ?? new ClrFunction(Engine, string.Empty, (_, _) => 1)
        );
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#set-up-readable-stream-default-controller
    /// </summary>
    private void SetUpReadableStreamDefaultController(
        ReadableStreamDefaultControllerInstance controller,
        Function? startAlgorithm,
        Function? pullAlgorithm,
        Function? cancelAlgorithm,
        double highWaterMark,
        Function sizeAlgorithm
    )
    {
        // Assert: stream.[[controller]] is undefined.
        // Set controller.[[stream]] to stream.

        // Perform ! ResetQueue(controller).
        controller.ResetQueue();

        // Set controller.[[started]], controller.[[closeRequested]], controller.[[pullAgain]], and controller.[[pulling]] to false.
        controller.Started = false;
        controller.CloseRequested = false;
        controller.PullAgain = false;
        controller.Pulling = false;

        // Set controller.[[strategySizeAlgorithm]] to sizeAlgorithm and controller.[[strategyHWM]] to highWaterMark.
        controller.StrategySizeAlgorithm = sizeAlgorithm;
        controller.StrategyHWM = highWaterMark;

        // Set controller.[[pullAlgorithm]] to pullAlgorithm.
        // Set controller.[[cancelAlgorithm]] to cancelAlgorithm.
        controller.PullAlgorithm = pullAlgorithm;
        controller.CancelAlgorithm = cancelAlgorithm;

        if (startAlgorithm is not null)
        {
            try
            {
                // Let startResult be the result of performing startAlgorithm. (This might throw an exception.)
                var startResult = startAlgorithm.Call(Undefined, [controller]);

                // Let startPromise be a promise resolved with startResult.
                var startPromise = PromiseHelper.CreateResolvedPromise(Engine, startResult);

                // Upon fulfillment of startPromise,
                // Set controller.[[started]] to true
                controller.Started = true;

                // Perform ! ReadableStreamDefaultControllerCallPullIfNeeded(controller).
                controller.CallPullIfNeeded();
            }
            catch (JavaScriptException ex)
            {
                // Upon rejection of startPromise with reason r,
                // Perform ! ReadableStreamDefaultControllerError(controller, r).
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

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-tee
    /// </summary>
    /// <param name="cloneForBranch2">The second argument, cloneForBranch2, governs whether or not the data from the original stream will be cloned (using HTML’s serializable objects framework) before appearing in the second of the returned branches. This is useful for scenarios where both branches are to be consumed in such a way that they might otherwise interfere with each other, such as by transferring their chunks. However, it does introduce a noticeable asymmetry between the two branches, and limits the possible chunks to serializable ones.</param>
    private (ReadableStreamInstance, ReadableStreamInstance) TeeInternal(bool cloneForBranch2)
    {
        // If stream is a readable byte stream, then cloneForBranch2 is ignored and chunks are cloned unconditionally.

        // Assert: stream implements ReadableStream.
        // Assert: cloneForBranch2 is a boolean.

        // If stream.[[controller]] implements ReadableByteStreamController, return ? ReadableByteStreamTee(stream).
        if (Controller is ReadableByteStreamControllerInstance)
        {
            throw new NotImplementedException("Tee is not yet implemented");
        }

        // Return ? ReadableStreamDefaultTee(stream, cloneForBranch2).
        return DefaultTee(cloneForBranch2);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#abstract-opdef-readablestreamdefaulttee
    /// </summary>
    private (ReadableStreamInstance, ReadableStreamInstance) DefaultTee(bool cloneForBranch2)
    {
        // Assert: stream implements ReadableStream.
        // Assert: cloneForBranch2 is a boolean.

        // Let reader be ? AcquireReadableStreamDefaultReader(stream).
        var reader = AcquireDefaultReader();

        // Let reading be false.
        // Let readAgain be false.
        // Let canceled1 be false.
        // Let canceled2 be false.
        var reading = false;
        var readAgain = false;
        var canceled1 = false;
        var canceled2 = false;

        // Let reason1 be undefined.
        // Let reason2 be undefined.
        // Let branch1 be undefined.
        // Let branch2 be undefined.
        ReadableStreamInstance? branch1 = null;
        ReadableStreamInstance? branch2 = null;
        var reason1 = Undefined;
        var reason2 = Undefined;

        // Let cancelPromise be a new promise.
        var cancelPromise = Engine.Advanced.RegisterPromise();

        // Let pullAlgorithm be the following steps:
        Function pullAlgorithm = null!;
        pullAlgorithm = new ClrFunction(
            Engine,
            string.Empty,
            (_, _) =>
            {
                // If reading is true,
                if (reading)
                {
                    // Set readAgain to true.
                    // Return a promise resolved with undefined.
                    readAgain = true;
                    return PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise;
                }

                // Set reading to true.
                reading = true;

                // Let readRequest be a read request with the following items:
                var readRequest = new ReadRequest(
                    ChunkSteps: (chunk) =>
                    {
                        // Set readAgain to false.
                        readAgain = true;

                        // Let chunk1 and chunk2 be chunk.
                        var chunk1 = chunk;
                        var chunk2 = chunk;

                        // If canceled2 is false and cloneForBranch2 is true,
                        if (!canceled2 && cloneForBranch2)
                        {
                            try
                            {
                                // Let cloneResult be StructuredClone(chunk2).
                                chunk2 = chunk2.StructuredClone();
                            }
                            catch (JavaScriptException ex)
                            {
                                // If cloneResult is an abrupt completion,
                                //   Perform ! ReadableStreamDefaultControllerError(branch1.[[controller]], cloneResult.[[Value]]).
                                //   Perform ! ReadableStreamDefaultControllerError(branch2.[[controller]], cloneResult.[[Value]]).
                                //   Resolve cancelPromise with ! ReadableStreamCancel(stream, cloneResult.[[Value]]).
                                //   Return.
                                var error = ex.Error;

                                branch1?.Controller.ErrorInternal(error);
                                branch2?.Controller.ErrorInternal(error);

                                var cancelResult = CancelInternal(error);
                                cancelPromise.Resolve(cancelResult);
                                return;
                            }
                        }

                        // If canceled1 is false, perform ! ReadableStreamDefaultControllerEnqueue(branch1.[[controller]], chunk1).
                        if (!canceled1)
                        {
                            branch1?.Controller.EnqueueInternal(chunk1);
                        }

                        // If canceled2 is false, perform ! ReadableStreamDefaultControllerEnqueue(branch2.[[controller]], chunk2).
                        if (!canceled2)
                        {
                            branch2?.Controller.EnqueueInternal(chunk2);
                        }

                        // Set reading to false.
                        reading = false;

                        // If readAgain is true, perform pullAlgorithm.
                        if (readAgain)
                        {
                            readAgain = false;
                            pullAlgorithm.Call(Undefined, Arguments.Empty);
                        }
                    },
                    CloseSteps: (_) =>
                    {
                        // Set reading to false.
                        reading = false;

                        // If canceled1 is false, perform ! ReadableStreamDefaultControllerClose(branch1.[[controller]]).
                        if (!canceled1)
                        {
                            branch1?.Controller.CloseInternal();
                        }

                        // If canceled2 is false, perform ! ReadableStreamDefaultControllerClose(branch2.[[controller]]).
                        if (!canceled2)
                        {
                            branch2?.Controller.CloseInternal();
                        }

                        // If canceled1 is false or canceled2 is false, resolve cancelPromise with undefined.
                        if (!canceled1 || !canceled2)
                        {
                            cancelPromise.Resolve(Undefined);
                        }
                    },
                    // Set reading to false.
                    ErrorSteps: (_) => reading = false
                );

                // Perform ! ReadableStreamDefaultReaderRead(reader, readRequest).
                DefaultReaderRead(readRequest);

                // Return a promise resolved with undefined.
                return PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise;
            }
        );

        var cancel1Algorithm = new ClrFunction(
            Engine,
            string.Empty,
            (_, args) =>
            {
                // Set canceled1 to true.
                canceled1 = true;

                // Set reason1 to reason.
                reason1 = args.At(0);

                // If canceled2 is true,
                if (canceled2)
                {
                    // Let compositeReason be ! CreateArrayFromList(« reason1, reason2 »).
                    var compositeReason = Engine.Intrinsics.Array.Construct([reason1, reason2]);

                    // Let cancelResult be ! ReadableStreamCancel(stream, compositeReason).
                    var cancelResult = CancelInternal(compositeReason);

                    // Resolve cancelPromise with cancelResult.
                    cancelPromise.Resolve(cancelResult);
                }

                // Return cancelPromise.
                return cancelPromise.Promise;
            }
        );

        var cancel2Algorithm = new ClrFunction(
            Engine,
            string.Empty,
            (_, args) =>
            {
                // Set canceled2 to true.
                canceled2 = true;

                // Set reason2 to reason.
                reason2 = args.At(0);

                // If canceled1 is true,
                if (canceled1)
                {
                    // Let compositeReason be ! CreateArrayFromList(« reason1, reason2 »).
                    var compositeReason = Engine.Intrinsics.Array.Construct([reason1, reason2]);

                    // Let cancelResult be ! ReadableStreamCancel(stream, compositeReason).
                    var cancelResult = CancelInternal(compositeReason);

                    // Resolve cancelPromise with cancelResult.
                    cancelPromise.Resolve(cancelResult);
                }

                // Return cancelPromise.
                return cancelPromise.Promise;
            }
        );

        // Let startAlgorithm be an algorithm that returns undefined.
        var startAlgorithm = new ClrFunction(Engine, string.Empty, (_, _) => Undefined);

        // Set branch1 to ! CreateReadableStream(startAlgorithm, pullAlgorithm, cancel1Algorithm).
        branch1 = CreateReadableStream(startAlgorithm, pullAlgorithm, cancel1Algorithm);
        // Set branch2 to ! CreateReadableStream(startAlgorithm, pullAlgorithm, cancel2Algorithm).
        branch2 = CreateReadableStream(startAlgorithm, pullAlgorithm, cancel2Algorithm);

        // Upon rejection of reader.[[closedPromise]] with reason r,
        if (reader.ClosedPromise.Promise.TryGetRejectedValue(out var r))
        {
            // Perform ! ReadableStreamDefaultControllerError(branch1.[[controller]], r).
            Controller.ErrorInternal(r);
            // Perform ! ReadableStreamDefaultControllerError(branch2.[[controller]], r).
            Controller.ErrorInternal(r);
        }

        return (branch1, branch2);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-default-reader-read
    /// </summary>
    private void DefaultReaderRead(ReadRequest readRequest)
    {
        // Let stream be reader.[[stream]].
        // Assert: stream is not undefined.

        // Set stream.[[disturbed]] to true.
        Disturbed = true;

        // If stream.[[state]] is "closed", perform readRequest’s close steps.
        if (State == ReadableStreamState.Closed)
        {
            readRequest.CloseSteps(Undefined);
        }
        // Otherwise, if stream.[[state]] is "errored", perform readRequest’s error steps given stream.[[storedError]].

        else if (State == ReadableStreamState.Errored)
        {
            readRequest.ErrorSteps(StoredError);
        }
        else
        {
            // Otherwise,
            //   Assert: stream.[[state]] is "readable".
            //   Perform ! stream.[[controller]].[[PullSteps]](readRequest).
            Controller.PullSteps(readRequest);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-default-controller-error
    /// </summary>
    private void DefaultControllerError(JsValue e)
    {
        // Let stream be controller.[[stream]].

        // If stream.[[state]] is not "readable", return.
        if (State != ReadableStreamState.Readable)
        {
            return;
        }

        // Perform ! ResetQueue(controller).
        switch (Controller)
        {
            case ReadableStreamDefaultControllerInstance defaultController:
                defaultController.ResetQueue();
                break;

            case ReadableByteStreamControllerInstance byteController:
                byteController.ResetQueue();
                break;
        }

        // Perform ! ReadableStreamDefaultControllerClearAlgorithms(controller).
        Controller.ClearAlgorithms();

        // Perform ! ReadableStreamError(stream, e).
        ErrorInternal(e);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#create-readable-stream
    /// </summary>
    private ReadableStreamInstance CreateReadableStream(
        Function startAlgorithm,
        Function pullAlgorithm,
        Function cancelAlgorithm,
        double highWaterMark = 1,
        Function? sizeAlgorithm = null
    )
    {
        // If highWaterMark was not passed, set it to 1.
        // If sizeAlgorithm was not passed, set it to an algorithm that returns 1.
        sizeAlgorithm ??= new ClrFunction(Engine, string.Empty, (_, _) => 1);

        // Assert: ! IsNonNegativeNumber(highWaterMark) is true.
        if (!Miscellaneous.IsNonNegativeNumber(highWaterMark))
        {
            TypeErrorHelper.Throw(Engine, "highWaterMark must be a non-negative number");
        }

        // Let stream be a new ReadableStream.
        var stream = (ReadableStreamInstance)
            _webApiIntrinsics.ReadableStream.Construct(Arguments.Empty, Undefined);

        // Perform ! InitializeReadableStream(stream).
        stream.InitializeReadableStream();

        // Let controller be a new ReadableStreamDefaultController.
        var controller = _webApiIntrinsics.ReadableStreamDefaultController.Construct(
            stream,
            1, // highWaterMark
            sizeAlgorithm // sizeAlgorithm
        );

        // Perform ? SetUpReadableStreamDefaultController(stream, controller, startAlgorithm, pullAlgorithm, cancelAlgorithm, highWaterMark, sizeAlgorithm).
        SetUpReadableStreamDefaultController(
            controller,
            startAlgorithm,
            pullAlgorithm,
            cancelAlgorithm,
            highWaterMark,
            sizeAlgorithm
        );

        return stream;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-pipe-to
    /// </summary>
    private JsValue PipeToInternal(
        WritableStreamInstance destination,
        bool preventClose,
        bool preventAbort,
        bool preventCancel,
        AbortSignalInstance? signal
    )
    {
        // Assert: source implements ReadableStream.
        // Assert: dest implements WritableStream.
        // Assert: preventClose, preventAbort, and preventCancel are all booleans.
        // If signal was not given, let signal be undefined.
        // Assert: either signal is undefined, or signal implements AbortSignal.

        // Assert: ! IsReadableStreamLocked(source) is false.
        if (Locked)
        {
            TypeErrorHelper.Throw(Engine, "ReadableStream is locked");
        }

        // Assert: ! IsWritableStreamLocked(dest) is false.
        if (destination.Locked)
        {
            TypeErrorHelper.Throw(Engine, "WritableStream is locked");
        }

        // If source.[[controller]] implements ReadableByteStreamController, let reader be either ! AcquireReadableStreamBYOBReader(source) or ! AcquireReadableStreamDefaultReader(source), at the user agent’s discretion.
        // Otherwise, let reader be ! AcquireReadableStreamDefaultReader(source).
        ReadableStreamReader reader;

        if (Controller is ReadableByteStreamControllerInstance)
        {
            // At the user agent's discretion, we choose BYOB reader here
            reader = AcquireBYOBReader();
        }
        else
        {
            reader = AcquireDefaultReader();
        }

        // Let writer be ! AcquireWritableStreamDefaultWriter(dest).
        var writer = destination.AcquireWriter();

        // Set source.[[disturbed]] to true.
        Disturbed = true;

        // Let shuttingDown be false.
        var shuttingDown = false;

        // Let promise be a new promise.
        var promise = Engine.Advanced.RegisterPromise();

        // Track current writes for shutdown
        var currentWrites = new List<JsValue>();

        void Finalize(JsValue? error)
        {
            writer.Release();
            reader.Release();

            if (error is not null)
            {
                promise.Reject(error);
            }
            else
            {
                promise.Resolve(Undefined);
            }
        }

        void ShutdownWithAction(Func<JsValue> action, JsValue? originalError)
        {
            if (shuttingDown)
            {
                return;
            }
            shuttingDown = true;

            var writesPromise = PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise;
            if (
                destination.State == WritableStreamState.Writable
                && !destination.IsCloseQueuedOrInFlight
                && currentWrites.Count > 0
            )
            {
                writesPromise = PromiseHelper.All(Engine, currentWrites);
            }

            var writesThen = writesPromise.Get("then");
            writesThen.Call(
                writesPromise,
                [
                    new ClrFunction(
                        Engine,
                        "",
                        (_, _) =>
                        {
                            var p = action();
                            if (p.IsPromise())
                            {
                                var pThen = p.Get("then");
                                pThen.Call(
                                    p,
                                    [
                                        new ClrFunction(
                                            Engine,
                                            "",
                                            (_, _) =>
                                            {
                                                Finalize(originalError);
                                                return Undefined;
                                            }
                                        ),
                                        new ClrFunction(
                                            Engine,
                                            "",
                                            (_, args) =>
                                            {
                                                Finalize(args[0]);
                                                return Undefined;
                                            }
                                        ),
                                    ]
                                );
                            }
                            else
                            {
                                Finalize(originalError);
                            }
                            return Undefined;
                        }
                    ),
                    new ClrFunction(
                        Engine,
                        "",
                        (_, args) =>
                        {
                            Finalize(args[0]);
                            return Undefined;
                        }
                    ),
                ]
            );
        }

        void Shutdown(JsValue? error)
        {
            if (shuttingDown)
                return;
            shuttingDown = true;

            var writesPromise = PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise;
            if (
                destination.State == WritableStreamState.Writable
                && !destination.IsCloseQueuedOrInFlight
                && currentWrites.Count > 0
            )
            {
                writesPromise = PromiseHelper.All(Engine, currentWrites);
            }

            var writesThen = writesPromise.Get("then");
            writesThen.Call(
                writesPromise,
                [
                    new ClrFunction(
                        Engine,
                        "",
                        (_, _) =>
                        {
                            Finalize(error);
                            return Undefined;
                        }
                    ),
                    new ClrFunction(
                        Engine,
                        "",
                        (_, args) =>
                        {
                            Finalize(args[0]);
                            return Undefined;
                        }
                    ),
                ]
            );
        }

        Action loop = null!;
        loop = () =>
        {
            if (shuttingDown)
            {
                return;
            }

            if (destination.State == WritableStreamState.Errored)
            {
                var destError = destination.StoredError;
                if (!preventCancel)
                {
                    ShutdownWithAction(() => CancelInternal(destError), destError);
                }
                else
                {
                    Shutdown(destError);
                }
                return;
            }

            if (
                destination.State == WritableStreamState.Closed
                || destination.IsCloseQueuedOrInFlight
            )
            {
                var destClosed = Engine.Intrinsics.TypeError.Construct("Destination stream closed");
                if (!preventCancel)
                {
                    ShutdownWithAction(() => CancelInternal(destClosed), destClosed);
                }
                else
                {
                    Shutdown(destClosed);
                }
                return;
            }

            if (State == ReadableStreamState.Errored)
            {
                var sourceError = StoredError;
                if (!preventAbort)
                {
                    ShutdownWithAction(() => destination.AbortInternal(sourceError), sourceError);
                }
                else
                {
                    Shutdown(sourceError);
                }
                return;
            }

            if (State == ReadableStreamState.Closed)
            {
                if (!preventClose)
                {
                    ShutdownWithAction(() => writer.Close(), null);
                }
                else
                {
                    Shutdown(null);
                }
                return;
            }

            var desiredSize = writer.DesiredSize;
            if (!desiredSize.HasValue || desiredSize.Value <= 0)
            {
                var ready = writer.Ready;
                if (ready.IsPromise())
                {
                    ready
                        .Get("then")
                        .Call(
                            ready,
                            [
                                new ClrFunction(
                                    Engine,
                                    "",
                                    (_, _) =>
                                    {
                                        loop();
                                        return Undefined;
                                    }
                                ),
                                new ClrFunction(
                                    Engine,
                                    "",
                                    (_, _) =>
                                    {
                                        loop();
                                        return Undefined;
                                    }
                                ),
                            ]
                        );
                }
                else
                {
                    loop();
                }
                return;
            }

            if (reader is ReadableStreamDefaultReaderInstance defaultReader)
            {
                var req = new ReadRequest(
                    ChunkSteps: (chunk) =>
                    {
                        var writePromise = writer.WriteInternal(chunk);
                        currentWrites.Add(writePromise);

                        var cleanup = new ClrFunction(
                            Engine,
                            "",
                            (_, _) =>
                            {
                                currentWrites.Remove(writePromise);
                                return Undefined;
                            }
                        );

                        if (writePromise.IsPromise())
                        {
                            writePromise.Get("then").Call(writePromise, [cleanup, cleanup]);
                        }

                        loop();
                    },
                    CloseSteps: (_) =>
                    {
                        if (!preventClose)
                        {
                            ShutdownWithAction(() => writer.Close(), null);
                        }
                        else
                        {
                            Shutdown(null);
                        }
                    },
                    ErrorSteps: (e) =>
                    {
                        if (!preventAbort)
                        {
                            ShutdownWithAction(() => destination.AbortInternal(e), e);
                        }
                        else
                        {
                            Shutdown(e);
                        }
                    }
                );

                defaultReader.ReadRequests.Add(req);
                Controller.CallPullIfNeeded();
            }
        };

        if (signal is not null)
        {
            if (signal.Aborted)
            {
                AbortAlgorithm();
                return promise.Promise;
            }

            signal.Abort += (_, _) => AbortAlgorithm();

            JsValue AbortAlgorithm()
            {
                var error = signal.Reason;
                var actions = new List<Func<JsValue>>();

                if (!preventAbort)
                {
                    actions.Add(() =>
                    {
                        if (destination.State == WritableStreamState.Writable)
                        {
                            return destination.AbortInternal(error);
                        }
                        return PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise;
                    });
                }

                if (!preventCancel)
                {
                    actions.Add(() =>
                    {
                        if (State == ReadableStreamState.Readable)
                        {
                            return CancelInternal(error);
                        }
                        return PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise;
                    });
                }

                ShutdownWithAction(
                    () =>
                    {
                        var promises = actions.Select(a => a());
                        return PromiseHelper.All(Engine, promises);
                    },
                    error
                );

                return PromiseHelper.CreateResolvedPromise(Engine, Undefined).Promise;
            }
        }

        loop();

        return promise.Promise;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#rs-default-controller-has-backpressure
    /// </summary>
    internal bool HasBackpressure()
    {
        // If ! ReadableStreamDefaultControllerShouldCallPull(controller) is true, return false.
        if (((ReadableStreamDefaultControllerInstance)Controller).ShouldCallPull())
        {
            return false;
        }

        // Otherwise, return true.
        return true;
    }
}
