using System;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStreamDefaultWriter;

public sealed partial class WritableStreamDefaultWriterInstance
{
    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-writer-abort
    /// </summary>
    internal JsValue AbortInternal(JsValue reason)
    {
        if (Stream is null)
        {
            throw new InvalidOperationException();
        }

        return Stream.AbortInternal(reason);
    }

    internal void EnsureReadyPromiseRejected(JsValue error)
    {
        ReadyPromise?.Reject(error);
        ReadyPromise = PromiseHelper.CreateRejectedPromise(Engine, error);
    }

    private void EnsureClosedPromiseRejected(JsValue error)
    {
        ClosedPromise?.Reject(error);
        ClosedPromise = PromiseHelper.CreateRejectedPromise(Engine, error);
    }

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
