using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class ReleaseLockMethodTests : TestBase
{
    [Fact]
    public void ShouldHaveReleaseLockMethod()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Evaluate("typeof writer.releaseLock === 'function'").AsBoolean());
    }

    [Fact]
    public void ShouldUnlockStream()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            """
        );

        Assert.False(Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldAllowNewWriterAfterRelease()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer1 = stream.getWriter();
            writer1.releaseLock();
            const writer2 = stream.getWriter();
            """
        );

        Assert.True(Evaluate("writer2 instanceof WritableStreamDefaultWriter").AsBoolean());
        Assert.True(Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldMakeWriterMethodsThrow()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            """
        );

        Assert.Throws<PromiseRejectedException>(
            () => Evaluate("writer.write('test')").UnwrapIfPromise()
        );
        Assert.Throws<PromiseRejectedException>(() => Evaluate("writer.close()").UnwrapIfPromise());
    }

    [Fact]
    public void ShouldRejectReadyPromise()
    {
        Execute(
            """
            let readyRejected = false;
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            writer.ready.catch(() => { readyRejected = true; });
            """
        );

        Assert.True(Evaluate("readyRejected").AsBoolean());
    }

    [Fact]
    public void ShouldRejectClosedPromise()
    {
        Execute(
            """
            let closedRejected = false;
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.closed.catch(() => { closedRejected = true; });
            writer.releaseLock();
            """
        );

        Assert.True(Evaluate("closedRejected").AsBoolean());
    }

    [Fact]
    public void ShouldBeIdempotent()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            writer.releaseLock(); // Should not throw
            """
        );

        Assert.False(Evaluate("stream.locked").AsBoolean());
    }
}
