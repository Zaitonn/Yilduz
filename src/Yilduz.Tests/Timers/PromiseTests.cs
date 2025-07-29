using System.Diagnostics;
using Jint;
using Jint.Native;
using Xunit;

namespace Yilduz.Tests.Timers;

public sealed class PromiseTests : TestBase
{
    [Fact]
    public void CanUsePromiseWithTimers()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        Engine.Execute("let executed = false;");

        Assert.Equal(
            JsValue.Undefined,
            Engine
                .Evaluate(
                    """
                    new Promise((resolve) => {
                        setTimeout(() => {
                            executed = true;
                            resolve();
                        }, 100);
                    });
                    """
                )
                .UnwrapIfPromise()
        );
        stopwatch.Stop();

        Assert.True(Engine.Evaluate("executed").AsBoolean());
        Assert.InRange(stopwatch.ElapsedMilliseconds, 0, 500);
    }
}
