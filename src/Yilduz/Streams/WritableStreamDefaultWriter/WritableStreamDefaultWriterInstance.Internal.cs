using System;
using Jint;
using Jint.Native;
using Jint.Native.Promise;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStreamDefaultWriter;

public sealed partial class WritableStreamDefaultWriterInstance
{
    /// <summary>
    /// [[closedPromise]] - A promise returned by the writer's closed getter
    /// </summary>
    internal ManualPromise? ClosedPromise { get; set; }

    /// <summary>
    /// [[readyPromise]] - A promise returned by the writer's ready getter
    /// </summary>
    internal ManualPromise? ReadyPromise { get; set; }

    /// <summary>
    /// [[stream]] - A WritableStream instance that owns this reader
    /// </summary>
    internal WritableStreamInstance? Stream { get; set; }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-writer-abort
    /// </summary>
    internal JsValue AbortInternal(JsValue reason)
    {
        // Let stream be writer.[[stream]].
        // Assert: stream is not undefined.
        // Return ! WritableStreamAbort(stream, reason).
        return Stream is null
            ? throw new InvalidOperationException("Writer is released")
            : Stream.AbortInternal(reason);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-writer-ensure-ready-promise-rejected
    /// </summary>
    internal void EnsureReadyPromiseRejected(JsValue error)
    {
        // If writer.[[readyPromise]].[[PromiseState]] is "pending", reject writer.[[readyPromise]] with error.
        if (ReadyPromise is not null && ReadyPromise.Promise.IsPendingPromise())
        {
            ReadyPromise?.Reject(error);
        }
        else
        {
            // Otherwise, set writer.[[readyPromise]] to a promise rejected with error.
            ReadyPromise = PromiseHelper.CreateRejectedPromise(Engine, error);
        }

        // Set writer.[[readyPromise]].[[PromiseIsHandled]] to true.
    }

    private void EnsureClosedPromiseRejected(JsValue error)
    {
        // If writer.[[closedPromise]].[[PromiseState]] is "pending", reject writer.[[closedPromise]] with error.
        if (ClosedPromise is not null && ClosedPromise.Promise.IsPendingPromise())
        {
            ClosedPromise.Reject(error);
        }
        else
        {
            // Otherwise, set writer.[[closedPromise]] to a promise rejected with error.
            ClosedPromise = PromiseHelper.CreateRejectedPromise(Engine, error);
        }

        // Set writer.[[closedPromise]].[[PromiseIsHandled]] to true.
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-writer-release
    /// </summary>
    internal void Release()
    {
        if (Stream == null)
        {
            return;
        }

        var releasedError = ErrorHelper.Create(Engine, "TypeError", "Writer has been released");

        EnsureReadyPromiseRejected(releasedError);
        EnsureClosedPromiseRejected(releasedError);

        Stream.Writer = null;
        Stream = null;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-writer-write
    /// </summary>
    internal JsValue WriteInternal(JsValue chunk)
    {
        if (Stream is null)
        {
            throw new InvalidOperationException();
        }

        var chunkSize = Stream.Controller.StrategySizeAlgorithm.Call(chunk).AsNumber();

        if (!Stream.Locked)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Stream is not locked")
                )
                .Promise;
        }

        var state = Stream.State;
        if (state == WritableStreamState.Errored)
        {
            return PromiseHelper.CreateRejectedPromise(Engine, Stream.StoredError).Promise;
        }

        if (Stream.IsCloseQueuedOrInFlight || state == WritableStreamState.Closed)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Stream is closed or closing")
                )
                .Promise;
        }

        if (state == WritableStreamState.Erroring)
        {
            return PromiseHelper.CreateRejectedPromise(Engine, Stream.StoredError).Promise;
        }

        if (state != WritableStreamState.Writable)
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    Engine,
                    ErrorHelper.Create(Engine, "TypeError", "Stream is not writable")
                )
                .Promise;
        }

        var promise = WritableStreamAddWriteRequest();
        Stream.Controller.Write(chunk, chunkSize);
        return promise;

        // https://streams.spec.whatwg.org/#writable-stream-add-write-request
        JsValue WritableStreamAddWriteRequest()
        {
            var manualPromise = Engine.Advanced.RegisterPromise();
            Stream.WriteRequests.Add(manualPromise);
            return manualPromise.Promise;
        }
    }
}
