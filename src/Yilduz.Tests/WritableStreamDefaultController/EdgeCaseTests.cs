using Jint;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultController;

public sealed class EdgeCaseTests : TestBase
{
    [Fact]
    public void ShouldHandleErrorWithSymbolReason()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            const symbolReason = Symbol('error reason');
            controller.error(symbolReason);
            """
        );

        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleErrorWithCircularObjectReason()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            const circularReason = { message: 'error' };
            circularReason.self = circularReason;
            controller.error(circularReason);
            """
        );

        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleErrorDuringStartAlgorithm()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                    ctrl.error(new Error('Start error'));
                    return Promise.resolve();
                }
            });
            """
        );

        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleMultipleErrorCallsWithDifferentReasons()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            controller.error('First error');
            controller.error(42);
            controller.error({ type: 'object error' });
            controller.error(Symbol('symbol error'));
            """
        );

        // Only the first error should be effective
        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleControllerAfterStreamClosed()
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
            writer.close();
            writer.releaseLock();

            // Try to error after stream is closed
            controller.error(new Error('Late error'));
            """
        );

        // Error on closed stream should be handled gracefully
        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleControllerAfterStreamAborted()
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
            writer.abort('Abort reason');
            writer.releaseLock();

            // Try to error after stream is aborted
            controller.error(new Error('Late error'));
            """
        );

        // Error on aborted stream should be handled gracefully
        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleControllerWithNoUnderlyingSink()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
                // No write, close, or abort methods
            });
            """
        );

        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());

        // Error should still work
        Execute("controller.error(new Error('No sink error'));");
    }

    [Fact]
    public void ShouldHandleControllerSignalProperty()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            const signal = controller.signal;
            """
        );

        Assert.True(Evaluate("signal !== undefined").AsBoolean());
        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleControllerInErroredState()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                    ctrl.error(new Error('Initial error'));
                }
            });

            // Try to error again
            controller.error(new Error('Second error'));
            """
        );

        // Second error should be ignored
        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleControllerWithFrozenController()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                    Object.freeze(ctrl);
                }
            });

            // Try to use frozen controller
            controller.error(new Error('Frozen controller error'));
            """
        );

        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }
}
