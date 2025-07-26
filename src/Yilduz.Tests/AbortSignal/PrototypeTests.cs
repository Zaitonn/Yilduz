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
            Engine.Evaluate($"AbortSignal.prototype.hasOwnProperty('{propertyName}')").AsBoolean()
        );
    }

    [Fact]
    public void ShouldHaveCorrectPrototypeChain()
    {
        Assert.IsType<AbortSignalConstructor>(Engine.Evaluate("AbortSignal.prototype.constructor"));
        Assert.IsType<EventTargetConstructor>(
            Engine.Evaluate("AbortSignal.prototype.__proto__.constructor")
        );
        Assert.IsType<ObjectConstructor>(
            Engine.Evaluate("AbortSignal.prototype.__proto__.__proto__.constructor")
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
