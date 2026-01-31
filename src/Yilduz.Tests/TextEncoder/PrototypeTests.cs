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
        Assert.True(Evaluate($"TextEncoder.prototype.hasOwnProperty('{property}')").AsBoolean());
    }

    [Theory]
    [InlineData("TextEncoder.prototype.encoding")]
    [InlineData("TextEncoder.prototype.encode()")]
    [InlineData("TextEncoder.prototype.encodeInto()")]
    public void ShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }

    [Fact]
    public void ShouldReturnCorrectToString()
    {
        Execute("const encoder = new TextEncoder();");
        Assert.Equal("[object TextEncoder]", Evaluate("encoder.toString()"));
    }
}
