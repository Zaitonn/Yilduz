using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Streams.Queue;
using Yilduz.Streams.ReadableStream;
using Yilduz.Streams.ReadableStreamDefaultReader;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamDefaultController;

public sealed partial class ReadableStreamDefaultControllerInstance
{
    internal Queue<QueueEntry> Queue { get; } = [];
    internal double QueueTotalSize { get; set; }

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
    internal override void CloseInternal()
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
    internal override void EnqueueInternal(JsValue chunk)
    {
        // If ! ReadableStreamDefaultControllerCanCloseOrEnqueue(controller) is false, return.
        if (!CanCloseOrEnqueue())
        {
            return;
        }

        // Let stream be controller.[[stream]].
        // If ! IsReadableStreamLocked(stream) is true and ! ReadableStreamGetNumReadRequests(stream) > 0,
        //   perform ! ReadableStreamFulfillReadRequest(stream, chunk, false).
        if (
            Stream.Locked
            && Stream.Reader is ReadableStreamDefaultReaderInstance reader
            && reader.ReadRequests.Count > 0
        )
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
    internal override void ErrorInternal(JsValue error)
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
        if (
            Stream.Locked
            && ((ReadableStreamDefaultReaderInstance)Stream.Reader).ReadRequests.Count > 0
        )
        {
            return true;
        }

        // Let desiredSize be ! ReadableStreamDefaultControllerGetDesiredSize(controller).
        // Assert: desiredSize is not null.
        if (DesiredSize is null)
        {
            throw new InvalidOperationException("Desired size should not be null");
        }

        // If desiredSize > 0, return true.
        // Return false.
        return DesiredSize > 0;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#readable-stream-default-controller-call-pull-if-needed
    /// </summary>
    internal override void CallPullIfNeeded()
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
        if (PullAgain)
        {
            throw new InvalidOperationException("Controller pullAgain should be false");
        }

        // Set controller.[[pulling]] to true.
        PullAgain = false;
        Pulling = true;

        PullAlgorithm
            ?.Call(this)
            .Then(
                onFulfilled: _ =>
                {
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
                    return Undefined;
                },
                onRejected: e =>
                {
                    // Perform ! ReadableStreamDefaultControllerError(controller, e).
                    ErrorInternal(e);
                    return Undefined;
                }
            );
    }

    internal override JsValue CancelSteps(JsValue reason)
    {
        // Perform ! ResetQueue(this).
        this.ResetQueue();

        // Let result be the result of performing this.[[cancelAlgorithm]], passing reason.
        JsValue? result;
        try
        {
            result = CancelAlgorithm?.Call(Undefined, [reason]);
        }
        catch (JavaScriptException ex)
        {
            // If result is an abrupt completion, perform ! ReadableStreamDefaultControllerError(this, result.[[Value]]).
            ErrorInternal(ex.Error);
            // Return result.
            throw;
        }

        // Perform ! ReadableStreamDefaultControllerClearAlgorithms(this).
        ClearAlgorithms();

        // Return result.
        return result ?? Undefined;
    }

    internal override void PullSteps(ReadRequest readRequest)
    {
        // Let stream be this.[[stream]].
        // If this.[[queue]] is not empty,
        if (Queue.Count > 0)
        {
            // Let chunk be ! DequeueValue(this).
            var chunk = this.DequeueValue();

            // If this.[[closeRequested]] is true and this.[[queue]] is empty,
            if (CloseRequested && Queue.Count == 0)
            {
                // ! ReadableStreamDefaultControllerClearAlgorithms(this).
                ClearAlgorithms();
                // Perform ! ReadableStreamClose(stream).
                Stream.CloseInternal();
            }
            // Otherwise, perform ! ReadableStreamDefaultControllerCallPullIfNeeded(this).
            else
            {
                CallPullIfNeeded();
            }
            // Perform readRequest’s chunk steps, given chunk.
            readRequest.ChunkSteps(chunk);
        }
        // Otherwise,
        else
        {
            // Perform ! ReadableStreamAddReadRequest(stream, readRequest).
            Stream.AddReadRequest(readRequest);

            // Perform ! ReadableStreamDefaultControllerCallPullIfNeeded(this).
            CallPullIfNeeded();
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#abstract-opdef-readablestreamdefaultcontroller-releasesteps
    /// </summary>
    internal override void ReleaseSteps()
    {
        // Return.
        return;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#rs-default-controller-has-backpressure
    /// </summary>
    internal bool HasBackpressure()
    {
        // If ! ReadableStreamDefaultControllerShouldCallPull(controller) is true, return false.
        if (ShouldCallPull())
        {
            return false;
        }

        // Return true.
        return true;
    }
}
