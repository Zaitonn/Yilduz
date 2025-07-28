using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.TextEncoder;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("encoding")]
    [InlineData("encode")]
    [InlineData("encodeInto")]
    public void ShouldHaveCorrectPrototype(string property)
    {
        Assert.True(
            Engine.Evaluate($"TextEncoder.prototype.hasOwnProperty('{property}')").AsBoolean()
        );
    }

    [Theory]
    [InlineData("TextEncoder.prototype.encoding")]
    [InlineData("TextEncoder.prototype.encode()")]
    [InlineData("TextEncoder.prototype.encodeInto()")]
    public void ShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate(expression));
    }

    [Fact]
    public void ShouldReturnCorrectToString()
    {
        Engine.Execute("const encoder = new TextEncoder();");
        Assert.Equal("[object TextEncoder]", Engine.Evaluate("encoder.toString()"));
    }
}
