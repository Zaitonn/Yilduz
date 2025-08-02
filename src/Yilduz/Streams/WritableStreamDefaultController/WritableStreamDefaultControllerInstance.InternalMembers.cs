using System.Collections.Generic;
using Jint.Native.Function;
using Yilduz.Aborting.AbortController;
using Yilduz.Streams.Queue;
using Yilduz.Streams.WritableStream;

namespace Yilduz.Streams.WritableStreamDefaultController;

public sealed partial class WritableStreamDefaultControllerInstance
    : IQueueEntriesContainer<QueueEntry>
{
    internal Queue<QueueEntry> Queue { get; } = [];
    internal double QueueTotalSize { get; set; }
    Queue<QueueEntry> IQueueEntriesContainer<QueueEntry>.Queue => Queue;
    double IQueueEntriesContainer<QueueEntry>.QueueTotalSize
    {
        get => QueueTotalSize;
        set => QueueTotalSize = value;
    }

    /// <summary>
    /// Internal slots as defined in the WHATWG Streams specification
    /// https://streams.spec.whatwg.org/#ws-default-controller-internal-slots
    /// </summary>

    /// <summary>
    /// [[abortAlgorithm]] - A promise-returning algorithm, taking one argument (the abort reason)
    /// </summary>
    internal Function AbortAlgorithm { get; set; }

    /// <summary>
    /// [[closeAlgorithm]] - A promise-returning algorithm, taking no arguments
    /// </summary>
    internal Function CloseAlgorithm { get; set; }

    /// <summary>
    /// [[stream]] - The WritableStream instance controlled by this object
    /// </summary>
    internal WritableStreamInstance Stream { get; }

    /// <summary>
    /// [[started]] - A boolean flag indicating whether the underlying sink's start method has finished
    /// </summary>
    internal bool Started { get; set; } = false;

    /// <summary>
    /// [[strategyHWM]] - A number supplied by the creator as part of the stream's queuing strategy
    /// </summary>
    internal double StrategyHWM { get; init; } = 1;

    /// <summary>
    /// [[strategySizeAlgorithm]] - An algorithm to calculate the size of enqueued chunks
    /// </summary>
    internal Function StrategySizeAlgorithm { get; init; }

    /// <summary>
    /// [[writeAlgorithm]] - A promise-returning algorithm, taking one argument (the chunk to write)
    /// </summary>
    internal Function WriteAlgorithm { get; set; }

    /// <summary>
    /// [[abortController]] - An AbortController instance for signaling abort
    /// </summary>
    internal AbortControllerInstance AbortController { get; }
}
