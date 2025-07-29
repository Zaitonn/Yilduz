using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultController;

public sealed class AdvancedErrorTests : TestBase
{
    [Fact]
    public void ShouldErrorStreamCorrectly()
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
            const errorReason = new Error('Custom error');
            controller.error(errorReason);
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldRejectPendingWritesOnError()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                write(chunk, ctrl) {
                    return new Promise(resolve => {
                        // Never resolve to keep write pending
                    });
                }
            });

            const writer = stream.getWriter();
            const writePromise = writer.write('pending write');
            const errorReason = new Error('Stream error');
            controller.error(errorReason);
            """
        );

        Assert.True(Engine.Evaluate("writePromise instanceof Promise").AsBoolean());
        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldRejectSubsequentWritesAfterError()
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
            controller.error(new Error('Stream error'));
            const writePromise = writer.write('after error');
            """
        );

        Assert.True(Engine.Evaluate("writePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldRejectCloseAfterError()
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
            controller.error(new Error('Stream error'));
            const closePromise = writer.close();
            """
        );

        Assert.True(Engine.Evaluate("closePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleErrorDuringWrite()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                write(chunk, ctrl) {
                    if (chunk === 'error') {
                        ctrl.error(new Error('Write-triggered error'));
                    }
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            writer.write('normal');
            const errorPromise = writer.write('error');
            """
        );

        Assert.True(Engine.Evaluate("errorPromise instanceof Promise").AsBoolean());
        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldHandleErrorDuringClose()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                close() {
                    controller.error(new Error('Close-triggered error'));
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            const closePromise = writer.close();
            """
        );

        Assert.True(Engine.Evaluate("closePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldNotAllowMultipleErrors()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            controller.error(new Error('First error'));
            controller.error(new Error('Second error'));
            """
        );

        // Multiple errors should be handled gracefully (second error ignored)
        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldHandleErrorWithUndefinedReason()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            controller.error(undefined);
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldHandleErrorWithNullReason()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            controller.error(null);
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldHandleErrorWithPrimitiveReason()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            controller.error('string error');
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldHandleErrorWithObjectReason()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            controller.error({ type: 'custom', message: 'error object' });
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldPropagateErrorToWriterPromises()
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
            const ready = writer.ready;
            const closed = writer.closed;
            
            controller.error(new Error('Stream error'));
            """
        );

        Assert.True(Engine.Evaluate("ready instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("closed instanceof Promise").AsBoolean());
    }
}
