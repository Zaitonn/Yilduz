using Jint;
using Xunit;

namespace Yilduz.Tests.ByteLengthQueuingStrategy;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateByteLengthQueuingStrategyWithHighWaterMark()
    {
        Engine.Execute("const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 16 });");
        Assert.Equal("ByteLengthQueuingStrategy", Engine.Evaluate("strategy.constructor.name"));
        Assert.Equal(16, Engine.Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldCreateByteLengthQueuingStrategyWithZeroHighWaterMark()
    {
        Engine.Execute("const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 0 });");
        Assert.Equal(0, Engine.Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldCreateByteLengthQueuingStrategyWithFloatHighWaterMark()
    {
        Engine.Execute("const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 1.5 });");
        Assert.Equal(1.5, Engine.Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldThrowWhenConstructedWithoutOptions()
    {
        Engine.Execute(
            """
            let caughtError;
            try {
                new ByteLengthQueuingStrategy();
            } catch (e) {
                caughtError = e;
            }
            """
        );
        Assert.True(Engine.Evaluate("caughtError instanceof TypeError").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenConstructedWithoutHighWaterMark()
    {
        Engine.Execute(
            """
            let caughtError;
            try {
                new ByteLengthQueuingStrategy({});
            } catch (e) {
                caughtError = e;
            }
            """
        );
        Assert.True(Engine.Evaluate("caughtError instanceof TypeError").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenConstructedWithNullOptions()
    {
        Engine.Execute(
            """
            let caughtError;
            try {
                new ByteLengthQueuingStrategy(null);
            } catch (e) {
                caughtError = e;
            }
            """
        );
        Assert.True(Engine.Evaluate("caughtError instanceof TypeError").AsBoolean());
    }

    [Fact]
    public void ShouldAcceptNegativeHighWaterMark()
    {
        Engine.Execute("const strategy = new ByteLengthQueuingStrategy({ highWaterMark: -1 });");
        Assert.Equal(-1, Engine.Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldIgnoreExtraProperties()
    {
        Engine.Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ 
                highWaterMark: 8, 
                extraProperty: 'ignored' 
            });
            """
        );
        Assert.Equal(8, Engine.Evaluate("strategy.highWaterMark").AsNumber());
        Assert.True(Engine.Evaluate("strategy.extraProperty === undefined").AsBoolean());
    }
}
