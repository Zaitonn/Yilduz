using Yilduz.Streams.ReadableStream;
using Yilduz.Streams.WritableStream;

namespace Yilduz.Streams.TransformStream;

/// <summary>
/// GenericTransformStream
/// </summary>
public interface IGenericTransformStream
{
    /// <summary>
    /// The ReadableStream instance controlled by this object
    /// <br/>
    /// https://streams.spec.whatwg.org/#ts-readable
    /// </summary>
    ReadableStreamInstance Readable { get; }

    /// <summary>
    /// The WritableStream instance controlled by this object
    /// <br/>
    /// https://streams.spec.whatwg.org/#ts-writable
    /// </summary>
    WritableStreamInstance Writable { get; }
}
