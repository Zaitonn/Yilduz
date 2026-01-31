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
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Evaluate("typeof writer.close === 'function'").AsBoolean());
    }

    [Fact]
    public void ShouldReturnPromiseFromClose()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const closePromise = writer.close();
            """
        );

        Assert.True(Evaluate("closePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldCallUnderlyingSinkClose()
    {
        Execute(
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

        Assert.True(Evaluate("closeCalled").AsBoolean());
    }

    [Fact]
    public void ShouldResolveClosePromiseOnSuccess()
    {
        Execute(
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

        Assert.True(Evaluate("closeResolved").AsBoolean());
    }

    [Fact]
    public void ShouldRejectClosePromiseOnError()
    {
        Execute(
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

        Assert.True(Evaluate("closeRejected").AsBoolean());
    }

    [Fact]
    public void ShouldRejectWhenWriterIsReleased()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            """
        );

        Assert.Throws<PromiseRejectedException>(() => Evaluate("writer.close()").UnwrapIfPromise());
    }

    [Fact]
    public async Task ShouldRejectWhenStreamIsAlreadyClosed()
    {
        Execute(
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
        Assert.True(Evaluate("closeRejected").AsBoolean());
    }

    [Fact]
    public void ShouldProcessPendingWritesBeforeClosing()
    {
        Execute(
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

        Assert.Equal(2, Evaluate("writtenChunks.length").AsNumber());
        Assert.True(Evaluate("closeCalled").AsBoolean());
    }
}
