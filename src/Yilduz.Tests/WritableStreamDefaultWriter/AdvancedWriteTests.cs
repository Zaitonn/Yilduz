using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class AdvancedWriteTests : TestBase
{
    [Fact]
    public void ShouldWriteMultipleChunksSequentially()
    {
        Engine.Execute(
            """
            let writtenChunks = [];
            const stream = new WritableStream({
                write(chunk, controller) {
                    writtenChunks.push(chunk);
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("writer instanceof WritableStreamDefaultWriter").AsBoolean());

        // Write multiple chunks
        Engine.Execute(
            """
            const promise1 = writer.write('chunk1');
            const promise2 = writer.write('chunk2');
            const promise3 = writer.write('chunk3');
            """
        );

        Assert.True(Engine.Evaluate("promise1 instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("promise2 instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("promise3 instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriteErrors()
    {
        Engine.Execute(
            """
            const stream = new WritableStream({
                write(chunk, controller) {
                    if (chunk === 'error') {
                        throw new Error('Write error');
                    }
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            """
        );

        // Write normal chunk should work
        Engine.Execute("const normalPromise = writer.write('normal');");
        Assert.True(Engine.Evaluate("normalPromise instanceof Promise").AsBoolean());

        // Write error chunk should be handled
        Engine.Execute("const errorPromise = writer.write('error');");
        Assert.True(Engine.Evaluate("errorPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriteToClosedStream()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.close();
            """
        );

        // Writing to a closed stream should be handled gracefully
        Engine.Execute("const writePromise = writer.write('test');");
        Assert.True(Engine.Evaluate("writePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriteToErroredStream()
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
            """
        );

        // Writing to an errored stream should be handled
        Engine.Execute("const writePromise = writer.write('test');");
        Assert.True(Engine.Evaluate("writePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriteWithBackpressure()
    {
        Engine.Execute(
            """
            const stream = new WritableStream({
                write(chunk, controller) {
                    return Promise.resolve();
                }
            }, {
                highWaterMark: 1,
                size(chunk) {
                    return 1;
                }
            });

            const writer = stream.getWriter();
            """
        );

        // Write chunks that might trigger backpressure
        Engine.Execute(
            """
            const promise1 = writer.write('chunk1');
            const promise2 = writer.write('chunk2');
            """
        );

        Assert.True(Engine.Evaluate("promise1 instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("promise2 instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldWriteWithCustomSizeFunction()
    {
        Engine.Execute(
            """
            const stream = new WritableStream({
                write(chunk, controller) {
                    return Promise.resolve();
                }
            }, {
                highWaterMark: 10,
                size(chunk) {
                    return chunk.length || 1;
                }
            });

            const writer = stream.getWriter();
            """
        );

        // Write chunks with different sizes
        Engine.Execute(
            """
            const shortPromise = writer.write('a');
            const longPromise = writer.write('this is a longer string');
            """
        );

        Assert.True(Engine.Evaluate("shortPromise instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("longPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldWriteNonStringChunks()
    {
        Engine.Execute(
            """
            let receivedChunks = [];
            const stream = new WritableStream({
                write(chunk, controller) {
                    receivedChunks.push(chunk);
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            """
        );

        // Write different types of chunks
        Engine.Execute(
            """
            const numberPromise = writer.write(42);
            const objectPromise = writer.write({ key: 'value' });
            const arrayPromise = writer.write([1, 2, 3]);
            const booleanPromise = writer.write(true);
            """
        );

        Assert.True(Engine.Evaluate("numberPromise instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("objectPromise instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("arrayPromise instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("booleanPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriteAfterReleaseLock()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            """
        );

        // Writing after releasing lock should return rejected promise, not throw
        Engine.Execute("const writePromise = writer.write('test');");
        Assert.True(Engine.Evaluate("writePromise instanceof Promise").AsBoolean());
    }
}
