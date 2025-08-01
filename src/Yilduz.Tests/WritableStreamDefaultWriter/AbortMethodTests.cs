using Jint;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class AbortMethodTests : TestBase
{
    [Fact]
    public void ShouldHaveAbortMethod()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.Equal("function", Engine.Evaluate("typeof writer.abort"));
    }

    [Fact]
    public void ShouldReturnPromiseFromAbort()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const abortPromise = writer.abort();
            """
        );

        Assert.True(Engine.Evaluate("abortPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldCallUnderlyingSinkAbort()
    {
        Engine.Execute(
            """
            let abortCalled = false;
            let abortReason = null;
            const stream = new WritableStream({
                abort(reason) {
                    abortCalled = true;
                    abortReason = reason;
                }
            });
            const writer = stream.getWriter();
            writer.abort('test reason');
            """
        );

        Assert.True(Engine.Evaluate("abortCalled").AsBoolean());
        Assert.Equal("test reason", Engine.Evaluate("abortReason").AsString());
    }

    [Fact]
    public void ShouldResolveAbortPromiseOnSuccess()
    {
        Engine.Execute(
            """
            let abortResolved = false;
            const stream = new WritableStream({
                abort(reason) {
                    // Successfully abort
                }
            });
            const writer = stream.getWriter();
            writer.abort().then(() => { abortResolved = true; });
            """
        );

        Assert.True(Engine.Evaluate("abortResolved").AsBoolean());
    }

    [Fact]
    public void ShouldRejectAbortPromiseOnError()
    {
        Engine.Execute(
            """
            let abortRejected = false;
            const stream = new WritableStream({
                abort(reason) {
                    throw new Error('Abort failed');
                }
            });
            const writer = stream.getWriter();
            writer.abort().catch(() => { abortRejected = true; });
            """
        );

        Assert.True(Engine.Evaluate("abortRejected").AsBoolean());
    }

    [Fact]
    public void ShouldRejectedWhenWriterIsReleased()
    {
        Engine.Execute(
            """
            let abortRejected = false;
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            writer.abort().catch(() => { abortRejected = true; });
            """
        );

        Assert.True(Engine.Evaluate("abortRejected").AsBoolean());
    }

    [Fact]
    public void ShouldResolveWhenStreamIsAlreadyClosed()
    {
        Engine.Execute(
            """
            let abortResolved = false;
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.close();
            writer.abort().then(() => { abortResolved = true; });
            """
        );

        Assert.True(Engine.Evaluate("abortResolved").AsBoolean());
    }

    [Fact]
    public void ShouldResolveWhenStreamIsAlreadyErrored()
    {
        Engine.Execute(
            """
            let abortResolved = false;
            const stream = new WritableStream({
                start(controller) {
                    controller.error(new Error('Stream error'));
                }
            });
            const writer = stream.getWriter();
            writer.abort().then(() => { abortResolved = true; });
            """
        );

        Assert.True(Engine.Evaluate("abortResolved").AsBoolean());
    }

    [Fact]
    public void ShouldRejectClosedPromiseAfterAbort()
    {
        Engine.Execute(
            """
            let closedRejected = false;
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.closed.catch(() => { closedRejected = true; });
            writer.abort(new Error('Aborted'));
            """
        );

        Assert.True(Engine.Evaluate("closedRejected").AsBoolean());
    }

    [Fact(Skip = "sebastienros/jint#2157")]
    public void ShouldAbortPendingWrites()
    {
        Engine.Execute(
            """
            let writeRejected = false;
            const stream = new WritableStream({
                write(chunk) {
                    // This write will be aborted
                }
            });
            const writer = stream.getWriter();
            writer.write('chunk').catch(() => { writeRejected = true; });
            writer.abort(new Error('Aborted'));
            """
        );

        Assert.True(Engine.Evaluate("writeRejected").AsBoolean());
    }
}
