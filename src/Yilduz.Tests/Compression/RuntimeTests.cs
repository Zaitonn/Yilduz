using System;
using System.Linq;
using Jint;
using Jint.Runtime;
using Xunit;
using Yilduz.Extensions;

namespace Yilduz.Tests.Compression;

public sealed class RuntimeTests : TestBase
{
    private readonly string _testInput = new('A', 100);

    [Theory]
    [InlineData("gzip")]
    [InlineData("deflate-raw")]
    [InlineData("deflate")]
    public void ShouldRoundtripAllFormats(string format)
    {
        Execute(
            $$"""
            const encoder = new TextEncoder();
            const decoder = new TextDecoder();
            const compressor = new CompressionStream('{{format}}');
            const decompressor = new DecompressionStream('{{format}}');
            const writer = compressor.writable.getWriter();
            const reader = compressor.readable.getReader();

            async function run() {
                const compressedChunks = [];

                const readCompressed = (async () => {
                    while (true) {
                        const { value, done } = await reader.read();
                        if (done) break;
                        compressedChunks.push(value);
                    }
                })();

                await writer.write(encoder.encode('{{_testInput}}'));
                await writer.close();
                await readCompressed;

                const compressedLength = compressedChunks.reduce((sum, chunk) => sum + chunk.byteLength, 0);
                const compressedMerged = new Uint8Array(compressedLength);
                let compressedOffset = 0;
                for (const chunk of compressedChunks) {
                    compressedMerged.set(chunk, compressedOffset);
                    compressedOffset += chunk.byteLength;
                }

                const writer2 = decompressor.writable.getWriter();
                const reader2 = decompressor.readable.getReader();
                const decompressedChunks = [];

                const readDecompressed = (async () => {
                    while (true) {
                        const { value, done } = await reader2.read();
                        if (done) break;
                        decompressedChunks.push(value);
                    }
                })();

                await writer2.write(compressedMerged);
                await writer2.close();
                await readDecompressed;

                const totalLength = decompressedChunks.reduce((sum, chunk) => sum + chunk.byteLength, 0);
                const merged = new Uint8Array(totalLength);
                let offset = 0;
                for (const chunk of decompressedChunks) {
                    merged.set(chunk, offset);
                    offset += chunk.byteLength;
                }

                return decoder.decode(merged);
            }
            """
        );

        var result = Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(_testInput, result.AsString());
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

    private void AssertDecompression(string format, string expected, string compressedHex)
    {
        var str = "";
        for (var i = 0; i < compressedHex.Length; i += 2)
        {
            str += string.Concat("0x", compressedHex.AsSpan(i, 2), ",");
        }

        Execute(
            $$"""
            const inputBytes = Uint8Array.from([{{str}}]);
            const decoder = new TextDecoder();
            const decompressor = new DecompressionStream('{{format}}');
            const writer2 = decompressor.writable.getWriter();
            const reader2 = decompressor.readable.getReader();

            async function run2() {
                const chunks = [];

                const readAll = (async () => {
                    while (true) {
                        const { value, done } = await reader2.read();
                        if (done) break;
                        chunks.push(value);
                    }
                })();

                await writer2.write(inputBytes);
                await writer2.close();
                await readAll;

                const totalLength = chunks.reduce((sum, chunk) => sum + chunk.byteLength, 0);
                const merged = new Uint8Array(totalLength);
                let offset = 0;
                for (const chunk of chunks) {
                    merged.set(chunk, offset);
                    offset += chunk.byteLength;
                }

                return decoder.decode(merged);
            }
            """
        );

        var result = Evaluate("run2()").UnwrapIfPromise();

        Assert.Equal(expected, result.AsString());
    }

    [Theory]
    [InlineData("testtesttesttest", "1F8B080000000000000A2B492D2E2941C200C487A34510000000")]
    [InlineData(
        "A fox jumps over the lazy dog",
        "1F8B080000000000000A735448CBAF50C82ACD2D2856C82F4B2D5228C94855C849ACAA5448C94F0700537CBAA71D000000"
    )]
    [InlineData(
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
        "1F8B08000000000000037374A43D00008DBC979564000000"
    )]
    public void GzipDecompressionShouldRestoreOriginalInput(string expected, string compressedHex)
    {
        AssertDecompression("gzip", expected, compressedHex);
    }

    [Theory]
    [InlineData("testtesttesttest", "2B492D2E2941C200")]
    [InlineData(
        "A fox jumps over the lazy dog",
        "735448CBAF50C82ACD2D2856C82F4B2D5228C94855C849ACAA5448C94F0700"
    )]
    [InlineData(
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
        "7374a43d0000"
    )]
    public void DeflateRawDecompressionShouldRestoreOriginalInput(
        string expected,
        string compressedHex
    )
    {
        AssertDecompression("deflate-raw", expected, compressedHex);
    }

    [Theory]
    [InlineData("testtesttesttest", "789C2B492D2E2941C2003B740701")]
    [InlineData(
        "A fox jumps over the lazy dog",
        "789C735448CBAF50C82ACD2D2856C82F4B2D5228C94855C849ACAA5448C94F070099790A75"
    )]
    [InlineData(
        "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA",
        "789c7374a43d000002e91965"
    )]
    public void DeflateDecompressionShouldRestoreOriginalInput(
        string expected,
        string compressedHex
    )
    {
        AssertDecompression("deflate", expected, compressedHex);
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
