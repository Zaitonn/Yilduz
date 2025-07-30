using System;
using Jint;
using Jint.Native;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStreamDefaultWriter;

public sealed partial class WritableStreamDefaultWriterInstance
{
    internal void EnsureReadyPromiseRejected(JsValue error)
    {
        if (ReadyPromise is not null)
        {
            ReadyPromise = PromiseHelper.CreateRejectedPromise(Engine, error);
        }
        else
        {
            // If the promise is already settled, create a new rejected promise
            ReadyPromise = PromiseHelper.CreateRejectedPromise(Engine, error);
        }
    }

    private void EnsureClosedPromiseRejected(JsValue error)
    {
        if (ClosedPromise is not null)
        {
            ClosedPromise = PromiseHelper.CreateRejectedPromise(Engine, error);
        }
        else
        {
            // If the promise is already settled, create a new rejected promise
            ClosedPromise = PromiseHelper.CreateRejectedPromise(Engine, error);
        }
    }

    private void Release()
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
    private JsValue WriteInternal(JsValue chunk)
    {
        if (Stream is null)
        {
            throw new InvalidOperationException();
        }

        var chunkSize = Stream.Controller.StrategySizeAlgorithm?.Call(chunk).AsNumber() ?? 1;

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

        if (Stream.IsCloseQueuedOrInFlight() || state == WritableStreamState.Closed)
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
