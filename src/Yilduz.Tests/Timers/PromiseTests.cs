using Jint;
using Jint.Native;
using Xunit;

namespace Yilduz.Tests.Timers;

public sealed class PromiseTests : TestBase
{
    [Fact]
    public void CanUsePromiseWithTimers()
    {
        Execute("let executed = false;");

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

        Assert.True(Evaluate("executed").AsBoolean());
    }
}
