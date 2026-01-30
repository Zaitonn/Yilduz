using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.TextEncoderStream;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("encoding")]
    [InlineData("readable")]
    [InlineData("writable")]
    public void ShouldHaveCorrectPrototype(string property)
    {
        Assert.True(
            Engine.Evaluate($"TextEncoderStream.prototype.hasOwnProperty('{property}')").AsBoolean()
        );
    }

    [Theory]
    [InlineData("TextEncoderStream.prototype.encoding")]
    [InlineData("TextEncoderStream.prototype.readable")]
    [InlineData("TextEncoderStream.prototype.writable")]
    public void ShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate(expression));
    }

    [Fact]
    public void ShouldReturnCorrectToString()
    {
        Engine.Execute("const stream = new TextEncoderStream();");
        Assert.Equal("[object TextEncoderStream]", Engine.Evaluate("stream.toString()"));
    }
}
