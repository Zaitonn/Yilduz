using System;
using Jint;
using Jint.Native;
using Yilduz.Utils;

namespace Yilduz.Streams.Queue;

internal static class AbstractOperations
{
    /// <summary>
    /// https://streams.spec.whatwg.org/#dequeue-value
    /// </summary>
    public static JsValue DequeueValue(this IQueueEntriesContainer container)
    {
        // Assert: container has [[queue]] and [[queueTotalSize]] internal slots.

        // Assert: container.[[queue]] is not empty.
        if (container.Queue.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }

        // Let valueWithSize be container.[[queue]][0].
        var entry = container.Queue[0];

        // Remove valueWithSize from container.[[queue]].
        container.Queue.RemoveAt(0);

        // Set container.[[queueTotalSize]] to container.[[queueTotalSize]] − valueWithSize’s size.
        container.QueueTotalSize -= entry.Size;

        // If container.[[queueTotalSize]] < 0, set container.[[queueTotalSize]] to 0. (This can occur due to rounding errors.)
        if (container.QueueTotalSize < 0)
        {
            container.QueueTotalSize = 0;
        }

        // Return valueWithSize’s value.
        return entry.Value;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#peek-queue-value
    /// </summary>
    public static JsValue PeekQueueValue(this IQueueEntriesContainer container)
    {
        // Assert: container has [[queue]] and [[queueTotalSize]] internal slots.

        // Assert: container.[[queue]] is not empty.
        if (container.Queue.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }

        // Let valueWithSize be container.[[queue]][0].
        // Return valueWithSize’s value.
        return container.Queue[0].Value;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#reset-queue
    /// </summary>
    public static void ResetQueue(this IQueueEntriesContainer container)
    {
        // Assert: container has [[queue]] and [[queueTotalSize]] internal slots.

        // Set container.[[queue]] to a new empty list.
        container.Queue.Clear();

        // Set container.[[queueTotalSize]] to 0.
        container.QueueTotalSize = 0;
    }

    public static void EnqueueValueWithSize(
        this IQueueEntriesContainer container,
        Engine engine,
        JsValue value,
        double size
    )
    {
        if (double.IsNaN(size) || size < 0 || double.IsInfinity(size))
        {
            TypeErrorHelper.Throw(engine, "Size is invalid");
        }

        container.Queue.Add(new(value, size));
        container.QueueTotalSize += size;
    }
}
