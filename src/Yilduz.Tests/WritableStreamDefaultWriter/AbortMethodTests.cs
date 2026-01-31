using Jint;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class AbortMethodTests : TestBase
{
    [Fact]
    public void ShouldHaveAbortMethod()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.Equal("function", Evaluate("typeof writer.abort"));
    }

    [Fact]
    public void ShouldReturnPromiseFromAbort()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const abortPromise = writer.abort();
            """
        );

        Assert.True(Evaluate("abortPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldCallUnderlyingSinkAbort()
    {
        Execute(
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

        Assert.True(Evaluate("abortCalled").AsBoolean());
        Assert.Equal("test reason", Evaluate("abortReason").AsString());
    }

    [Fact]
    public void ShouldResolveAbortPromiseOnSuccess()
    {
        Execute(
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

        Assert.True(Evaluate("abortResolved").AsBoolean());
    }

    [Fact]
    public void ShouldRejectAbortPromiseOnError()
    {
        Execute(
            """
            let abortRejected = false;
            const stream = new WritableStream({
                abort(reason) {
                    throw new Error('Abort failed');
                }
            });
            const writer = stream.getWriter();
            """
        );

        Evaluate("writer.abort().catch(() => { abortRejected = true; })").UnwrapIfPromise();

        Assert.True(Evaluate("abortRejected").AsBoolean());
    }

    [Fact]
    public void ShouldRejectedWhenWriterIsReleased()
    {
        Execute(
            """
            let abortRejected = false;
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            writer.abort().catch(() => { abortRejected = true; });
            """
        );

        Assert.True(Evaluate("abortRejected").AsBoolean());
    }

    [Fact]
    public void ShouldResolveWhenStreamIsAlreadyClosed()
    {
        Execute(
            """
            let abortResolved = false;
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Evaluate("writer.close()").UnwrapIfPromise();
        Evaluate("writer.abort().then(() => { abortResolved = true; })").UnwrapIfPromise();

        Assert.True(Evaluate("abortResolved").AsBoolean());
    }

    [Fact]
    public void ShouldResolveWhenStreamIsAlreadyErrored()
    {
        Execute(
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

        Assert.True(Evaluate("abortResolved").AsBoolean());
    }

    [Fact]
    public void ShouldRejectClosedPromiseAfterAbort()
    {
        Execute(
            """
            let closedRejected = false;
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.closed.catch(() => { closedRejected = true; });
            """
        );

        Evaluate("writer.abort(new Error('Aborted'))").UnwrapIfPromise();

        Assert.True(Evaluate("closedRejected").AsBoolean());
    }
}
