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

    [Fact]
    public void ShouldHaveCorrectPrototype()
    {
        Assert.True(Engine.Evaluate("AbortSignal.prototype.hasOwnProperty('aborted')").AsBoolean());
        Assert.True(Engine.Evaluate("AbortSignal.prototype.hasOwnProperty('reason')").AsBoolean());
    }

    [Fact]
    public void ShouldThrowOnInvalidInvocation()
    {
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate("AbortSignal.prototype.aborted"));
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate("AbortSignal.prototype.reason "));
        Assert.Throws<JavaScriptException>(
            () => Engine.Evaluate("AbortSignal.prototype.addEventListener()")
        );
    }
}
