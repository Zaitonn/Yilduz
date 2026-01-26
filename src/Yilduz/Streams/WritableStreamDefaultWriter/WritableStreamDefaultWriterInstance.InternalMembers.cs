using Jint.Native.Promise;
using Yilduz.Streams.WritableStream;

namespace Yilduz.Streams.WritableStreamDefaultWriter;

public sealed partial class WritableStreamDefaultWriterInstance
{
    // Internal slots as defined in the WHATWG Streams specification
    // https://streams.spec.whatwg.org/#ws-default-writer-internal-slots

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
}
