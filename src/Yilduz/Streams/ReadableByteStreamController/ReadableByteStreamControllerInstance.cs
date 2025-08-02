using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;
using Yilduz.Streams.Queue;
using Yilduz.Streams.ReadableStream;

namespace Yilduz.Streams.ReadableByteStreamController;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/ReadableByteStreamController
/// </summary>
public sealed class ReadableByteStreamControllerInstance
    : ReadableStreamController,
        IQueueEntriesContainer<ReadableByteStreamQueueEntry>
{
    internal ReadableByteStreamControllerInstance(Engine engine, ReadableStreamInstance stream)
        : base(engine, stream) { }

    internal double QueueTotalSize { get; set; }
    internal Queue<ReadableByteStreamQueueEntry> Queue { get; } = [];
    internal List<PullIntoDescriptor> PendingPullIntos { get; } = [];
    internal double AutoAllocateChunkSize { get; set; } = 0;

    Queue<ReadableByteStreamQueueEntry> IQueueEntriesContainer<ReadableByteStreamQueueEntry>.Queue =>
        Queue;

    double IQueueEntriesContainer<ReadableByteStreamQueueEntry>.QueueTotalSize
    {
        get => QueueTotalSize;
        set => QueueTotalSize = value;
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
