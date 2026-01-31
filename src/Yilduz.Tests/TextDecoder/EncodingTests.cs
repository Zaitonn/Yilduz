using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.TextDecoder;

/// <summary>
/// Tests for various encoding support in TextDecoder
/// </summary>
public sealed class EncodingTests : TestBase
{
    [Theory]
    [InlineData("utf-8", new byte[] { 72, 101, 108, 108, 111 }, "Hello")]
    [InlineData("utf-8", new byte[] { 0xC4, 0x85 }, "ą")] // Latin Small Letter A with Ogonek
    [InlineData("utf-8", new byte[] { 0xE4, 0xB8, 0xAD }, "中")] // Chinese character
    [InlineData("utf-8", new byte[] { 0xF0, 0x9F, 0x98, 0x80 }, "😀")] // Emoji
    public void ShouldDecodeUtf8Characters(string encoding, byte[] input, string expected)
    {
        var bytesStr = string.Join(", ", input);
        Execute(
            $"""
            const decoder = new TextDecoder('{encoding}');
            const bytes = new Uint8Array([{bytesStr}]);
            const result = decoder.decode(bytes);
            """
        );

        Assert.Equal(expected, Evaluate("result").AsString());
    }

    [Theory]
    [InlineData(
        "utf-16le",
        new byte[] { 0x48, 0x00, 0x65, 0x00, 0x6C, 0x00, 0x6C, 0x00, 0x6F, 0x00 },
        "Hello"
    )]
    [InlineData(
        "utf-16be",
        new byte[] { 0x00, 0x48, 0x00, 0x65, 0x00, 0x6C, 0x00, 0x6C, 0x00, 0x6F },
        "Hello"
    )]
    public void ShouldDecodeUtf16Characters(string encoding, byte[] input, string expected)
    {
        var bytesStr = string.Join(", ", input);
        Execute(
            $"""
            const decoder = new TextDecoder('{encoding}');
            const bytes = new Uint8Array([{bytesStr}]);
            const result = decoder.decode(bytes);
            """
        );

        Assert.Equal(expected, Evaluate("result").AsString());
    }

    [Theory]
    [InlineData("windows-1252", new byte[] { 72, 101, 108, 108, 111 }, "Hello")]
    [InlineData("windows-1252", new byte[] { 0xC0, 0xC1, 0xC2, 0xC3 }, "ÀÁÂÃ")] // Accented characters
    [InlineData("windows-1251", new byte[] { 0xC0, 0xE1, 0xE8, 0xF1 }, "Абис")] // Cyrillic characters
    public void ShouldDecodeWindowsCodePages(string encoding, byte[] input, string expected)
    {
        var bytesStr = string.Join(", ", input);
        Execute(
            $"""
            const decoder = new TextDecoder('{encoding}');
            const bytes = new Uint8Array([{bytesStr}]);
            const result = decoder.decode(bytes);
            """
        );

        Assert.Equal(expected, Evaluate("result").AsString());
    }

    [Theory]
    [InlineData("iso-8859-1", new byte[] { 72, 101, 108, 108, 111 }, "Hello")]
    [InlineData("iso-8859-1", new byte[] { 0xC0, 0xC1, 0xC2, 0xC3 }, "ÀÁÂÃ")]
    [InlineData("iso-8859-2", new byte[] { 0xC0, 0xC1, 0xC2, 0xC3 }, "ŔÁÂĂ")] // Central European
    [InlineData("iso-8859-7", new byte[] { 0xC0, 0xC1, 0xC2, 0xC3 }, "ΐΑΒΓ")] // Greek
    public void ShouldDecodeIsoEncodings(string encoding, byte[] input, string expected)
    {
        var bytesStr = string.Join(", ", input);
        Execute(
            $"""
            const decoder = new TextDecoder('{encoding}');
            const bytes = new Uint8Array([{bytesStr}]);
            const result = decoder.decode(bytes);
            """
        );

        Assert.Equal(expected, Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldHandleUtf8BomCorrectly()
    {
        Execute(
            """
            const decoder = new TextDecoder('utf-8');
            const bytesWithBOM = new Uint8Array([0xEF, 0xBB, 0xBF, 72, 101, 108, 108, 111]); // BOM + 'Hello'
            const result = decoder.decode(bytesWithBOM);
            """
        );

        Assert.Equal("Hello", Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldHandleUtf16LeBomCorrectly()
    {
        Execute(
            """
            const decoder = new TextDecoder('utf-16le');
            const bytesWithBOM = new Uint8Array([0xFF, 0xFE, 0x48, 0x00, 0x65, 0x00]); // BOM + 'He'
            const result = decoder.decode(bytesWithBOM);
            """
        );

        Assert.Equal("He", Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldHandleUtf16BeBomCorrectly()
    {
        Execute(
            """
            const decoder = new TextDecoder('utf-16be');
            const bytesWithBOM = new Uint8Array([0xFE, 0xFF, 0x00, 0x48, 0x00, 0x65]); // BOM + 'He'
            const result = decoder.decode(bytesWithBOM);
            """
        );

        Assert.Equal("He", Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldRespectIgnoreBomOptionForUtf8()
    {
        Execute(
            """
            const decoder = new TextDecoder('utf-8', { ignoreBOM: true });
            const bytesWithBOM = new Uint8Array([0xEF, 0xBB, 0xBF, 72, 101, 108, 108, 111]); // BOM + 'Hello'
            const result = decoder.decode(bytesWithBOM);
            """
        );

        Assert.Equal("\ufeffHello", Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldRespectIgnoreBomOptionForUtf16Le()
    {
        Execute(
            """
            const decoder = new TextDecoder('utf-16le', { ignoreBOM: true });
            const bytesWithBOM = new Uint8Array([0xFF, 0xFE, 0x48, 0x00, 0x65, 0x00]); // BOM + 'He'
            const result = decoder.decode(bytesWithBOM);
            """
        );

        Assert.Equal("\ufeffHe", Evaluate("result").AsString());
    }

    [Theory]
    [InlineData("ascii")]
    [InlineData("us-ascii")]
    [InlineData("iso-8859-1")]
    public void ShouldHandleAsciiCompatibleEncodings(string encoding)
    {
        Execute(
            $"""
            const decoder = new TextDecoder('{encoding}');
            const bytes = new Uint8Array([72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100]); // 'Hello World'
            const result = decoder.decode(bytes);
            """
        );

        Assert.Equal("Hello World", Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldHandleEmptyBytesForAllEncodings()
    {
        var encodings = new[] { "utf-8", "utf-16le", "utf-16be", "windows-1252", "iso-8859-1" };

        foreach (var encoding in encodings)
        {
            Execute(
                $"""
                var decoder = new TextDecoder('{encoding}');
                var result = decoder.decode(new Uint8Array([]));
                """
            );

            Assert.Equal("", Evaluate("result").AsString());
        }
    }

    [Theory]
    [InlineData("utf-8", new byte[] { 0xFF, 0xFE })] // Invalid UTF-8 sequence
    [InlineData("utf-8", new byte[] { 0x80, 0x80 })] // Invalid UTF-8 continuation
    [InlineData("utf-16le", new byte[] { 0x00 })] // Incomplete UTF-16 sequence
    public void ShouldHandleInvalidSequencesWithFatalFalse(string encoding, byte[] input)
    {
        var bytesStr = string.Join(", ", input);
        Execute(
            $$"""
            const decoder = new TextDecoder('{{encoding}}', { fatal: false });
            const bytes = new Uint8Array([{{bytesStr}}]);
            const result = decoder.decode(bytes);
            """
        );

        // Should not throw and return some result (possibly with replacement characters)
        var result = Evaluate("result").AsString();
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("cp1252")]
    [InlineData("cp1251")]
    [InlineData("cp1250")]
    [InlineData("latin1")]
    [InlineData("latin2")]
    public void ShouldSupportCommonEncodingAliases(string alias)
    {
        Execute(
            $"""
            const decoder = new TextDecoder('{alias}');
            const bytes = new Uint8Array([72, 101, 108, 108, 111]); // 'Hello'
            const result = decoder.decode(bytes);
            """
        );

        Assert.Equal("Hello", Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldHandleMixedContentInUtf8()
    {
        Execute(
            """
            const decoder = new TextDecoder('utf-8');
            // Mix of ASCII, Latin, Cyrillic, and emoji
            const bytes = new Uint8Array([
                72, 101, 108, 108, 111, 32, // 'Hello '
                0xC3, 0xA9, 32, // 'é '
                0xD0, 0x9F, 0xD1, 0x80, 0xD0, 0xB8, 0xD0, 0xB2, 0xD0, 0xB5, 0xD1, 0x82, 32, // 'Привет '
                0xF0, 0x9F, 0x98, 0x80 // '😀'
            ]);
            const result = decoder.decode(bytes);
            """
        );

        Assert.Equal("Hello é Привет 😀", Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldWorkWithLargeInputs()
    {
        Execute(
            """
            const decoder = new TextDecoder('utf-8');
            const largeText = 'A'.repeat(10000);
            const encoder = new TextEncoder();
            const bytes = encoder.encode(largeText);
            const result = decoder.decode(bytes);
            """
        );

        var result = Evaluate("result").AsString();
        Assert.Equal(10000, result.Length);
        Assert.All(result, c => Assert.Equal('A', c));
    }

    [Theory]
    [InlineData("utf-8", new byte[] { 0xEF, 0xBB, 0xBF })] // UTF-8 BOM
    [InlineData("utf-16le", new byte[] { 0xFF, 0xFE })] // UTF-16LE BOM
    [InlineData("utf-16be", new byte[] { 0xFE, 0xFF })] // UTF-16BE BOM
    public void ShouldHandleBomOnlyInput(string encoding, byte[] bom)
    {
        var bytesStr = string.Join(", ", bom);
        Execute(
            $"""
            const decoder = new TextDecoder('{encoding}');
            const bytes = new Uint8Array([{bytesStr}]);
            const result = decoder.decode(bytes);
            """
        );

        Assert.Equal("", Evaluate("result").AsString());
    }
}
