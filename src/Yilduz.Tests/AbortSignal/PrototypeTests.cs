using Jint;
using Jint.Native.Object;
using Jint.Runtime;
using Xunit;
using Yilduz.Aborting.AbortSignal;
using Yilduz.Events.EventTarget;

namespace Yilduz.Tests.AbortSignal;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("aborted")]
    [InlineData("reason")]
    [InlineData("onabort")]
    public void ShouldHaveCorrectPrototype(string propertyName)
    {
        Assert.True(
            Evaluate($"AbortSignal.prototype.hasOwnProperty('{propertyName}')").AsBoolean()
        );
    }

    [Fact]
    public void ShouldHaveCorrectPrototypeChain()
    {
        Assert.IsType<AbortSignalConstructor>(Evaluate("AbortSignal.prototype.constructor"));
        Assert.IsType<EventTargetConstructor>(
            Evaluate("AbortSignal.prototype.__proto__.constructor")
        );
        Assert.IsType<ObjectConstructor>(
            Evaluate("AbortSignal.prototype.__proto__.__proto__.constructor")
        );
    }

    [Theory]
    [InlineData("AbortSignal.prototype.aborted")]
    [InlineData("AbortSignal.prototype.reason")]
    [InlineData("AbortSignal.prototype.onabort")]
    [InlineData("AbortSignal.prototype.addEventListener()")]
    public void ShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }
}
