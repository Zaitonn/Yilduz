using System.Collections.Generic;
using Jint.Native.Function;
using Yilduz.Aborting.AbortController;
using Yilduz.Streams.Queue;
using Yilduz.Streams.WritableStream;

namespace Yilduz.Streams.WritableStreamDefaultController;

public sealed partial class WritableStreamDefaultControllerInstance
{
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
    /// [[queue]] - A list representing the stream's internal queue of chunks
    /// </summary>
    internal List<QueueEntry> Queue { get; set; } = [];

    /// <summary>
    /// [[queueTotalSize]] - The total size of all the chunks stored in [[queue]]
    /// </summary>
    internal double QueueTotalSize { get; set; }

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

    List<QueueEntry> IQueueEntriesContainer.Queue => Queue;

    double IQueueEntriesContainer.QueueTotalSize
    {
        get => QueueTotalSize;
        set => QueueTotalSize = value;
    }
}
