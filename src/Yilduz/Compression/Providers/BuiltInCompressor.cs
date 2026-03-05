using System;
using System.IO;

namespace Yilduz.Compression.Providers;

/// <summary>
/// Default implementation of <see cref="ICompressionProvider"/> for compression,
/// backed by a <see cref="Stream"/>-based compressor.
/// </summary>
internal sealed class BuiltInCompressor : ICompressionProvider
{
    private readonly MemoryStream _output = new();
    private readonly Stream _stream;
    private long _readOffset;
    private bool _flushed;

    internal BuiltInCompressor(Func<Stream, Stream> streamFactory)
    {
        _stream = streamFactory(_output);
    }

    public byte[] Transform(ReadOnlySpan<byte> input)
    {
#if NETSTANDARD2_0
        var bytes = input.ToArray();
        _stream.Write(bytes, 0, bytes.Length);
#else
        _stream.Write(input);
#endif
        _stream.Flush();
        return ReadPending();
    }

    public byte[] Flush()
    {
        if (_flushed)
        {
            return [];
        }

        _flushed = true;
        _stream.Dispose();
        return ReadPending();
    }

    private byte[] ReadPending()
    {
        var available = (int)(_output.Length - _readOffset);
        if (available <= 0)
        {
            return [];
        }

        _output.TryGetBuffer(out var segment);
        var result = segment.AsSpan((int)_readOffset, available).ToArray();
        _readOffset += available;
        return result;
    }

    public void Dispose()
    {
        if (!_flushed)
        {
            _stream.Dispose();
        }

        _output.Dispose();
    }
}
