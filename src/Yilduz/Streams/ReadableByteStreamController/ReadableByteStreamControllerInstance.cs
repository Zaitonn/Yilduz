using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Streams.Queue;
using Yilduz.Streams.ReadableStream;
using Yilduz.Streams.ReadableStreamBYOBRequest;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableByteStreamController;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/ReadableByteStreamController
/// </summary>
public sealed class ReadableByteStreamControllerInstance
    : ReadableStreamController,
        IQueueEntriesContainer<ReadableByteStreamQueueEntry>
{
    internal ReadableByteStreamControllerInstance(Engine engine, ReadableStreamInstance stream)
        : base(engine, stream)
    {
        _webApiIntrinsics = engine.GetWebApiIntrinsics();
    }

    private readonly WebApiIntrinsics _webApiIntrinsics;
    internal ReadableStreamBYOBRequestInstance? _byobRequest;

    /// <summary>
    /// https://streams.spec.whatwg.org/#readablebytestreamcontroller-queuetotalsize
    /// </summary>
    internal double QueueTotalSize { get; set; }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readablebytestreamcontroller-queue
    /// </summary>
    internal Queue<ReadableByteStreamQueueEntry> Queue { get; } = [];

    /// <summary>
    /// https://streams.spec.whatwg.org/#readablebytestreamcontroller-pendingpullintos
    /// </summary>
    internal List<PullIntoDescriptor> PendingPullIntos { get; } = [];

    /// <summary>
    /// https://streams.spec.whatwg.org/#readablebytestreamcontroller-autoallocatechunksize
    /// </summary>
    internal double AutoAllocateChunkSize { get; set; } = 0;

    /// <summary>
    /// https://streams.spec.whatwg.org/#readablebytestreamcontroller-byobrequest
    /// </summary>
    public ReadableStreamBYOBRequestInstance? BYOBRequest
    {
        get
        {
            //If controller.[[byobRequest]] is null and controller.[[pendingPullIntos]] is not empty,
            if (_byobRequest is null && PendingPullIntos.Count > 0)
            {
                // Let firstDescriptor be controller.[[pendingPullIntos]][0].
                var firstDescriptor = PendingPullIntos[0];

                // Let view be ! Construct(%Uint8Array%, « firstDescriptor’s buffer, firstDescriptor’s byte offset + firstDescriptor’s bytes filled, firstDescriptor’s byte length − firstDescriptor’s bytes filled »).
                var view = Engine.Intrinsics.Uint8Array.Construct(
                    firstDescriptor.Buffer!,
                    firstDescriptor.ByteOffset + firstDescriptor.BytesFilled,
                    firstDescriptor.ByteLength - firstDescriptor.BytesFilled
                );

                // Let byobRequest be a new ReadableStreamBYOBRequest.
                // Set byobRequest.[[controller]] to controller.
                // Set byobRequest.[[view]] to view.
                // Set controller.[[byobRequest]] to byobRequest.
                var byobRequest = _webApiIntrinsics.ReadableStreamBYOBRequest.Construct(this, view);
                _byobRequest = byobRequest;
            }

            return _byobRequest;
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#rbs-controller-desired-size
    /// <br/>
    /// https://streams.spec.whatwg.org/#readable-byte-stream-controller-get-desired-size
    /// </summary>
    public double? DesiredSize
    {
        get
        {
            // Let state be controller.[[stream]].[[state]].

            // If state is "errored", return null.
            if (Stream.State == ReadableStreamState.Errored)
            {
                return null;
            }

            // If state is "closed", return 0.
            if (Stream.State == ReadableStreamState.Closed)
            {
                return 0;
            }

            // Return controller.[[strategyHWM]] − controller.[[queueTotalSize]].
            return StrategyHWM - QueueTotalSize;
        }
    }

    Queue<ReadableByteStreamQueueEntry> IQueueEntriesContainer<ReadableByteStreamQueueEntry>.Queue =>
        Queue;

    double IQueueEntriesContainer<ReadableByteStreamQueueEntry>.QueueTotalSize
    {
        get => QueueTotalSize;
        set => QueueTotalSize = value;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/ReadableByteStreamController/close
    /// <br/>
    /// https://streams.spec.whatwg.org/#rbs-controller-close
    /// </summary>
    public void Close()
    {
        if (CloseRequested)
        {
            TypeErrorHelper.Throw(Engine, "The stream is already closed.");
        }

        if (Stream.State != ReadableStreamState.Readable)
        {
            TypeErrorHelper.Throw(Engine, "The stream is not in a state that can be closed.");
        }

        CloseInternal();
    }

    internal override void CallPullIfNeeded()
    {
        throw new NotImplementedException();
    }

    internal override JsValue CancelSteps(JsValue reason)
    {
        throw new NotImplementedException();
    }

    internal override void CloseInternal()
    { // Let stream be controller.[[stream]].
        // If controller.[[closeRequested]] is true or stream.[[state]] is not "readable", return.
        if (CloseRequested && Stream.State != ReadableStreamState.Readable)
        {
            return;
        }

        // If controller.[[queueTotalSize]] > 0,
        if (QueueTotalSize > 0)
        {
            // Set controller.[[closeRequested]] to true.
            CloseRequested = true;
            // Return.

            return;
        }

        // If controller.[[pendingPullIntos]] is not empty,
        if (PendingPullIntos.Count > 0)
        {
            // Let firstPendingPullInto be controller.[[pendingPullIntos]][0].
            var firstPendingPullInto = PendingPullIntos[0];

            // If the remainder after dividing firstPendingPullInto’s bytes filled by firstPendingPullInto’s element size is not 0,
            if (firstPendingPullInto.BytesFilled % firstPendingPullInto.ElementSize != 0)
            {
                // Let e be a new TypeError exception.
                var e = Engine.Intrinsics.TypeError.Construct(
                    "BYOB request has a partially filled buffer that cannot be closed."
                );

                // Perform ! ReadableByteStreamControllerError(controller, e).
                ErrorInternal(e);

                // Throw e.
                throw new JavaScriptException(e);
            }

            // Perform ! ReadableByteStreamControllerClearAlgorithms(controller).
            ClearAlgorithms();

            // Perform ! ReadableStreamClose(stream).
            Stream.CloseInternal();
        }
    }

    internal override void EnqueueInternal(JsValue chunk)
    {
        throw new NotImplementedException();
    }

    internal override void ErrorInternal(JsValue error)
    {
        throw new NotImplementedException();
    }

    internal override void PullSteps(ReadRequest readRequest)
    {
        throw new NotImplementedException();
    }

    internal override void ReleaseSteps()
    {
        throw new NotImplementedException();
    }
}
