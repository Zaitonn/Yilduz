using Jint;
using Xunit;

namespace Yilduz.Tests.Fetch;

public sealed class BaseUrlMissingTests : TestBase
{
    [Fact]
    public void ShouldRejectRelativeFetchWhenBaseUrlIsMissing()
    {
        Execute(
            """
            let caught = false;
            let errorType = '';
            async function run() {
                try {
                    await fetch('/api/ping');
                } catch (e) {
                    caught = true;
                    errorType = e.constructor.name;
                }
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.True(Evaluate("caught").AsBoolean());
        Assert.Equal("TypeError", Evaluate("errorType").AsString());
    }
}
