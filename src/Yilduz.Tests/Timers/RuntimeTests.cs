using System;
using System.Threading.Tasks;
using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Timers;

public sealed class RuntimeTests : TestBase
{
    public RuntimeTests()
    {
        Engine.AddTimerApi(TimeSpan.FromSeconds(500), Token);
    }

    [Theory]
    [InlineData("setTimeout")]
    [InlineData("setInterval")]
    public void ShouldThrowWhenGivenEmptyArg(string method)
    {
        Assert.Throws<JavaScriptException>(() => Engine.Execute($"{method}();"));
    }

    [Theory]
    [InlineData("setTimeout")]
    [InlineData("setInterval")]
    public void ShouldNotThrowWhenAnyArgIsGiven(string method)
    {
        Assert.Equal(1, Engine.Evaluate($"{method}(null)"));
    }

    [Theory]
    [InlineData("setTimeout")]
    [InlineData("setInterval")]
    public void CanAcceptStringAsTimeout(string method)
    {
        Engine.Execute(
            $"""
            {method}("", 100);
            {method}("", "100");
            """
        );
    }

    [Fact]
    public async Task CanCallObjectToStringMethod()
    {
        Engine.Execute(
            """
            var log = '';
            function logger(s) { log += s + ' '; }

            setTimeout({
                toString: function () {
                    setTimeout("logger('ONE')", 100);
                    return "logger('TWO')";
                }
            }, 100);
            """
        );

        await Task.Delay(300);

        Assert.Equal("ONE TWO ", Engine.GetValue("log"));
    }

    [Theory]
    [InlineData("setTimeout")]
    [InlineData("setInterval")]
    public void ShouldNotThrowWhenObjectDoesntHaveToStringMethod(string method)
    {
        Engine.Execute(
            $$"""
            {{method}}({
                foo: function() {
                    return "bar";
                }
            }, 100);
            """
        );
    }

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
}
