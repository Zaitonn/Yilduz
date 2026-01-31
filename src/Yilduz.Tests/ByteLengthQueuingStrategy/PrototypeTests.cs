using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.ByteLengthQueuingStrategy;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("constructor")]
    [InlineData("highWaterMark")]
    [InlineData("size")]
    public void ByteLengthQueuingStrategyShouldHaveCorrectPrototype(string propertyName)
    {
        Assert.True(
            Engine
                .Evaluate($"ByteLengthQueuingStrategy.prototype.hasOwnProperty('{propertyName}')")
                .AsBoolean()
        );
    }

    [Theory]
    [InlineData("ByteLengthQueuingStrategy.prototype.highWaterMark")]
    [InlineData("ByteLengthQueuingStrategy.prototype.size()")]
    public void ByteLengthQueuingStrategyShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Execute("const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 1 });");
        Assert.Equal(
            "[object ByteLengthQueuingStrategy]",
            Evaluate("Object.prototype.toString.call(strategy)").AsString()
        );
    }

    [Fact]
    public void ShouldHaveCorrectConstructorName()
    {
        Assert.Equal(
            "ByteLengthQueuingStrategy",
            Evaluate("ByteLengthQueuingStrategy.name").AsString()
        );
    }

    [Fact]
    public void ShouldNotBeCallableAsFunction()
    {
        Assert.Throws<JavaScriptException>(
            () => Evaluate("ByteLengthQueuingStrategy({ highWaterMark: 1 })")
        );
    }

    [Fact]
    public void HighWaterMarkPropertyShouldBeReadOnly()
    {
        Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 16 });
            const originalHighWaterMark = strategy.highWaterMark;
            strategy.highWaterMark = 32;
            """
        );
        Assert.Equal(
            Evaluate("originalHighWaterMark").AsNumber(),
            Evaluate("strategy.highWaterMark").AsNumber()
        );
    }
}
