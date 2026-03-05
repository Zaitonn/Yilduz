using System;
using System.IO.Compression;
using Yilduz.Compression.Providers;

namespace Yilduz;

public sealed partial class Options
{
    public CompressionOptions Compression { get; init; } = new();

    public sealed class CompressionOptions
    {
        /// <summary>
        /// Factory for creating <see cref="ICompressionProvider"/> instances for compression by format name.
        /// The default implementation supports "gzip", "deflate-raw" and "deflate" (on .NET 6+) using
        /// the built-in <c>System.IO.Compression</c> library.
        /// <br/>
        /// Throw <see cref="NotSupportedException"/> for unsupported formats; any other exception will be
        /// surfaced as a JavaScript <c>TypeError</c>.
        /// </summary>
        public Func<string, ICompressionProvider> CompressorFactory { get; init; } =
            DefaultCompressorFactory;

        /// <summary>
        /// Factory for creating <see cref="ICompressionProvider"/> instances for decompression by format name.
        /// The default implementation supports "gzip", "deflate-raw" and "deflate" (on .NET 6+) using
        /// the built-in <c>System.IO.Compression</c> library.
        /// <br/>
        /// Throw <see cref="NotSupportedException"/> for unsupported formats; any other exception will be
        /// surfaced as a JavaScript <c>TypeError</c>.
        /// </summary>
        public Func<string, ICompressionProvider> DecompressorFactory { get; init; } =
            DefaultDecompressorFactory;

        private static ICompressionProvider DefaultCompressorFactory(string format) =>
            format.ToLowerInvariant() switch
            {
                "gzip" => new BuiltInCompressor(output => new GZipStream(
                    output,
                    CompressionMode.Compress,
                    leaveOpen: true
                )),
                "deflate-raw" => new BuiltInCompressor(output => new DeflateStream(
                    output,
                    CompressionMode.Compress,
                    leaveOpen: true
                )),
#if NET6_0_OR_GREATER
                "deflate" => new BuiltInCompressor(output => new ZLibStream(
                    output,
                    CompressionMode.Compress,
                    leaveOpen: true
                )),
#endif
                _ => throw new NotSupportedException(),
            };

        private static ICompressionProvider DefaultDecompressorFactory(string format) =>
            format.ToLowerInvariant() switch
            {
                "gzip" => new BuiltInDecompressor(input => new GZipStream(
                    input,
                    CompressionMode.Decompress
                )),
                "deflate-raw" => new BuiltInDecompressor(input => new DeflateStream(
                    input,
                    CompressionMode.Decompress
                )),
#if NET6_0_OR_GREATER
                "deflate" => new BuiltInDecompressor(input => new ZLibStream(
                    input,
                    CompressionMode.Decompress
                )),
#endif
                _ => throw new NotSupportedException(),
            };
    }
}
