using System;
using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.Timer;

public sealed class SetIntervalTests : TestBase
{
    public SetIntervalTests()
    {
        Engine.AddTimerApi(TimeSpan.FromSeconds(500), Token);
    }

    [Fact]
    public async Task CanSetIntervalWithFunction()
    {
        Engine.Execute(
            """
            var count = 0;
            setInterval(() => {
                count++;
            }, 100);
            """
        );

        await Task.Delay(380);

        var result = Engine.GetValue("count");
        Assert.Equal(3, result);
    }

    [Fact]
    public async Task CanSetIntervalWithString()
    {
        Engine.Execute(
            """
            var count = 0;
            setInterval("count++", 100);
            """
        );

        await Task.Delay(380);

        var result = Engine.GetValue("count");
        Assert.Equal(3, result);
    }
}
