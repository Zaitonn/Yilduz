using System;
using Jint;
using Xunit;

namespace Yilduz.Tests.Compression;

internal static class CompressionTestHelper
{
    private const string Test4 = "testtesttesttest";
    private const string LazyDog = "A fox jumps over the lazy dog";
    private static readonly string A100 = new('A', 100);

    public static readonly TheoryData<string, string> GzipTestData = new()
    {
        { A100, "1F8B08000000000000037374A43D00008DBC979564000000" },
        { Test4, "1F8B080000000000000A2B492D2E2941C200C487A34510000000" },
        {
            LazyDog,
            "1F8B080000000000000A735448CBAF50C82ACD2D2856C82F4B2D5228C94855C849ACAA5448C94F0700537CBAA71D000000"
        },
    };

    public static readonly TheoryData<string, string> DeflateRawTestData = new()
    {
        { A100, "7374a43d0000" },
        { Test4, "2B492D2E2941C200" },
        { LazyDog, "735448CBAF50C82ACD2D2856C82F4B2D5228C94855C849ACAA5448C94F0700" },
    };

    public static readonly TheoryData<string, string> DeflateTestData = new()
    {
        { A100, "789c7374a43d000002e91965" },
        { Test4, "789C2B492D2E2941C2003B740701" },
        { LazyDog, "789C735448CBAF50C82ACD2D2856C82F4B2D5228C94855C849ACAA5448C94F070099790A75" },
    };

    public static void AssertRoundtripAllFormats(Engine engine, string format)
    {
        engine.Execute(
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

                await writer.write(encoder.encode('{{A100}}'));
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

        var result = engine.Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(A100, result.AsString());
    }

    public static void AssertDecompression(
        Engine engine,
        string format,
        string expected,
        string compressedHex
    )
    {
        var str = "";
        for (var i = 0; i < compressedHex.Length; i += 2)
        {
            str += string.Concat("0x", compressedHex.AsSpan(i, 2), ",");
        }

        engine.Execute(
            $$"""
            async function run() {
                const inputBytes = Uint8Array.from([{{str}}]);
                const decoder = new TextDecoder();
                const decompressor = new DecompressionStream('{{format}}');
                const writer = decompressor.writable.getWriter();
                const reader = decompressor.readable.getReader();
                const chunks = [];

                const readAll = (async () => {
                    while (true) {
                        const { value, done } = await reader.read();
                        if (done) break;
                        chunks.push(value);
                    }
                })();

                await writer.write(inputBytes);
                await writer.close();
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

        var result = engine.Evaluate("run()").UnwrapIfPromise();

        Assert.Equal(expected, result.AsString());
    }
}
