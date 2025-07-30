using System.Collections.Generic;
using Jint.Native;
using Jint.Native.Promise;
using Yilduz.Streams.WritableStreamDefaultController;
using Yilduz.Streams.WritableStreamDefaultWriter;

namespace Yilduz.Streams.WritableStream;

public sealed partial class WritableStreamInstance
{
    /// <summary>
    /// Internal slots as defined in the WHATWG Streams specification
    /// https://streams.spec.whatwg.org/#ws-internal-slots
    /// </summary>

    /// <summary>
    /// [[backpressure]] - A boolean indicating the backpressure signal set by the controller
    /// </summary>
    internal bool Backpressure { get; private set; }

    /// <summary>
    /// [[closeRequest]] - The promise returned from the writer's close() method
    /// </summary>
    internal ManualPromise? CloseRequest { get; set; }

    /// <summary>
    /// [[controller]] - A WritableStreamDefaultController created with the ability to control the state and queue of this stream
    /// </summary>
    internal WritableStreamDefaultControllerInstance Controller { get; private set; }

    /// <summary>
    /// [[Detached]] - A boolean flag set to true when the stream is transferred
    /// </summary>
    internal bool Detached { get; set; }

    /// <summary>
    /// [[inFlightWriteRequest]] - A slot set to the promise for the current in-flight write operation
    /// </summary>
    internal ManualPromise? InFlightWriteRequest { get; set; }

    /// <summary>
    /// [[inFlightCloseRequest]] - A slot set to the promise for the current in-flight close operation
    /// </summary>
    internal ManualPromise? InFlightCloseRequest { get; set; }

    /// <summary>
    /// [[pendingAbortRequest]] - A pending abort request
    /// </summary>
    internal PendingAbortRequest? PendingAbortRequest { get; set; }

    /// <summary>
    /// [[state]] - A string containing the stream's current state
    /// </summary>
    internal WritableStreamState State { get; private set; }

    /// <summary>
    /// [[storedError]] - A value indicating how the stream failed
    /// </summary>
    internal JsValue StoredError { get; private set; } = Undefined;

    /// <summary>
    /// [[writer]] - A WritableStreamDefaultWriter instance, if the stream is locked to a writer
    /// </summary>
    internal WritableStreamDefaultWriterInstance? Writer { get; set; }

    /// <summary>
    /// [[writeRequests]] - A list of promises representing the stream's internal queue of write requests
    /// </summary>
    internal List<ManualPromise> WriteRequests { get; private set; } = [];
}
