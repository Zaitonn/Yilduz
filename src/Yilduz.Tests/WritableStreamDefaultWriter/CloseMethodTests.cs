using System.Threading.Tasks;
using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class CloseMethodTests : TestBase
{
    [Fact]
    public void ShouldHaveCloseMethod()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("typeof writer.close === 'function'").AsBoolean());
    }

    [Fact]
    public void ShouldReturnPromiseFromClose()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const closePromise = writer.close();
            """
        );

        Assert.True(Engine.Evaluate("closePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldCallUnderlyingSinkClose()
    {
        Engine.Execute(
            """
            let closeCalled = false;
            const stream = new WritableStream({
                close() {
                    closeCalled = true;
                }
            });
            const writer = stream.getWriter();
            writer.close();
            """
        );

        Assert.True(Engine.Evaluate("closeCalled").AsBoolean());
    }

    [Fact]
    public void ShouldResolveClosePromiseOnSuccess()
    {
        Engine.Execute(
            """
            let closeResolved = false;
            const stream = new WritableStream({
                close() {
                    // Successfully close
                }
            });
            const writer = stream.getWriter();
            writer.close().then(() => { closeResolved = true; });
            """
        );

        Assert.True(Engine.Evaluate("closeResolved").AsBoolean());
    }

    [Fact]
    public void ShouldRejectClosePromiseOnError()
    {
        Engine.Execute(
            """
            let closeRejected = false;
            const stream = new WritableStream({
                close() {
                    throw new Error('Close failed');
                }
            });
            const writer = stream.getWriter();
            writer.close().catch(() => { closeRejected = true; });
            """
        );

        Assert.True(Engine.Evaluate("closeRejected").AsBoolean());
    }

    [Fact]
    public void ShouldRejectWhenWriterIsReleased()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            """
        );

        Assert.Throws<PromiseRejectedException>(
            () => Engine.Evaluate("writer.close()").UnwrapIfPromise()
        );
    }

    [Fact]
    public async Task ShouldRejectWhenStreamIsAlreadyClosed()
    {
        Engine.Execute(
            """
            let closeRejected = false;
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.close();
            writer.close().catch(() => { closeRejected = true; });
            """
        );

        await WaitForJsConditionAsync("closeRejected === true");
        // Explicit assertion for test clarity and documentation
        Assert.True(Engine.Evaluate("closeRejected").AsBoolean());
    }

    [Fact]
    public void ShouldProcessPendingWritesBeforeClosing()
    {
        Engine.Execute(
            """
            const writtenChunks = [];
            let closeCalled = false;
            const stream = new WritableStream({
                write(chunk) {
                    writtenChunks.push(chunk);
                },
                close() {
                    closeCalled = true;
                }
            });
            const writer = stream.getWriter();
            writer.write('chunk1');
            writer.write('chunk2');
            writer.close();
            """
        );

        Assert.Equal(2, Engine.Evaluate("writtenChunks.length").AsNumber());
        Assert.True(Engine.Evaluate("closeCalled").AsBoolean());
    }
}
