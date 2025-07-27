using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Base64;

public sealed class Base64Tests : TestBase
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
    [InlineData("________")]
    public void ShouldThrowForBase64IsInvalid(string base64)
    {
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate($"atob('{base64}')"));
    }
}
