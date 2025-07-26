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
        Engine.Execute(
            """
            var count = 0;
            const id = setInterval(() => {
                count++;
            }, 100);
            """
        );

        await Task.Delay(250);
        Assert.Equal(2, Engine.GetValue("count"));

        Engine.Execute($"{method}(id);");

        await Task.Delay(250);
        Assert.Equal(2, Engine.GetValue("count"));
    }

    [Theory]
    [InlineData("clearInterval")]
    [InlineData("clearTimeout")]
    public void ShouldNotThrowWhenTryingToClearNonExistentTimer(string method)
    {
        Engine.Execute($"{method}(123456789);");
        Engine.Execute($"{method}(null);");
        Engine.Execute($"{method}(undefined);");
        Engine.Execute($"{method}();");
    }

    [Fact]
    public async Task ClearTimeoutShouldPreventExecution()
    {
        Engine.Execute("let executed = false;");
        Engine.Execute("const id = setTimeout(() => { executed = true; }, 50);");
        Engine.Execute("clearTimeout(id);");

        await Task.Delay(100);
        Assert.False(Engine.Evaluate("executed").AsBoolean());
    }

    [Fact]
    public async Task ClearTimeoutAfterExecutionShouldBeHarmless()
    {
        Engine.Execute("let count = 0;");
        Engine.Execute("const id = setTimeout(() => { count++; }, 10);");

        await Task.Delay(300);
        Engine.Execute("clearTimeout(id);");

        Assert.Equal(1, Engine.Evaluate("count").AsNumber());
    }

    [Fact]
    public void ClearTimeoutWithInvalidIdShouldNotThrow()
    {
        Engine.Execute("clearTimeout(99999);");
        Engine.Execute("clearTimeout(null);");
        Engine.Execute("clearTimeout(undefined);");
        Engine.Execute("clearTimeout('invalid');");
    }

    [Fact]
    public async Task ClearIntervalShouldStopExecution()
    {
        Engine.Execute("let count = 0;");
        Engine.Execute("const id = setInterval(() => { count++; }, 10);");

        await Task.Delay(100);
        Engine.Execute("clearInterval(id);");

        var countAfterClear = Engine.Evaluate("count").AsNumber();
        await Task.Delay(100);
        var countAfterWait = Engine.Evaluate("count").AsNumber();

        Assert.Equal(countAfterClear, countAfterWait);
    }
}
