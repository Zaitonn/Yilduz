using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime;
using Yilduz.Streams.Queue;
using Yilduz.Streams.ReadableStream;

namespace Yilduz.Streams.ReadableStreamDefaultController;

public sealed partial class ReadableStreamDefaultControllerInstance
{
    List<QueueEntry> IQueueEntriesContainer.Queue => Queue;
    double IQueueEntriesContainer.QueueTotalSize
    {
        get => QueueTotalSize;
        set => QueueTotalSize = value;
    }

    internal readonly ReadableStreamInstance Stream;
    internal Function? PullAlgorithm;
    internal Function? CancelAlgorithm;
    internal Function? StrategySizeAlgorithm;
    internal readonly List<QueueEntry> Queue = [];
    internal double QueueTotalSize;
    internal readonly double StrategyHWM;
    internal bool CloseRequested;
    internal bool Started;
    internal bool Pulling;
    internal bool PullAgain;

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-default-controller-can-close-or-enqueue
    /// </summary>
    internal bool CanCloseOrEnqueue()
    {
        // Let state be controller.[[stream]].[[state]].
        var state = Stream.State;

        // If controller.[[closeRequested]] is false and state is "readable", return true.
        // Otherwise, return false.
        return !CloseRequested && state == ReadableStreamState.Readable;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-default-controller-close
    /// </summary>
    internal void CloseInternal()
    {
        // If ! ReadableStreamDefaultControllerCanCloseOrEnqueue(controller) is false, return.
        if (!CanCloseOrEnqueue())
        {
            return;
        }

        // Let stream be controller.[[stream]].
        // Set controller.[[closeRequested]] to true.
        CloseRequested = true;

        // If controller.[[queue]] is empty,
        if (Queue.Count == 0)
        {
            // Perform ! ReadableStreamDefaultControllerClearAlgorithms(controller).
            ClearAlgorithms();
            // Perform ! ReadableStreamClose(stream).
            Stream.CloseInternal();
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-default-controller-enqueue
    /// </summary>
    internal void EnqueueInternal(JsValue chunk)
    {
        // If ! ReadableStreamDefaultControllerCanCloseOrEnqueue(controller) is false, return.
        if (!CanCloseOrEnqueue())
        {
            return;
        }

        // Let stream be controller.[[stream]].
        // If ! IsReadableStreamLocked(stream) is true and ! ReadableStreamGetNumReadRequests(stream) > 0,
        //   perform ! ReadableStreamFulfillReadRequest(stream, chunk, false).
        if (Stream.HasDefaultReader && Stream.Reader!.ReadRequests.Count > 0)
        {
            Stream.FulfillReadRequest(chunk, false);
        }
        // Otherwise,
        else
        {
            // Let result be the result of performing controller.[[strategySizeAlgorithm]], passing in chunk, and interpreting the result as a completion record.
            var chunkSize = 1.0;
            try
            {
                if (StrategySizeAlgorithm != null)
                {
                    var result = StrategySizeAlgorithm.Call(Undefined, [chunk]);

                    // Let chunkSize be result.[[Value]].
                    chunkSize = TypeConverter.ToNumber(result);
                }
            }
            // If result is an abrupt completion,
            catch (JavaScriptException ex)
            {
                // Perform ! ReadableStreamDefaultControllerError(controller, result.[[Value]]).
                ErrorInternal(ex.Error);
                // Return result.
                throw;
            }

            try
            {
                // Let enqueueResult be EnqueueValueWithSize(controller, chunk, chunkSize).
                this.EnqueueValueWithSize(Engine, chunk, chunkSize);
            }
            catch (JavaScriptException ex)
            {
                // Perform ! ReadableStreamDefaultControllerError(controller, enqueueResult.[[Value]]).
                ErrorInternal(ex.Error);
                // Return enqueueResult.
                throw;
            }
        }

        // Perform ! ReadableStreamDefaultControllerCallPullIfNeeded(controller).
        CallPullIfNeeded();
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-default-controller-error
    /// </summary>
    internal void ErrorInternal(JsValue error)
    {
        // Let stream be controller.[[stream]].
        // If stream.[[state]] is not "readable", return.
        if (Stream.State != ReadableStreamState.Readable)
        {
            return;
        }

        // Perform ! ResetQueue(controller).
        this.ResetQueue();

        // Perform ! ReadableStreamDefaultControllerClearAlgorithms(controller).
        ClearAlgorithms();

        // Perform ! ReadableStreamError(stream, e).
        Stream.ErrorInternal(error);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-default-controller-should-call-pull
    /// </summary>
    internal bool ShouldCallPull()
    {
        // Let stream be controller.[[stream]].

        // If ! ReadableStreamDefaultControllerCanCloseOrEnqueue(controller) is false, return false.
        if (!CanCloseOrEnqueue())
        {
            return false;
        }

        // If controller.[[started]] is false, return false.
        if (!Started)
        {
            return false;
        }

        // If ! IsReadableStreamLocked(stream) is true and ! ReadableStreamGetNumReadRequests(stream) > 0, return true.
        if (Stream.Locked && Stream.Reader!.ReadRequests.Count > 0)
        {
            return true;
        }

        // Let desiredSize be ! ReadableStreamDefaultControllerGetDesiredSize(controller).
        // Assert: desiredSize is not null.
        // If desiredSize > 0, return true.
        // Return false.
        return DesiredSize > 0;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-default-controller-call-pull-if-needed
    /// </summary>
    internal void CallPullIfNeeded()
    {
        // Let shouldPull be ! ReadableStreamDefaultControllerShouldCallPull(controller).
        // If shouldPull is false, return.
        if (!ShouldCallPull())
        {
            return;
        }

        // If controller.[[pulling]] is true,
        if (Pulling)
        {
            // Set controller.[[pullAgain]] to true.
            // Return.
            PullAgain = true;
            return;
        }

        // Assert: controller.[[pullAgain]] is false.
        // Set controller.[[pulling]] to true.
        PullAgain = false;
        Pulling = true;

        if (PullAlgorithm != null)
        {
            try
            {
                // Let pullPromise be the result of performing controller.[[pullAlgorithm]].
                PullAlgorithm.Call().UnwrapIfPromise();
            }
            catch (JavaScriptException ex)
            {
                // Perform ! ReadableStreamDefaultControllerError(controller, e).
                ErrorInternal(ex.Error);
                return;
            }
        }
        // Set controller.[[pulling]] to false.
        Pulling = false;
        // If controller.[[pullAgain]] is true,
        if (PullAgain)
        {
            // Set controller.[[pullAgain]] to false.
            PullAgain = false;
            // Perform ! ReadableStreamDefaultControllerCallPullIfNeeded(controller).
            CallPullIfNeeded();
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-default-controller-clear-algorithms
    /// </summary>
    internal void ClearAlgorithms()
    {
        // Set controller.[[pullAlgorithm]] to undefined.
        PullAlgorithm = null;
        // Set controller.[[cancelAlgorithm]] to undefined.
        CancelAlgorithm = null;
        // Set controller.[[strategySizeAlgorithm]] to undefined.
        StrategySizeAlgorithm = null;
    }
}
