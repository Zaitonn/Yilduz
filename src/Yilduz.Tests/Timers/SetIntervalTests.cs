using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.Timers;

public sealed class SetIntervalTests : TestBase
{
    [Fact]
    public async Task SetIntervalShouldExecuteRepeatedly()
    {
        Execute("let count = 0;");
        Execute("const id = setInterval(() => { count++; }, 20);");

        await Task.Delay(1000);
        Execute("clearInterval(id);");

        Assert.InRange(Evaluate("count").AsNumber(), 3, 1000); // Should execute multiple times
    }

    [Fact]
    public async Task MultipleIntervalsShouldWorkIndependently()
    {
        Execute("let count1 = 0, count2 = 0;");
        Execute("const id1 = setInterval(() => { count1++; }, 15);");
        Execute("const id2 = setInterval(() => { count2++; }, 25);");

        await Task.Delay(1000);
        Execute("clearInterval(id1);");
        Execute("clearInterval(id2);");

        var count1 = Evaluate("count1").AsNumber();
        var count2 = Evaluate("count2").AsNumber();

        Assert.True(count1 > 0);
        Assert.True(count2 > 0);
        Assert.True(count1 >= count2); // Faster interval should execute more
    }

    [Fact]
    public async Task ShouldHandleIntervalWithComplexCallback()
    {
        Execute(
            """
            let results = [];
            const id = setInterval(() => {
                const timestamp = Date.now();
                results.push({ timestamp, length: results.length });
                if (results.length >= 3) {
                    clearInterval(id);
                }
            }, 15);
            """
        );

        await Task.Delay(100);
        Assert.True(Evaluate("results.length").AsNumber() >= 3);
        Assert.Equal("number", Evaluate("typeof results[0].timestamp").AsString());
    }

    [Fact]
    public async Task ShouldHandleErrorsInIntervalCallback()
    {
        Execute("let count = 0;");
        Execute(
            """
            const id = setInterval(() => {
                count++;
                if (count === 2) {
                    throw new Error('Test error');
                }
                if (count >= 4) {
                    clearInterval(id);
                }
            }, 15);
            """
        );

        await Task.Delay(100);
        Assert.True(Evaluate("count").AsNumber() >= 4); // Should continue after error
    }
}
