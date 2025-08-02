using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.CountQueuingStrategy;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("constructor")]
    [InlineData("highWaterMark")]
    [InlineData("size")]
    public void CountQueuingStrategyShouldHaveCorrectPrototype(string propertyName)
    {
        Assert.True(
            Engine
                .Evaluate($"CountQueuingStrategy.prototype.hasOwnProperty('{propertyName}')")
                .AsBoolean()
        );
    }

    [Theory]
    [InlineData("CountQueuingStrategy.prototype.highWaterMark")]
    [InlineData("CountQueuingStrategy.prototype.size()")]
    public void CountQueuingStrategyShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate(expression));
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Engine.Execute("const strategy = new CountQueuingStrategy({ highWaterMark: 1 });");
        Assert.Equal(
            "[object CountQueuingStrategy]",
            Engine.Evaluate("Object.prototype.toString.call(strategy)").AsString()
        );
    }

    [Fact]
    public void ShouldHaveCorrectConstructorName()
    {
        Assert.Equal(
            "CountQueuingStrategy",
            Engine.Evaluate("CountQueuingStrategy.name").AsString()
        );
    }

    [Fact]
    public void ShouldNotBeCallableAsFunction()
    {
        Assert.Throws<JavaScriptException>(
            () => Engine.Evaluate("CountQueuingStrategy({ highWaterMark: 1 })")
        );
    }

    [Fact]
    public void HighWaterMarkPropertyShouldBeReadOnly()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 5 });
            const originalHighWaterMark = strategy.highWaterMark;
            strategy.highWaterMark = 10;
            """
        );
        Assert.Equal(
            Engine.Evaluate("originalHighWaterMark").AsNumber(),
            Engine.Evaluate("strategy.highWaterMark").AsNumber()
        );
    }
}
