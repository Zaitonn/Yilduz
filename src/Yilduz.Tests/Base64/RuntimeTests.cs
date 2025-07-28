using System;
using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Base64;

public sealed class RuntimeTests : TestBase
{
    [Theory]
    [InlineData("", "")]
    [InlineData("hello", "aGVsbG8=")]
    [InlineData("Yilduz", "WWlsZHV6")]
    [InlineData("123456", "MTIzNDU2")]
    [InlineData("!@#$%^&*", "IUAjJCVeJio=")]
    [InlineData(" ", "IA==")]
    [InlineData(
        "The quick brown fox jumps over the lazy dog",
        "VGhlIHF1aWNrIGJyb3duIGZveCBqdW1wcyBvdmVyIHRoZSBsYXp5IGRvZw=="
    )]
    [InlineData("a", "YQ==")]
    [InlineData("ab", "YWI=")]
    [InlineData("abc", "YWJj")]
    [InlineData("abcd", "YWJjZA==")]
    [InlineData("abcde", "YWJjZGU=")]
    [InlineData("abcdef", "YWJjZGVm")]
    [InlineData(
        "This is a test string with special chars: ~!@#$%^&*()_+",
        "VGhpcyBpcyBhIHRlc3Qgc3RyaW5nIHdpdGggc3BlY2lhbCBjaGFyczogfiFAIyQlXiYqKClfKw=="
    )]
    public void ShouldEncodeAndDecodeBetweenStringAndBase64(string text, string base64)
    {
        Assert.Equal(text, Engine.Evaluate($"atob('{base64}')"));
        Assert.Equal(base64, Engine.Evaluate($"btoa('{text}')"));
    }

    [Theory]
    [InlineData("你好")]
    [InlineData("😀")]
    [InlineData("■")]
    public void ShouldThrowForAnyCharaterIsInvalid(string text)
    {
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate($"btoa('{text}')"));
    }

    [Theory]
    [InlineData("1")]
    [InlineData("YQ=")]
    [InlineData("YQ===")]
    [InlineData("====")]
    [InlineData("%%%=")]
    [InlineData("________")]
    public void ShouldThrowForBase64IsInvalid(string base64)
    {
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate($"atob('{base64}')"));
    }

    [Fact]
    public void ShouldEncodeAndDecodeLongString()
    {
        var longString = new string('A', 1000);
        Engine.Execute($"const longStr = '{longString}';");
        Engine.Execute("const encoded = btoa(longStr);");
        Engine.Execute("const decoded = atob(encoded);");

        var result = Engine.Evaluate("decoded").AsString();
        Assert.Equal(longString, result);

        var encodedLength = Engine.Evaluate("encoded.length").AsNumber();
        Assert.Equal(Math.Ceiling(1000.0 / 3) * 4, encodedLength);
    }

    [Fact]
    public void ShouldHandleEmptyInput()
    {
        var emptyEncoded = Engine.Evaluate("btoa('')").AsString();
        Assert.Empty(emptyEncoded);

        var emptyDecoded = Engine.Evaluate("atob('')").AsString();
        Assert.Empty(emptyDecoded);
    }

    [Fact]
    public void ShouldHandleAsciiControlCharacters()
    {
        Engine.Execute(
            """
            let controlChars = ''; 
            for (let i = 0; i < 32; i++) {
                controlChars += String.fromCharCode(i);
            }
            const encoded = btoa(controlChars);
            const decoded = atob(encoded);
            """
        );

        var length = Engine.Evaluate("decoded.length").AsNumber();
        Assert.Equal(32, length);
    }

    [Fact]
    public void ShouldCoerceNonStringArgumentsToString()
    {
        Engine.Execute("const numEncoded = btoa(123);");
        Engine.Execute("const numDecoded = atob(numEncoded);");
        var numResult = Engine.Evaluate("numDecoded").AsString();
        Assert.Equal("123", numResult);

        Engine.Execute("const boolEncoded = btoa(true);");
        Engine.Execute("const boolDecoded = atob(boolEncoded);");
        var boolResult = Engine.Evaluate("boolDecoded").AsString();
        Assert.Equal("true", boolResult);

        Engine.Execute("const nullEncoded = btoa(null);");
        Engine.Execute("const nullDecoded = atob(nullEncoded);");
        var nullResult = Engine.Evaluate("nullDecoded").AsString();
        Assert.Equal("null", nullResult);
    }

    [Fact]
    public void ShouldHandleObjectArguments()
    {
        Assert.Equal("W29iamVjdCBPYmplY3Rd", Engine.Evaluate("btoa({})"));
    }

    [Fact]
    public void ShouldHandleMultipleEncodingDecoding()
    {
        Engine.Execute(
            """
            const original = 'Test String';
            const firstEncode = btoa(original);
            const secondEncode = btoa(firstEncode);
            const firstDecode = atob(secondEncode);
            const finalDecode = atob(firstDecode);
            """
        );

        var finalResult = Engine.Evaluate("finalDecode").AsString();
        Assert.Equal("Test String", finalResult);
    }

    [Fact]
    public void ShouldHandleSpecialChars()
    {
        Engine.Execute(
            """
            const specialChars = '\t\n\r\\\'\""';
            const encoded = btoa(specialChars);
            const decoded = atob(encoded);
            """
        );

        var specialResult = Engine.Evaluate("decoded").AsString();
        Assert.Equal("\t\n\r\\\'\"", specialResult);
    }

    [Theory]
    [InlineData(0, 31)]
    [InlineData(32, 47)]
    [InlineData(48, 57)]
    [InlineData(58, 64)]
    [InlineData(65, 90)]
    [InlineData(91, 96)]
    [InlineData(97, 122)]
    [InlineData(123, 126)]
    public void ShouldEncodeAndDecodeAllAsciiRanges(int start, int end)
    {
        var chars = string.Empty;
        for (var i = start; i <= end; i++)
        {
            chars += (char)i;
        }

        var escapedChars = chars
            .Replace("\\", "\\\\")
            .Replace("'", "\\'")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");

        Engine.Execute($"const str = '{escapedChars}';");
        Engine.Execute("const encoded = btoa(str);");
        Engine.Execute("const decoded = atob(encoded);");

        var result = Engine.Evaluate("decoded").AsString();
        Assert.Equal(chars, result);
    }
}
