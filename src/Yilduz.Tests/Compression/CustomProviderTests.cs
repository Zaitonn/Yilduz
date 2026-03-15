using System;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;
using ICSharpCode.SharpZipLib.GZip;
using Xunit;
using Yilduz.Compression.Providers;
using ZstdSharp;
using DeflateStream = Ionic.Zlib.DeflateStream;
using GZipStream = SharpCompress.Compressors.Deflate.GZipStream;
#if NETCOREAPP
using System.IO.Compression;
#endif

namespace Yilduz.Tests.Compression;

public class CustomProviderTests : TestBase
{
    protected override Options GetOptions()
    {
        return new Options
        {
            CancellationToken = Token,
            Compression =
            {
                CompressorFactory = CompressorFactory,
                DecompressorFactory = DecompressorFactory,
            },
        };

        static ICompressionProvider CompressorFactory(string format) =>
            format.ToLowerInvariant() switch
            {
                // ICSharpCode.SharpZipLib.GZip.GZipOutputStream
                "gzip" => new StreamCompressor(s => new GZipOutputStream(s)),

                // Ionic.Zlib.DeflateStream
                "deflate" => new StreamCompressor(s => new DeflateStream(
                    s,
                    Ionic.Zlib.CompressionMode.Compress
                )),

                // ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream
                "bzip2" => new StreamCompressor(s => new BZip2OutputStream(s)),

                // SharpCompress.Compressors.Deflate.GZipStream
                "gzip-sc" => new StreamCompressor(s => new GZipStream(
                    s,
                    SharpCompress.Compressors.CompressionMode.Compress
                )),

#if NETCOREAPP
                // System.IO.Compression.BrotliStream
                "brotli" => new StreamCompressor(
                    (s) => new BrotliStream(s, CompressionMode.Compress)
                ),

                // System.IO.Compression.BrotliStream with built-in support (no wrapping)
                "brotli-built-in" => new BuiltInCompressor(
                    (s) => new BrotliStream(s, CompressionMode.Compress, true)
                ),
#endif
                // ZstdSharp.CompressionStream
                "ztsd" => new StreamCompressor((s) => new CompressionStream(s)),
                _ => throw new NotSupportedException(),
            };

        static ICompressionProvider DecompressorFactory(string format) =>
            format.ToLowerInvariant() switch
            {
                // ICSharpCode.SharpZipLib.GZip.GZipInputStream
                "gzip" => new StreamDecompressor(s => new GZipInputStream(s)),

                // Ionic.Zlib.DeflateStream
                "deflate" => new StreamDecompressor(s => new DeflateStream(
                    s,
                    Ionic.Zlib.CompressionMode.Decompress
                )),

                // ICSharpCode.SharpZipLib.BZip2.BZip2InputStream
                "bzip2" => new StreamDecompressor(s => new BZip2InputStream(s)),

                // SharpCompress.Compressors.Deflate.GZipStream
                "gzip-sc" => new StreamDecompressor(s => new GZipStream(
                    s,
                    SharpCompress.Compressors.CompressionMode.Decompress
                )),

#if NETCOREAPP
                // System.IO.Compression.BrotliStream
                "brotli" => new StreamDecompressor(
                    (s) => new BrotliStream(s, CompressionMode.Decompress)
                ),

                // System.IO.Compression.BrotliStream with built-in support (no wrapping)
                "brotli-built-in" => new BuiltInDecompressor(
                    (s) => new BrotliStream(s, CompressionMode.Decompress, true)
                ),
#endif

                // ZstdSharp.DecompressionStream
                "ztsd" => new StreamDecompressor((s) => new DecompressionStream(s)),
                _ => throw new NotSupportedException(),
            };
    }

    [Theory]
    [InlineData("gzip")]
    [InlineData("deflate")]
    [InlineData("bzip2")]
    [InlineData("gzip-sc")]
#if NETCOREAPP
    [InlineData("brotli")]
    [InlineData("brotli-built-in")]
#else
    [InlineData(
        "brotli",
        Skip = "Brotli format is not supported in .NET Standard or .NET Framework"
    )]
    [InlineData(
        "brotli-built-in",
        Skip = "Brotli format is not supported in .NET Standard or .NET Framework"
    )]
#endif
    [InlineData("ztsd")]
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

    private class StreamDecompressor(Func<Stream, Stream> streamFactory) : ICompressionProvider
    {
        private readonly MemoryStream _inputBuffer = new();
        private readonly Func<Stream, Stream> _streamFactory = streamFactory;
        private long _decompressedOffset;

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
