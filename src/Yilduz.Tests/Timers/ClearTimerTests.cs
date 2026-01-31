using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.Timers;

public sealed class ClearTimerTests : TestBase
{
    [Theory]
    [InlineData("clearInterval")]
    [InlineData("clearTimeout")]
    public async Task CanRemoveIntervalTimer(string method)
    {
        Execute(
            """
            var count = 0;
            const id = setInterval(() => {
                count++;
            }, 100);
            """
        );

        await Task.Delay(250);
        Assert.Equal(2, Engine.GetValue("count"));

        Execute($"{method}(id);");

        await Task.Delay(250);
        Assert.Equal(2, Engine.GetValue("count"));
    }

    [Theory]
    [InlineData("clearInterval")]
    [InlineData("clearTimeout")]
    public void ShouldNotThrowWhenTryingToClearNonExistentTimer(string method)
    {
        Execute($"{method}(123456789);");
        Execute($"{method}(null);");
        Execute($"{method}(undefined);");
        Execute($"{method}();");
    }

    [Fact]
    public async Task ClearTimeoutShouldPreventExecution()
    {
        Execute("let executed = false;");
        Execute("const id = setTimeout(() => { executed = true; }, 50);");
        Execute("clearTimeout(id);");

        await Task.Delay(100);
        Assert.False(Evaluate("executed").AsBoolean());
    }

    [Fact]
    public async Task ClearTimeoutAfterExecutionShouldBeHarmless()
    {
        Execute("let count = 0;");
        Execute("const id = setTimeout(() => { count++; }, 10);");

        await Task.Delay(300);
        Execute("clearTimeout(id);");

        Assert.Equal(1, Evaluate("count").AsNumber());
    }

    [Fact]
    public void ClearTimeoutWithInvalidIdShouldNotThrow()
    {
        Execute("clearTimeout(99999);");
        Execute("clearTimeout(null);");
        Execute("clearTimeout(undefined);");
        Execute("clearTimeout('invalid');");
    }

    [Fact]
    public async Task ClearIntervalShouldStopExecution()
    {
        Execute("let count = 0;");
        Execute("const id = setInterval(() => { count++; }, 10);");

        await Task.Delay(100);
        Execute("clearInterval(id);");

        var countAfterClear = Evaluate("count").AsNumber();
        await Task.Delay(100);
        var countAfterWait = Evaluate("count").AsNumber();

        Assert.Equal(countAfterClear, countAfterWait);
    }
}
