using System.Collections.Generic;

namespace Yilduz.Streams.Queue;

internal interface IQueueEntriesContainer
{
    public List<QueueEntry> Queue { get; }
    public double QueueTotalSize { get; set; }
}
