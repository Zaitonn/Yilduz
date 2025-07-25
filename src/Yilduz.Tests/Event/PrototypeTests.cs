using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Event;

public sealed class PrototypeTests : TestBase
{
    public PrototypeTests()
    {
        Engine.AddEventsApi();
    }

    [Theory]
    [InlineData("bubbles")]
    [InlineData("cancelable")]
    [InlineData("composed")]
    [InlineData("currentTarget")]
    [InlineData("defaultPrevented")]
    [InlineData("eventPhase")]
    [InlineData("isTrusted")]
    [InlineData("target")]
    [InlineData("timeStamp")]
    [InlineData("type")]
    [InlineData("composedPath")]
    [InlineData("preventDefault")]
    [InlineData("stopPropagation")]
    [InlineData("stopImmediatePropagation")]
    public void ShouldHaveCorrectPrototype(string property)
    {
        Assert.True(Engine.Evaluate($"Event.prototype.hasOwnProperty('{property}')").AsBoolean());
    }

    [Theory]
    [InlineData("Event.prototype.bubbles")]
    [InlineData("Event.prototype.cancelable")]
    [InlineData("Event.prototype.composed")]
    [InlineData("Event.prototype.currentTarget")]
    [InlineData("Event.prototype.defaultPrevented")]
    [InlineData("Event.prototype.eventPhase")]
    [InlineData("Event.prototype.isTrusted")]
    [InlineData("Event.prototype.target")]
    [InlineData("Event.prototype.timeStamp")]
    [InlineData("Event.prototype.type")]
    [InlineData("Event.prototype.composedPath()")]
    [InlineData("Event.prototype.preventDefault()")]
    [InlineData("Event.prototype.stopPropagation()")]
    [InlineData("Event.prototype.stopImmediatePropagation()")]
    public void ShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate(expression));
    }
}
