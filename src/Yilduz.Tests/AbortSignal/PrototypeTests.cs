using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.AbortSignal;

public sealed class PrototypeTests : TestBase
{
    public PrototypeTests()
    {
        Engine.AddAbortingApi();
    }

    [Theory]
    [InlineData("aborted")]
    [InlineData("reason")]
    [InlineData("onabort")]
    [InlineData("addEventListener")]
    public void ShouldHaveCorrectPrototype(string propertyName)
    {
        Assert.True(
            Engine.Evaluate($"AbortSignal.prototype.hasOwnProperty('{propertyName}')").AsBoolean()
        );
    }

    [Theory]
    [InlineData("AbortSignal.prototype.aborted")]
    [InlineData("AbortSignal.prototype.reason")]
    [InlineData("AbortSignal.prototype.onabort")]
    [InlineData("AbortSignal.prototype.addEventListener()")]
    public void ShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate(expression));
    }
}
