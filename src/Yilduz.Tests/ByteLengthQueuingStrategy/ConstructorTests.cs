using Jint;
using Xunit;

namespace Yilduz.Tests.ByteLengthQueuingStrategy;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateByteLengthQueuingStrategyWithHighWaterMark()
    {
        Execute("const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 16 });");
        Assert.Equal("ByteLengthQueuingStrategy", Evaluate("strategy.constructor.name"));
        Assert.Equal(16, Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldCreateByteLengthQueuingStrategyWithZeroHighWaterMark()
    {
        Execute("const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 0 });");
        Assert.Equal(0, Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldCreateByteLengthQueuingStrategyWithFloatHighWaterMark()
    {
        Execute("const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 1.5 });");
        Assert.Equal(1.5, Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldThrowWhenConstructedWithoutOptions()
    {
        Execute(
            """
            let caughtError;
            try {
                new ByteLengthQueuingStrategy();
            } catch (e) {
                caughtError = e;
            }
            """
        );
        Assert.True(Evaluate("caughtError instanceof TypeError").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenConstructedWithoutHighWaterMark()
    {
        Execute(
            """
            let caughtError;
            try {
                new ByteLengthQueuingStrategy({});
            } catch (e) {
                caughtError = e;
            }
            """
        );
        Assert.True(Evaluate("caughtError instanceof TypeError").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenConstructedWithNullOptions()
    {
        Execute(
            """
            let caughtError;
            try {
                new ByteLengthQueuingStrategy(null);
            } catch (e) {
                caughtError = e;
            }
            """
        );
        Assert.True(Evaluate("caughtError instanceof TypeError").AsBoolean());
    }

    [Fact]
    public void ShouldAcceptNegativeHighWaterMark()
    {
        Execute("const strategy = new ByteLengthQueuingStrategy({ highWaterMark: -1 });");
        Assert.Equal(-1, Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldIgnoreExtraProperties()
    {
        Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ 
                highWaterMark: 8, 
                extraProperty: 'ignored' 
            });
            """
        );
        Assert.Equal(8, Evaluate("strategy.highWaterMark").AsNumber());
        Assert.True(Evaluate("strategy.extraProperty === undefined").AsBoolean());
    }
}
