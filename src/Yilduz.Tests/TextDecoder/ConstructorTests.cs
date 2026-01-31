using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.TextDecoder;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateInstance()
    {
        Execute("const decoder = new TextDecoder();");

        Assert.True(Evaluate("decoder instanceof TextDecoder").AsBoolean());
    }

    [Fact]
    public void ShouldAcceptEncodingParameter()
    {
        Execute("const decoder = new TextDecoder('utf-8');");

        Assert.Equal("utf-8", Evaluate("decoder.encoding").AsString());
    }

    [Fact]
    public void ShouldAcceptOptionsParameter()
    {
        Execute("const decoder = new TextDecoder('utf-8', { fatal: true, ignoreBOM: true });");

        Assert.True(Evaluate("decoder.fatal").AsBoolean());
        Assert.True(Evaluate("decoder.ignoreBOM").AsBoolean());
    }

    [Fact]
    public void ShouldDefaultToUtf8()
    {
        Execute("const decoder = new TextDecoder();");

        Assert.Equal("utf-8", Evaluate("decoder.encoding").AsString());
    }

    [Theory]
    [InlineData("utf-8", "utf-8")]
    [InlineData("utf8", "utf-8")]
    [InlineData("utf-16", "utf-16le")]
    [InlineData("utf-16le", "utf-16le")]
    [InlineData("utf-16be", "utf-16be")]
    [InlineData("ascii", "windows-1252")]
    [InlineData("us-ascii", "windows-1252")]
    [InlineData("iso-8859-1", "windows-1252")]
    [InlineData("cp1253", "windows-1253")]
    public void ShouldNormalizeEncodingNames(string input, string expected)
    {
        Execute($"const decoder = new TextDecoder('{input}');");

        Assert.Equal(expected, Evaluate("decoder.encoding").AsString());
    }

    [Fact]
    public void ShouldSetFatalToFalseByDefault()
    {
        Execute("const decoder = new TextDecoder();");

        Assert.False(Evaluate("decoder.fatal").AsBoolean());
    }

    [Fact]
    public void ShouldSetIgnoreBOMToFalseByDefault()
    {
        Execute("const decoder = new TextDecoder();");

        Assert.False(Evaluate("decoder.ignoreBOM").AsBoolean());
    }
}
