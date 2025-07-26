using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.Timers;

public sealed class SetTimeoutTests : TestBase
{
    [Fact]
    public async Task ShouldHandleStringCode()
    {
        Engine.Execute(
            @"
            let executed = false;
            const id = setTimeout('executed = true;', 10);
        "
        );

        await Task.Delay(100);
        Assert.True(Engine.Evaluate("executed").AsBoolean());
    }

    [Fact]
    public async Task ShouldHandleFunctionCallback()
    {
        Engine.Execute(
            @"
            let executed = false;
            const id = setTimeout(() => { executed = true; }, 10);
        "
        );

        await Task.Delay(100);
        Assert.True(Engine.Evaluate("executed").AsBoolean());
    }

    [Fact]
    public async Task ShouldPassArgumentsToCallback()
    {
        Engine.Execute(
            @"
            let result = null;
            const id = setTimeout((a, b, c) => { result = a + b + c; }, 10, 1, 2, 3);
        "
        );

        await Task.Delay(100);
        Assert.Equal(6, Engine.Evaluate("result").AsNumber());
    }

    [Fact]
    public async Task ShouldUseMinimumDelay()
    {
        Engine.Execute(
            @"
            let executed = false;
            const id = setTimeout(() => { executed = true; }, 0);
        "
        );

        await Task.Delay(100);
        Assert.True(Engine.Evaluate("executed").AsBoolean());
    }

    [Fact]
    public async Task ShouldHandleNegativeDelay()
    {
        Engine.Execute(
            @"
            let executed = false;
            const id = setTimeout(() => { executed = true; }, -100);
        "
        );

        await Task.Delay(100);
        Assert.True(Engine.Evaluate("executed").AsBoolean());
    }

    [Fact]
    public async Task ShouldHandleStringDelay()
    {
        Engine.Execute(
            @"
            let executed = false;
            const id = setTimeout(() => { executed = true; }, '10');
        "
        );

        await Task.Delay(100);
        Assert.True(Engine.Evaluate("executed").AsBoolean());
    }

    [Fact]
    public async Task ShouldHandleZeroDelay()
    {
        Engine.Execute("let executed = false;");
        Engine.Execute("setTimeout(() => { executed = true; }, 0);");

        await Task.Delay(100);
        Assert.True(Engine.Evaluate("executed").AsBoolean());
    }

    [Fact]
    public async Task ShouldHandleVeryLongDelay()
    {
        Engine.Execute("let executed = false;");
        Engine.Execute("const id = setTimeout(() => { executed = true; }, 10000);");

        await Task.Delay(100);
        Assert.False(Engine.Evaluate("executed").AsBoolean());

        Engine.Execute("clearTimeout(id);");
    }

    [Fact]
    public async Task SetTimeoutShouldExecuteOnce()
    {
        Engine.Execute("let count = 0;");
        Engine.Execute("const id = setTimeout(() => { count++; }, 20);");

        await Task.Delay(100);
        Assert.Equal(1, Engine.Evaluate("count").AsNumber());
    }

    [Fact]
    public async Task MultipleTimeoutsShouldExecuteIndependently()
    {
        Engine.Execute("let results = [];");
        Engine.Execute("setTimeout(() => { results.push('first'); }, 10);");
        Engine.Execute("setTimeout(() => { results.push('second'); }, 20);");
        Engine.Execute("setTimeout(() => { results.push('third'); }, 30);");

        await Task.Delay(100);
        Assert.Equal(3, Engine.Evaluate("results.length").AsNumber());
    }

    [Fact]
    public async Task ShouldMaintainExecutionOrder()
    {
        Engine.Execute("let order = [];");
        Engine.Execute("setTimeout(() => { order.push(1); }, 30);");
        Engine.Execute("setTimeout(() => { order.push(2); }, 10);");
        Engine.Execute("setTimeout(() => { order.push(3); }, 20);");

        await Task.Delay(100);
        Assert.Equal(2, Engine.Evaluate("order[0]").AsNumber()); // 10ms delay
        Assert.Equal(3, Engine.Evaluate("order[1]").AsNumber()); // 20ms delay
        Assert.Equal(1, Engine.Evaluate("order[2]").AsNumber()); // 30ms delay
    }

    [Fact]
    public async Task ShouldHandleNestedTimeouts()
    {
        Engine.Execute(
            @"
            let results = [];
            setTimeout(() => {
                results.push('outer');
                setTimeout(() => {
                    results.push('inner');
                }, 10);
            }, 10);
        "
        );

        await Task.Delay(100);
        Assert.Equal(2, Engine.Evaluate("results.length").AsNumber());
        Assert.Equal("outer", Engine.Evaluate("results[0]").AsString());
        Assert.Equal("inner", Engine.Evaluate("results[1]").AsString());
    }

    [Fact]
    public async Task ShouldHandleTimeoutWithComplexCallback()
    {
        Engine.Execute(
            @"
            let result = null;
            setTimeout(() => {
                const obj = { value: 42 };
                const arr = [1, 2, 3];
                result = {
                    objectValue: obj.value,
                    arraySum: arr.reduce((a, b) => a + b, 0)
                };
            }, 10);
        "
        );

        await Task.Delay(100);
        Assert.Equal(42, Engine.Evaluate("result.objectValue").AsNumber());
        Assert.Equal(6, Engine.Evaluate("result.arraySum").AsNumber());
    }

    [Fact]
    public async Task ShouldHandleErrorsInTimeoutCallback()
    {
        Engine.Execute("let executed = false;");
        Engine.Execute(
            @"
            setTimeout(() => {
                throw new Error('Test error');
            }, 10);
            setTimeout(() => {
                executed = true;
            }, 20);
        "
        );

        await Task.Delay(100);
        Assert.True(Engine.Evaluate("executed").AsBoolean()); // Second timeout should still execute
    }
}
