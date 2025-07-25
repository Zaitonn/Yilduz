using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.AbortController;

public sealed class PrototypeTests : TestBase
{
    public PrototypeTests()
    {
        Engine.AddAbortingApi();
    }

    [Fact]
    public void ShouldHaveCorrectPrototype()
    {
        Assert.True(
            Engine.Evaluate("AbortController.prototype.hasOwnProperty('constructor')").AsBoolean()
        );
        Assert.True(
            Engine.Evaluate("AbortController.prototype.hasOwnProperty('signal')").AsBoolean()
        );
        Assert.True(
            Engine.Evaluate("AbortController.prototype.hasOwnProperty('abort')").AsBoolean()
        );
    }

    [Fact]
    public void ShouldThrowOnInvalidInvocation()
    {
        Assert.Throws<JavaScriptException>(
            () => Engine.Evaluate("AbortController.prototype.constructor()")
        );

        Assert.Throws<JavaScriptException>(
            () => Engine.Evaluate("AbortController.prototype.abort()")
        );

        Assert.Throws<JavaScriptException>(
            () => Engine.Evaluate("AbortController.prototype.signal")
        );
    }
}
