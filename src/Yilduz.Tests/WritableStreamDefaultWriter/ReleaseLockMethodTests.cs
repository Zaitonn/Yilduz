using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class ReleaseLockMethodTests : TestBase
{
    [Fact]
    public void ShouldHaveReleaseLockMethod()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("typeof writer.releaseLock === 'function'").AsBoolean());
    }

    [Fact]
    public void ShouldUnlockStream()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            """
        );

        Assert.False(Engine.Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldAllowNewWriterAfterRelease()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer1 = stream.getWriter();
            writer1.releaseLock();
            const writer2 = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("writer2 instanceof WritableStreamDefaultWriter").AsBoolean());
        Assert.True(Engine.Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldMakeWriterMethodsThrow()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            """
        );

        Assert.Throws<JavaScriptException>(() => Engine.Execute("writer.write('test');"));
        Assert.Throws<JavaScriptException>(() => Engine.Execute("writer.close();"));
    }

    [Fact]
    public void ShouldRejectReadyPromise()
    {
        Engine.Execute(
            """
            let readyRejected = false;
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.ready.catch(() => { readyRejected = true; });
            writer.releaseLock();
            """
        );

        Assert.True(Engine.Evaluate("readyRejected").AsBoolean());
    }

    [Fact]
    public void ShouldRejectClosedPromise()
    {
        Engine.Execute(
            """
            let closedRejected = false;
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.closed.catch(() => { closedRejected = true; });
            writer.releaseLock();
            """
        );

        Assert.True(Engine.Evaluate("closedRejected").AsBoolean());
    }

    [Fact]
    public void ShouldBeIdempotent()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            writer.releaseLock(); // Should not throw
            """
        );

        Assert.False(Engine.Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenStreamHasPendingOperations()
    {
        Engine.Execute(
            """
            const stream = new WritableStream({
                write(chunk) {
                    // Slow write operation
                    return new Promise(resolve => setTimeout(resolve, 100));
                }
            });
            const writer = stream.getWriter();
            writer.write('test'); // Start pending write
            """
        );

        // This should throw because there's a pending write
        Assert.Throws<JavaScriptException>(() => Engine.Execute("writer.releaseLock();"));
    }
}
