using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class ClosedPromiseTests : TestBase
{
    [Fact]
    public void ShouldHaveClosedPromiseOnCreation()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("writer.closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldResolveClosedPromiseWhenClosed()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const closed = writer.closed;
            writer.close();
            """
        );

        Assert.True(Engine.Evaluate("closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldRejectClosedPromiseOnError()
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
            const closed = writer.closed;
            
            // Error the stream
            controller.error(new Error('Stream error'));
            """
        );

        Assert.True(Engine.Evaluate("closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldRejectClosedPromiseOnAbort()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const closed = writer.closed;
            writer.abort(new Error('Abort reason'));
            """
        );

        Assert.True(Engine.Evaluate("closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHaveClosedPromiseResolvedForClosedStream()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer1 = stream.getWriter();
            writer1.close();
            writer1.releaseLock();
            
            // Get a new writer for the closed stream
            const writer2 = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("writer2.closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHaveClosedPromiseRejectedForErroredStream()
    {
        Engine.Execute(
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

        Assert.True(Engine.Evaluate("writer2.closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleClosedPromiseAfterReleaseLock()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const closed = writer.closed;
            writer.releaseLock();
            """
        );

        Assert.True(Engine.Evaluate("closed instanceof Promise").AsBoolean());
        
        // Accessing closed after release should throw
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate("writer.closed"));
    }

    [Fact]
    public void ShouldHandleClosedPromiseWithCloseError()
    {
        Engine.Execute(
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

        Assert.True(Engine.Evaluate("closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleClosedPromiseWithAsyncClose()
    {
        Engine.Execute(
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

        Assert.True(Engine.Evaluate("closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldMaintainClosedPromiseIdentity()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const closed1 = writer.closed;
            const closed2 = writer.closed;
            """
        );

        Assert.True(Engine.Evaluate("closed1 === closed2").AsBoolean());
    }
}
