using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.CountQueuingStrategy;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateCountQueuingStrategyWithHighWaterMark()
    {
        Engine.Execute("const strategy = new CountQueuingStrategy({ highWaterMark: 4 });");
        Assert.Equal("CountQueuingStrategy", Engine.Evaluate("strategy.constructor.name"));
        Assert.Equal(4, Engine.Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldCreateCountQueuingStrategyWithZeroHighWaterMark()
    {
        Engine.Execute("const strategy = new CountQueuingStrategy({ highWaterMark: 0 });");
        Assert.Equal(0, Engine.Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldCreateCountQueuingStrategyWithFloatHighWaterMark()
    {
        Engine.Execute("const strategy = new CountQueuingStrategy({ highWaterMark: 2.5 });");
        Assert.Equal(2.5, Engine.Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldThrowWhenConstructedWithoutOptions()
    {
        Engine.Execute(
            """
            let caughtError;
            try {
                new CountQueuingStrategy();
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
                new CountQueuingStrategy({});
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
                new CountQueuingStrategy(null);
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
        Engine.Execute("const strategy = new CountQueuingStrategy({ highWaterMark: -2 });");
        Assert.Equal(-2, Engine.Evaluate("strategy.highWaterMark").AsNumber());
    }

    [Fact]
    public void ShouldIgnoreExtraProperties()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ 
                highWaterMark: 3, 
                extraProperty: 'ignored' 
            });
            """
        );
        Assert.Equal(3, Engine.Evaluate("strategy.highWaterMark").AsNumber());
        Assert.True(Engine.Evaluate("strategy.extraProperty === undefined").AsBoolean());
    }

    [Fact]
    public void ShouldAcceptInfinityAsHighWaterMark()
    {
        Engine.Execute("const strategy = new CountQueuingStrategy({ highWaterMark: Infinity });");
        Assert.True(Engine.Evaluate("strategy.highWaterMark === Infinity").AsBoolean());
    }
}
