using System;

namespace Yilduz.Compression.Providers;

/// <summary>
/// Abstraction for a stateful compression or decompression operation.
/// Each instance processes one stream of data — create a new instance per stream.
/// </summary>
public interface ICompressionProvider : IDisposable
{
    /// <summary>
    /// Processes a chunk of input data and returns any immediately available output bytes.
    /// May return an empty array if no output is ready yet.
    /// </summary>
    byte[] Transform(ReadOnlySpan<byte> input);

    /// <summary>
    /// Signals the end of input data and flushes any remaining buffered output.
    /// </summary>
    byte[] Flush();
}
