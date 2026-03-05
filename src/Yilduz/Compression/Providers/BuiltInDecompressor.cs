using System;
using System.IO;

namespace Yilduz.Compression.Providers;

/// <summary>
/// Default implementation of <see cref="ICompressionProvider"/> for decompression,
/// backed by a <see cref="Stream"/>-based decompressor.
/// </summary>
internal sealed class BuiltInDecompressor : ICompressionProvider
{
    private readonly MemoryStream _inputBuffer = new();
    private long _decompressedOffset;
    private readonly Func<Stream, Stream> _streamFactory;

    internal BuiltInDecompressor(Func<Stream, Stream> streamFactory)
    {
        _streamFactory = streamFactory;
    }

    public byte[] Transform(ReadOnlySpan<byte> input)
    {
        _inputBuffer.Position = _inputBuffer.Length;
#if NETSTANDARD2_0
        var bytes = input.ToArray();
        _inputBuffer.Write(bytes, 0, bytes.Length);
#else
        _inputBuffer.Write(input);
#endif
        return Decompress(isFlush: false);
    }

    public byte[] Flush() => Decompress(isFlush: true);

    private byte[] Decompress(bool isFlush)
    {
        var inputData = _inputBuffer.ToArray();

        using var inputStream = new MemoryStream(inputData);
        using var decompressor = _streamFactory(inputStream);
        using var outputBuffer = new MemoryStream();

        decompressor.CopyTo(outputBuffer);

        var consumed = inputStream.Position;
        var total = inputStream.Length;

        var all = outputBuffer.ToArray();
        var newLength = (int)(all.Length - _decompressedOffset);

        byte[] result;
        if (newLength > 0)
        {
            result = new byte[newLength];
            Array.Copy(all, (int)_decompressedOffset, result, 0, newLength);
            _decompressedOffset = all.Length;
        }
        else
        {
            result = [];
        }

        if (consumed < total)
        {
            throw new InvalidDataException("Compressed input contains trailing data.");
        }

        if (isFlush && consumed != total)
        {
            throw new InvalidDataException("Compressed input incomplete at end of stream.");
        }

        return result;
    }

    public void Dispose() => _inputBuffer.Dispose();
}
