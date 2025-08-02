using System.Collections.Generic;

namespace Yilduz.Streams.Queue;

internal interface IQueueEntriesContainer<T>
{
    internal Queue<T> Queue { get; }
    internal double QueueTotalSize { get; set; }
}
