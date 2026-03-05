using System;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;
using Xunit;
using Yilduz.Compression.Providers;
using DeflateStream = Ionic.Zlib.DeflateStream;
using GZipStream = SharpCompress.Compressors.Deflate.GZipStream;

namespace Yilduz.Tests.Compression;

public class CustomProviderTests : TestBase
{
    protected override Options GetOptions()
    {
        return new Options
        {
            CancellationToken = Token,
            Compression = new()
            {
                CompressorFactory = format =>
                    format.ToLowerInvariant() switch
                    {
                        "gzip" => new StreamCompressor(s => new GZipOutputStream(s)),
                        "deflate" => new StreamCompressor(s => new DeflateStream(
                            s,
                            Ionic.Zlib.CompressionMode.Compress
                        )),
                        "bzip2" => new StreamCompressor(s => new BZip2OutputStream(s)),
                        "gzip-sc" => new StreamCompressor(s => new GZipStream(
                            s,
                            SharpCompress.Compressors.CompressionMode.Compress
                        )),
                        _ => throw new NotSupportedException(),
                    },
                DecompressorFactory = format =>
                    format.ToLowerInvariant() switch
                    {
                        "gzip" => new StreamDecompressor(s => new GZipInputStream(s)),
                        "deflate" => new StreamDecompressor(s => new DeflateStream(
                            s,
                            Ionic.Zlib.CompressionMode.Decompress
                        )),
                        "bzip2" => new StreamDecompressor(s => new BZip2InputStream(s)),
                        "gzip-sc" => new StreamDecompressor(s => new GZipStream(
                            s,
                            SharpCompress.Compressors.CompressionMode.Decompress
                        )),
                        _ => throw new NotSupportedException(),
                    },
            },
        };
    }

    [Theory]
    [InlineData("gzip")]
    [InlineData("deflate")]
    [InlineData("bzip2")]
    [InlineData("gzip-sc")]
    public void ShouldRoundTripWithCustomProviders(string format)
    {
        CompressionTestHelper.AssertRoundtripAllFormats(Engine, format);
    }

    [Theory]
    [MemberData(
        nameof(CompressionTestHelper.GzipTestData),
        MemberType = typeof(CompressionTestHelper)
    )]
    public void GzipDecompressionShouldRestoreOriginalInput(string expected, string compressedHex)
    {
        CompressionTestHelper.AssertDecompression(Engine, "gzip", expected, compressedHex);
        CompressionTestHelper.AssertDecompression(Engine, "gzip-sc", expected, compressedHex);
    }

    [Theory]
    [MemberData(
        nameof(CompressionTestHelper.DeflateTestData),
        MemberType = typeof(CompressionTestHelper)
    )]
    public void DeflateDecompressionShouldRestoreOriginalInput(
        string expected,
        string compressedHex
    )
    {
        CompressionTestHelper.AssertDecompression(Engine, "deflate", expected, compressedHex);
    }

    private class NonClosingMemoryStream : MemoryStream
    {
        public override void Close() { }

        protected override void Dispose(bool disposing) { }
    }

    private class StreamCompressor : ICompressionProvider
    {
        private readonly NonClosingMemoryStream _output = new();
        private readonly Stream _stream;
        private long _readOffset;
        private bool _flushed;

        public StreamCompressor(Func<Stream, Stream> streamFactory)
        {
            _stream = streamFactory(_output);
        }

        public byte[] Transform(ReadOnlySpan<byte> input)
        {
            var arr = input.ToArray();
            _stream.Write(arr, 0, arr.Length);
            try
            {
                _stream.Flush();
            }
            catch (NotSupportedException) { }
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
        }
    }

    private class StreamDecompressor : ICompressionProvider
    {
        private readonly MemoryStream _inputBuffer = new();
        private readonly Func<Stream, Stream> _streamFactory;
        private long _decompressedOffset;

        public StreamDecompressor(Func<Stream, Stream> streamFactory)
        {
            _streamFactory = streamFactory;
        }

        public byte[] Transform(ReadOnlySpan<byte> input)
        {
            var arr = input.ToArray();
            _inputBuffer.Position = _inputBuffer.Length;
            _inputBuffer.Write(arr, 0, arr.Length);
            return Decompress();
        }

        public byte[] Flush() => Decompress();

        private byte[] Decompress()
        {
            try
            {
                var inputData = _inputBuffer.ToArray();
                using var inputStream = new MemoryStream(inputData);
                using var decompressor = _streamFactory(inputStream);
                using var outputBuffer = new MemoryStream();
                decompressor.CopyTo(outputBuffer);

                var all = outputBuffer.ToArray();
                var newLength = (int)(all.Length - _decompressedOffset);
                if (newLength <= 0)
                {
                    return [];
                }

                var result = new byte[newLength];
                Array.Copy(all, (int)_decompressedOffset, result, 0, newLength);
                _decompressedOffset = all.Length;
                return result;
            }
            catch
            {
                // Ignore incomplete stream errors
                return [];
            }
        }

        public void Dispose() => _inputBuffer.Dispose();
    }
}
