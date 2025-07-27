using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.AbortSignal;

public sealed class EventTests : TestBase
{
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
    public void ShouldHandleComplexAbortScenarios()
    {
        Engine.Execute(
            """
            const controllers = [];
            const results = [];

            for (let i = 0; i < 5; i++) {
                const controller = new AbortController();
                controllers.push(controller);
                
                controller.signal.addEventListener('abort', () => {
                    results.push(`Controller ${i} aborted`);
                });
            }

            // Abort some controllers
            controllers[1].abort('Reason 1');
            controllers[3].abort('Reason 3');
            """
        );

        Assert.Equal(2, Engine.Evaluate("results.length").AsNumber());
        Assert.Equal("Controller 1 aborted", Engine.Evaluate("results[0]").AsString());
        Assert.Equal("Controller 3 aborted", Engine.Evaluate("results[1]").AsString());
    }

    [Fact]
    public void ShouldHandleErrorsInAbortListeners()
    {
        Engine.Execute(
            """
            const controller = new AbortController();
            let secondListenerExecuted = false;

            controller.signal.addEventListener('abort', () => {
                throw new Error('Listener error');
            });

            controller.signal.addEventListener('abort', () => {
                secondListenerExecuted = true;
            });

            controller.abort();
            """
        );

        Assert.True(Engine.Evaluate("secondListenerExecuted").AsBoolean());
    }

    [Fact]
    public async Task ShouldSupportStaticTimeoutMethod()
    {
        Engine.Execute(
            """
            const signal = AbortSignal.timeout(50);
            let timeoutFired = false;

            signal.addEventListener('abort', () => {
                timeoutFired = true;
            });
            """
        );

        await Task.Delay(100);

        Assert.True(Engine.Evaluate("timeoutFired").AsBoolean());
        Assert.True(Engine.Evaluate("signal.aborted").AsBoolean());
        Assert.Equal("TimeoutError", Engine.Evaluate("signal.reason.name"));
        Assert.Equal("signal timed out", Engine.Evaluate("signal.reason.message"));
    }

    [Fact]
    public void ShouldHandleAbortSignalAny()
    {
        Engine.Execute(
            """
            const controller1 = new AbortController();
            const controller2 = new AbortController();
            const controller3 = new AbortController();

            const compositeSignal = AbortSignal.any([
                controller1.signal,
                controller2.signal,
                controller3.signal
            ]);

            let abortEventFired = false;
            compositeSignal.addEventListener('abort', () => {
                abortEventFired = true;
            });

            controller2.abort('Second controller aborted');
            """
        );

        Assert.True(Engine.Evaluate("compositeSignal.aborted").AsBoolean());
        Assert.True(Engine.Evaluate("abortEventFired").AsBoolean());
        Assert.Equal("Second controller aborted", Engine.Evaluate("compositeSignal.reason"));
    }

    [Fact]
    public void ShouldFireAbortEventOnSignal()
    {
        Engine.Execute(
            """
            const controller = new AbortController();
            let eventFired = false;
            let eventReason = null;

            controller.signal.addEventListener('abort', (event) => {
                eventFired = true;
                eventReason = controller.signal.reason;
            });

            controller.abort('test reason');
            """
        );

        Assert.True(Engine.Evaluate("eventFired").AsBoolean());
        Assert.Equal("test reason", Engine.Evaluate("eventReason").AsString());
    }

    [Fact]
    public void ShouldSupportMultipleAbortListeners()
    {
        Engine.Execute(
            """
            const controller = new AbortController();
            let listener1Executed = false;
            let listener2Executed = false;
            let listener3Executed = false;

            controller.signal.addEventListener('abort', () => { listener1Executed = true; });
            controller.signal.addEventListener('abort', () => { listener2Executed = true; });
            controller.signal.addEventListener('abort', () => { listener3Executed = true; });

            controller.abort();
            """
        );

        Assert.True(Engine.Evaluate("listener1Executed").AsBoolean());
        Assert.True(Engine.Evaluate("listener2Executed").AsBoolean());
        Assert.True(Engine.Evaluate("listener3Executed").AsBoolean());
    }

    [Fact]
    public void ShouldNotFireEventIfAlreadyAborted()
    {
        Engine.Execute(
            """
            const controller = new AbortController();
            let eventCount = 0;

            controller.signal.addEventListener('abort', () => { eventCount++; });

            controller.abort('first');
            controller.abort('second');
            controller.abort('third');
            """
        );

        Assert.Equal(1, Engine.Evaluate("eventCount").AsNumber());
    }

    [Fact]
    public void ShouldSupportOnAbortProperty()
    {
        Engine.Execute(
            """
            const controller = new AbortController();
            let onAbortExecuted = false;

            controller.signal.onabort = () => { onAbortExecuted = true; };
            controller.abort();
            """
        );

        Assert.True(Engine.Evaluate("onAbortExecuted").AsBoolean());
    }

    [Fact]
    public void ShouldExecuteBothOnAbortAndEventListener()
    {
        Engine.Execute(
            """
            const controller = new AbortController();
            let onAbortExecuted = false;
            let eventListenerExecuted = false;

            controller.signal.onabort = () => { onAbortExecuted = true; };
            controller.signal.addEventListener('abort', () => { eventListenerExecuted = true; });

            controller.abort();
            """
        );

        Assert.True(Engine.Evaluate("onAbortExecuted").AsBoolean());
        Assert.True(Engine.Evaluate("eventListenerExecuted").AsBoolean());
    }

    [Fact]
    public void ShouldAllowListenerRemoval()
    {
        Engine.Execute(
            """
            const controller = new AbortController();
            let executed = false;

            const listener = () => { executed = true; };
            controller.signal.addEventListener('abort', listener);
            controller.signal.removeEventListener('abort', listener);

            controller.abort();
            """
        );

        Assert.False(Engine.Evaluate("executed").AsBoolean());
    }
}
