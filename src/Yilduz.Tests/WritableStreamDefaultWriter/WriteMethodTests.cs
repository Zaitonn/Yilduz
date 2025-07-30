using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class WriteMethodTests : TestBase
{
    [Fact]
    public void ShouldHaveWriteMethod()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("typeof writer.write === 'function'").AsBoolean());
    }

    [Fact]
    public void ShouldReturnPromiseFromWrite()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const writePromise = writer.write('test');
            """
        );

        Assert.True(Engine.Evaluate("writePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldWriteChunk()
    {
        Engine.Execute(
            """
            let writtenChunk = null;
            const stream = new WritableStream({
                write(chunk, controller) {
                    writtenChunk = chunk;
                }
            });
            const writer = stream.getWriter();
            writer.write('test chunk');
            """
        );

        Assert.Equal("test chunk", Engine.Evaluate("writtenChunk").AsString());
    }

    [Fact]
    public void ShouldResolveWritePromiseOnSuccess()
    {
        Engine.Execute(
            """
            let writeResolved = false;
            const stream = new WritableStream({
                write(chunk, controller) {
                    // Successfully write chunk
                }
            });
            const writer = stream.getWriter();
            writer.write('test').then(() => { writeResolved = true; });
            """
        );

        Assert.True(Engine.Evaluate("writeResolved").AsBoolean());
    }

    [Fact]
    public void ShouldRejectWritePromiseOnError()
    {
        Engine.Execute(
            """
            let writeRejected = false;
            const stream = new WritableStream({
                write(chunk, controller) {
                    throw new Error('Write failed');
                }
            });
            const writer = stream.getWriter();
            writer.write('test').catch(() => { writeRejected = true; });
            """
        );

        Assert.True(Engine.Evaluate("writeRejected").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenWriterIsReleased()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            """
        );

        Assert.Throws<PromiseRejectedException>(
            () => Engine.Evaluate("writer.write('test');").UnwrapIfPromise()
        );
    }

    [Fact]
    public void ShouldRejectWriteWhenStreamIsClosed()
    {
        Engine.Execute(
            """
            let writeRejected = false;
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.close();
            writer.write('test').catch(() => { writeRejected = true; });
            """
        );

        Assert.True(Engine.Evaluate("writeRejected").AsBoolean());
    }

    [Fact]
    public void ShouldRejectWriteWhenStreamIsErrored()
    {
        Engine.Execute(
            """
            let writeRejected = false;
            const stream = new WritableStream({
                start(controller) {
                    controller.error(new Error('Stream error'));
                }
            });
            const writer = stream.getWriter();
            writer.write('test').catch(() => { writeRejected = true; });
            """
        );

        Assert.True(Engine.Evaluate("writeRejected").AsBoolean());
    }

    [Fact]
    public void ShouldQueueMultipleWrites()
    {
        Engine.Execute(
            """
            const writtenChunks = [];
            const stream = new WritableStream({
                write(chunk, controller) {
                    writtenChunks.push(chunk);
                }
            });
            const writer = stream.getWriter();
            writer.write('chunk1');
            writer.write('chunk2');
            writer.write('chunk3');
            """
        );

        Assert.Equal(3, Engine.Evaluate("writtenChunks.length").AsNumber());
        Assert.Equal("chunk1", Engine.Evaluate("writtenChunks[0]").AsString());
        Assert.Equal("chunk2", Engine.Evaluate("writtenChunks[1]").AsString());
        Assert.Equal("chunk3", Engine.Evaluate("writtenChunks[2]").AsString());
    }
}
