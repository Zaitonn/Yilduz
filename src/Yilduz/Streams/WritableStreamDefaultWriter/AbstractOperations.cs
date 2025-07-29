using Jint;
using Jint.Native;
using Yilduz.Streams.WritableStream;
using Yilduz.Utils;

namespace Yilduz.Streams.WritableStreamDefaultWriter;

/// <summary>
/// Abstract operations for WritableStreamDefaultWriter as defined in WHATWG Streams Standard
/// https://streams.spec.whatwg.org/#ws-default-writer-abstract-ops
/// </summary>
internal static class AbstractOperations
{
    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-writer-abort
    /// </summary>
    public static JsValue WritableStreamDefaultWriterAbort(
        WritableStreamDefaultWriterInstance writer,
        JsValue reason
    )
    {
        var stream = writer.Stream!;
        return WritableStream.AbstractOperations.WritableStreamAbort(stream, reason);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-writer-close
    /// </summary>
    public static JsValue WritableStreamDefaultWriterClose(
        WritableStreamDefaultWriterInstance writer
    )
    {
        var stream = writer.Stream!;
        return WritableStream.AbstractOperations.WritableStreamClose(stream);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-writer-ensure-closed-promise-rejected
    /// </summary>
    public static void WritableStreamDefaultWriterEnsureClosedPromiseRejected(
        WritableStreamDefaultWriterInstance writer,
        JsValue error
    )
    {
        if (writer.ClosedPromise is not null)
        {
            writer.ClosedPromise = PromiseHelper.CreateRejectedPromise(writer.Engine, error);
        }
        else
        {
            // If the promise is already settled, create a new rejected promise
            writer.ClosedPromise = PromiseHelper.CreateRejectedPromise(writer.Engine, error);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-writer-ensure-ready-promise-rejected
    /// </summary>
    public static void WritableStreamDefaultWriterEnsureReadyPromiseRejected(
        WritableStreamDefaultWriterInstance writer,
        JsValue error
    )
    {
        if (writer.ReadyPromise is not null)
        {
            writer.ReadyPromise = PromiseHelper.CreateRejectedPromise(writer.Engine, error);
        }
        else
        {
            // If the promise is already settled, create a new rejected promise
            writer.ReadyPromise = PromiseHelper.CreateRejectedPromise(writer.Engine, error);
        }
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-writer-get-desired-size
    /// </summary>
    public static double? WritableStreamDefaultWriterGetDesiredSize(
        WritableStreamDefaultWriterInstance writer
    )
    {
        var stream = writer.Stream;
        if (stream == null)
        {
            return null;
        }

        var state = stream.State;
        if (state == WritableStreamState.Errored || state == WritableStreamState.Erroring)
        {
            return null;
        }

        if (state == WritableStreamState.Closed)
        {
            return 0;
        }

        return WritableStreamDefaultController.AbstractOperations.WritableStreamDefaultControllerGetDesiredSize(
            stream.Controller!
        );
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-writer-release
    /// </summary>
    public static void WritableStreamDefaultWriterRelease(
        WritableStreamDefaultWriterInstance writer
    )
    {
        var stream = writer.Stream;
        if (stream == null)
        {
            return;
        }

        var releasedError = ErrorHelper.Create(
            writer.Engine,
            "TypeError",
            "Writer has been released"
        );

        WritableStreamDefaultWriterEnsureReadyPromiseRejected(writer, releasedError);
        WritableStreamDefaultWriterEnsureClosedPromiseRejected(writer, releasedError);

        stream.Writer = null;
        writer.Stream = null;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-default-writer-write
    /// </summary>
    public static JsValue WritableStreamDefaultWriterWrite(
        WritableStreamDefaultWriterInstance writer,
        JsValue chunk
    )
    {
        var stream = writer.Stream!;
        var controller = stream.Controller!;

        var chunkSize = controller.StrategySizeAlgorithm(chunk);

        if (!WritableStream.AbstractOperations.IsWritableStreamLocked(stream))
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    writer.Engine,
                    ErrorHelper.Create(writer.Engine, "TypeError", "Stream is not locked")
                )
                .Promise;
        }

        var state = stream.State;
        if (state == WritableStreamState.Errored)
        {
            return PromiseHelper.CreateRejectedPromise(writer.Engine, stream.StoredError).Promise;
        }

        if (
            WritableStream.AbstractOperations.WritableStreamCloseQueuedOrInFlight(stream)
            || state == WritableStreamState.Closed
        )
        {
            return PromiseHelper
                .CreateRejectedPromise(
                    writer.Engine,
                    ErrorHelper.Create(writer.Engine, "TypeError", "Stream is closed or closing")
                )
                .Promise;
        }

        if (state == WritableStreamState.Erroring)
        {
            return PromiseHelper.CreateRejectedPromise(writer.Engine, stream.StoredError).Promise;
        }

        var promise = WritableStreamAddWriteRequest(stream);
        WritableStreamDefaultController.AbstractOperations.WritableStreamDefaultControllerWrite(
            controller,
            chunk,
            chunkSize
        );
        return promise;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#writable-stream-add-write-request
    /// </summary>
    private static JsValue WritableStreamAddWriteRequest(WritableStreamInstance stream)
    {
        var manualPromise = stream.Engine.Advanced.RegisterPromise();
        stream.WriteRequests.Add(manualPromise);
        return manualPromise.Promise;
    }
}
