using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.CountQueuingStrategy;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateCountQueuingStrategyWithHighWaterMark()
    {
        Execute("const strategy = new CountQueuingStrategy({ highWaterMark: 4 });");
        Assert.Equal("CountQueuingStrategy", Evaluate("strategy.constructor.name"));
        Assert.Equal(4, Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldCreateCountQueuingStrategyWithZeroHighWaterMark()
    {
        Execute("const strategy = new CountQueuingStrategy({ highWaterMark: 0 });");
        Assert.Equal(0, Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldCreateCountQueuingStrategyWithFloatHighWaterMark()
    {
        Execute("const strategy = new CountQueuingStrategy({ highWaterMark: 2.5 });");
        Assert.Equal(2.5, Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldThrowWhenConstructedWithoutOptions()
    {
        Execute(
            """
            let caughtError;
            try {
                new CountQueuingStrategy();
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
                new CountQueuingStrategy({});
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
                new CountQueuingStrategy(null);
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
        Execute("const strategy = new CountQueuingStrategy({ highWaterMark: -2 });");
        Assert.Equal(-2, Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldIgnoreExtraProperties()
    {
        Execute(
            """
            const strategy = new CountQueuingStrategy({ 
                highWaterMark: 3, 
                extraProperty: 'ignored' 
            });
            """
        );
        Assert.Equal(3, Evaluate("strategy.highWaterMark").AsNumber());
        Assert.True(Evaluate("strategy.extraProperty === undefined").AsBoolean());
    }

    [Fact]
    public void ShouldAcceptInfinityAsHighWaterMark()
    {
        Execute("const strategy = new CountQueuingStrategy({ highWaterMark: Infinity });");
        Assert.True(Evaluate("strategy.highWaterMark === Infinity").AsBoolean());
    }
}
