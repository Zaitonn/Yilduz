using System.Linq;
using Jint;
using Jint.Runtime;
using Xunit;
using Yilduz.Extensions;

namespace Yilduz.Tests.Compression;

public sealed class RuntimeTests : TestBase
{
    [Theory]
    [InlineData("gzip")]
    [InlineData("deflate-raw")]
    [InlineData("deflate")]
    public void ShouldRoundtripAllFormats(string format)
    {
        CompressionTestHelper.AssertRoundtripAllFormats(Engine, format);
    }

    [Theory]
    [InlineData("gzip")]
    [InlineData("deflate-raw")]
    [InlineData("deflate")]
    public void CompressedOutputShouldBeSmallerForRepetitiveInput(string format)
    {
        var repetitive = new string('B', 50);

        Execute(
            $$"""
            const encoder = new TextEncoder();
            const compressor = new CompressionStream('{{format}}');
            const writer = compressor.writable.getWriter();
            const reader = compressor.readable.getReader();
            const chunks = [];

            async function run() {
                const readAll = (async () => {
                    while (true) {
                        const { value, done } = await reader.read();
                        if (done) break;
                        chunks.push(value);
                    }
                })();
                await writer.write(encoder.encode('{{repetitive}}'));
                await writer.close();
                await readAll;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        var compressedLength = Evaluate("chunks").AsArray().Sum(c => c.TryAsBytes()!.Length);

        Assert.True(compressedLength < repetitive.Length);
    }

    [Theory]
    [MemberData(
        nameof(CompressionTestHelper.GzipTestData),
        MemberType = typeof(CompressionTestHelper)
    )]
    public void GzipDecompressionShouldRestoreOriginalInput(string expected, string compressedHex)
    {
        CompressionTestHelper.AssertDecompression(Engine, "gzip", expected, compressedHex);
    }

    [Theory]
    [MemberData(
        nameof(CompressionTestHelper.DeflateRawTestData),
        MemberType = typeof(CompressionTestHelper)
    )]
    public void DeflateRawDecompressionShouldRestoreOriginalInput(
        string expected,
        string compressedHex
    )
    {
        CompressionTestHelper.AssertDecompression(Engine, "deflate-raw", expected, compressedHex);
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

    [Fact]
    public void ShouldRejectNonBufferOnCompression()
    {
        Execute(
            """
            const compressor = new CompressionStream('gzip');
            const writer = compressor.writable.getWriter();
            const reader = compressor.readable.getReader();
            """
        );

        Assert.Throws<PromiseRejectedException>(
            () =>
                Engine
                    .Evaluate(
                        """
                        const readPromise = reader.read();
                        writer.write('not-bytes');
                        """
                    )
                    .UnwrapIfPromise()
        );
    }

    [Fact]
    public void ShouldRejectNonBufferOnDecompression()
    {
        Execute(
            """
            const decompressor = new DecompressionStream('gzip');
            const writer = decompressor.writable.getWriter();
            const reader = decompressor.readable.getReader();
            """
        );

        Assert.Throws<PromiseRejectedException>(
            () =>
                Engine
                    .Evaluate(
                        """
                        const readPromise = reader.read();
                        writer.write('not-bytes');
                        """
                    )
                    .UnwrapIfPromise()
        );
    }
}
