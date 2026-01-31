using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.TextDecoderStream;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("encoding")]
    [InlineData("fatal")]
    [InlineData("ignoreBOM")]
    [InlineData("readable")]
    [InlineData("writable")]
    public void ShouldHaveCorrectPrototype(string property)
    {
        Assert.True(
            Evaluate($"TextDecoderStream.prototype.hasOwnProperty('{property}')").AsBoolean()
        );
    }

    [Theory]
    [InlineData("TextDecoderStream.prototype.encoding")]
    [InlineData("TextDecoderStream.prototype.fatal")]
    [InlineData("TextDecoderStream.prototype.ignoreBOM")]
    [InlineData("TextDecoderStream.prototype.readable")]
    [InlineData("TextDecoderStream.prototype.writable")]
    public void ShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }

    [Fact]
    public void ShouldReturnCorrectToString()
    {
        Execute("const stream = new TextDecoderStream();");
        Assert.Equal("[object TextDecoderStream]", Evaluate("stream.toString()"));
    }
}
