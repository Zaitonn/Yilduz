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
    public static JsValue DequeueValue(this IQueueEntriesContainer<QueueEntry> container)
    {
        // Assert: container has [[queue]] and [[queueTotalSize]] internal slots.

        // Assert: container.[[queue]] is not empty.
        if (container.Queue.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }

        // Let valueWithSize be container.[[queue]][0].
        // Remove valueWithSize from container.[[queue]].
        var entry = container.Queue.Dequeue();

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
    public static JsValue PeekQueueValue(this IQueueEntriesContainer<QueueEntry> container)
    {
        // Assert: container has [[queue]] and [[queueTotalSize]] internal slots.

        // Assert: container.[[queue]] is not empty.
        if (container.Queue.Count == 0)
        {
            throw new InvalidOperationException("Queue is empty");
        }

        // Let valueWithSize be container.[[queue]][0].
        // Return valueWithSize’s value.
        return container.Queue.Peek().Value;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#reset-queue
    /// </summary>
    public static void ResetQueue<T>(this IQueueEntriesContainer<T> container)
    {
        // Assert: container has [[queue]] and [[queueTotalSize]] internal slots.

        // Set container.[[queue]] to a new empty list.
        container.Queue.Clear();

        // Set container.[[queueTotalSize]] to 0.
        container.QueueTotalSize = 0;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#enqueue-value-with-size
    /// </summary>
    public static void EnqueueValueWithSize(
        this IQueueEntriesContainer<QueueEntry> container,
        Engine engine,
        JsValue value,
        double size
    )
    {
        // Assert: container has [[queue]] and [[queueTotalSize]] internal slots.

        // If ! IsNonNegativeNumber(size) is false, throw a RangeError exception.
        // If size is +∞, throw a RangeError exception.
        if (!Miscellaneous.IsNonNegativeNumber(size) || double.IsInfinity(size))
        {
            TypeErrorHelper.Throw(engine, "Size is invalid");
        }

        // Append a new value-with-size with value value and size size to container.[[queue]].
        container.Queue.Enqueue(new(value, size));

        // Set container.[[queueTotalSize]] to container.[[queueTotalSize]] + size.
        container.QueueTotalSize += size;
    }
}
