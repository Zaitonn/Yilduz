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
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Execute("const strategy = new CountQueuingStrategy({ highWaterMark: 1 });");
        Assert.Equal(
            "[object CountQueuingStrategy]",
            Evaluate("Object.prototype.toString.call(strategy)").AsString()
        );
    }

    [Fact]
    public void ShouldHaveCorrectConstructorName()
    {
        Assert.Equal("CountQueuingStrategy", Evaluate("CountQueuingStrategy.name").AsString());
    }

    [Fact]
    public void ShouldNotBeCallableAsFunction()
    {
        Assert.Throws<JavaScriptException>(
            () => Evaluate("CountQueuingStrategy({ highWaterMark: 1 })")
        );
    }

    [Fact]
    public void HighWaterMarkPropertyShouldBeReadOnly()
    {
        Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 5 });
            const originalHighWaterMark = strategy.highWaterMark;
            strategy.highWaterMark = 10;
            """
        );
        Assert.Equal(
            Evaluate("originalHighWaterMark").AsNumber(),
            Evaluate("strategy.highWaterMark").AsNumber()
        );
    }
}
