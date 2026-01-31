using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultController;

public sealed class SignalPropertyTests : TestBase
{
    [Fact]
    public void ShouldHaveSignalProperty()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });
            """
        );

        Assert.True(Evaluate("'signal' in controller").AsBoolean());
    }

    [Fact]
    public void ShouldReturnAbortSignal()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });
            """
        );

        Assert.True(Evaluate("controller.signal instanceof AbortSignal").AsBoolean());
    }

    [Fact]
    public void ShouldNotBeAbortedInitially()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });
            """
        );

        Assert.False(Evaluate("controller.signal.aborted").AsBoolean());
    }

    [Fact]
    public void ShouldBeAbortedWhenStreamIsAborted()
    {
        Execute(
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

        Assert.True(Evaluate("controller.signal.aborted").AsBoolean());
    }

    [Fact]
    public void ShouldFireAbortEventWhenStreamIsAborted()
    {
        Execute(
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

        Assert.True(Evaluate("abortEventFired").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectAbortReason()
    {
        Execute(
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

        Assert.Equal("custom abort reason", Evaluate("controller.signal.reason").AsString());
    }

    [Fact]
    public void ShouldBeReadOnly()
    {
        Execute(
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

        Assert.Throws<JavaScriptException>(() => Execute("controller.signal = new AbortSignal();"));

        // Signal should remain the same
        Assert.True(Evaluate("controller.signal === originalSignal").AsBoolean());
    }

    [Fact]
    public void ShouldAllowListeningToAbortEvents()
    {
        Execute(
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

        Assert.True(Evaluate("eventListenerCalled").AsBoolean());
    }
}
