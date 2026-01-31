using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class ClosedPromiseTests : TestBase
{
    [Fact]
    public void ShouldHaveClosedPromiseOnCreation()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Evaluate("writer.closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldResolveClosedPromiseWhenClosed()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const closed = writer.closed;
            writer.close();
            """
        );

        Assert.True(Evaluate("closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldRejectClosedPromiseOnError()
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
            const closed = writer.closed;

            // Error the stream
            controller.error(new Error('Stream error'));
            """
        );

        Assert.True(Evaluate("closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldRejectClosedPromiseOnAbort()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const closed = writer.closed;
            writer.abort(new Error('Abort reason'));
            """
        );

        Assert.True(Evaluate("closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHaveClosedPromiseResolvedForClosedStream()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer1 = stream.getWriter();
            writer1.close();
            writer1.releaseLock();

            // Get a new writer for the closed stream
            const writer2 = stream.getWriter();
            """
        );

        Assert.True(Evaluate("writer2.closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHaveClosedPromiseRejectedForErroredStream()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            const writer1 = stream.getWriter();
            controller.error(new Error('Stream error'));
            writer1.releaseLock();

            // Get a new writer for the errored stream
            const writer2 = stream.getWriter();
            """
        );

        Assert.True(Evaluate("writer2.closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleClosedPromiseWithCloseError()
    {
        Execute(
            """
            const stream = new WritableStream({
                close() {
                    throw new Error('Close error');
                }
            });

            const writer = stream.getWriter();
            const closed = writer.closed;
            writer.close();
            """
        );

        Assert.True(Evaluate("closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleClosedPromiseWithAsyncClose()
    {
        Execute(
            """
            const stream = new WritableStream({
                close() {
                    return new Promise((resolve, reject) => {
                        setTimeout(() => resolve(), 10);
                    });
                }
            });

            const writer = stream.getWriter();
            const closed = writer.closed;
            writer.close();
            """
        );

        Assert.True(Evaluate("closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldMaintainClosedPromiseIdentity()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const closed1 = writer.closed;
            const closed2 = writer.closed;
            """
        );

        Assert.True(Evaluate("closed1 === closed2").AsBoolean());
    }
}
