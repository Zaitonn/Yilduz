using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.AbortController;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("constructor")]
    [InlineData("signal")]
    [InlineData("abort")]
    public void ShouldHaveCorrectPrototype(string propertyName)
    {
        Assert.True(
            Evaluate($"AbortController.prototype.hasOwnProperty('{propertyName}')").AsBoolean()
        );
    }

    [Theory]
    [InlineData("AbortController.prototype.constructor()")]
    [InlineData("AbortController.prototype.abort()")]
    [InlineData("AbortController.prototype.signal")]
    public void ShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }
}
