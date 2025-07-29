using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultController;

public sealed class SignalPropertyTests : TestBase
{
    [Fact]
    public void ShouldHaveSignalProperty()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });
            """
        );

        Assert.True(Engine.Evaluate("'signal' in controller").AsBoolean());
    }

    [Fact]
    public void ShouldReturnAbortSignal()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });
            """
        );

        Assert.True(Engine.Evaluate("controller.signal instanceof AbortSignal").AsBoolean());
    }

    [Fact]
    public void ShouldNotBeAbortedInitially()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });
            """
        );

        Assert.False(Engine.Evaluate("controller.signal.aborted").AsBoolean());
    }

    [Fact]
    public void ShouldBeAbortedWhenStreamIsAborted()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            const writer = stream.getWriter();
            writer.abort('test reason');
            """
        );

        Assert.True(Engine.Evaluate("controller.signal.aborted").AsBoolean());
    }

    [Fact]
    public void ShouldFireAbortEventWhenStreamIsAborted()
    {
        Engine.Execute(
            """
            let controller = null;
            let abortEventFired = false;

            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                    ctrl.signal.addEventListener('abort', () => {
                        abortEventFired = true;
                    });
                }
            });

            const writer = stream.getWriter();
            writer.abort('test reason');
            """
        );

        Assert.True(Engine.Evaluate("abortEventFired").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectAbortReason()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            const writer = stream.getWriter();
            writer.abort('custom abort reason');
            """
        );

        Assert.Equal("custom abort reason", Engine.Evaluate("controller.signal.reason").AsString());
    }

    [Fact]
    public void ShouldBeReadOnly()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            const originalSignal = controller.signal;
            """
        );

        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("controller.signal = new AbortSignal();")
        );

        // Signal should remain the same
        Assert.True(Engine.Evaluate("controller.signal === originalSignal").AsBoolean());
    }

    [Fact]
    public void ShouldAllowListeningToAbortEvents()
    {
        Engine.Execute(
            """
            let controller = null;
            let eventListenerCalled = false;

            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            controller.signal.onabort = () => {
                eventListenerCalled = true;
            };

            const writer = stream.getWriter();
            writer.abort();
            """
        );

        Assert.True(Engine.Evaluate("eventListenerCalled").AsBoolean());
    }
}
