using Jint;
using Xunit;

namespace Yilduz.Tests.AbortSignal;

public sealed class EventTests : TestBase
{
    public EventTests()
    {
        Engine.AddAbortingApi().AddEventsApi();
    }

    [Fact]
    public void CanAddEventListener()
    {
        Engine.Execute(
            """
            const controller = new AbortController();
            const signal = controller.signal;
            let called = false;

            signal.addEventListener("abort", () => {
                called = true;
            });
            controller.abort();
            """
        );

        Assert.True(Engine.Evaluate("called").AsBoolean());
    }

    [Fact]
    public void CanSetOnAbort()
    {
        Engine.Execute(
            """
            const controller = new AbortController();
            const signal = controller.signal;
            let called = false;

            signal.onabort = () => {
                called = true;
            };
            controller.abort();
            """
        );

        Assert.True(Engine.Evaluate("called").AsBoolean());
    }
}
