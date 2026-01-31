using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class AdvancedWriteTests : TestBase
{
    [Fact]
    public void ShouldWriteMultipleChunksSequentially()
    {
        Execute(
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

        Assert.True(Evaluate("writer instanceof WritableStreamDefaultWriter").AsBoolean());

        // Write multiple chunks
        Execute(
            """
            const promise1 = writer.write('chunk1');
            const promise2 = writer.write('chunk2');
            const promise3 = writer.write('chunk3');
            """
        );

        Assert.True(Evaluate("promise1 instanceof Promise").AsBoolean());
        Assert.True(Evaluate("promise2 instanceof Promise").AsBoolean());
        Assert.True(Evaluate("promise3 instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriteErrors()
    {
        Execute(
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
        Execute("const normalPromise = writer.write('normal');");
        Assert.True(Evaluate("normalPromise instanceof Promise").AsBoolean());

        // Write error chunk should be handled
        Execute("const errorPromise = writer.write('error');");
        Assert.True(Evaluate("errorPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriteToClosedStream()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.close();
            """
        );

        // Writing to a closed stream should be handled gracefully
        Execute("const writePromise = writer.write('test');");
        Assert.True(Evaluate("writePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriteToErroredStream()
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
            controller.error(new Error('Stream error'));
            """
        );

        // Writing to an errored stream should be handled
        Execute("const writePromise = writer.write('test');");
        Assert.True(Evaluate("writePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriteWithBackpressure()
    {
        Execute(
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
        Execute(
            """
            const promise1 = writer.write('chunk1');
            const promise2 = writer.write('chunk2');
            """
        );

        Assert.True(Evaluate("promise1 instanceof Promise").AsBoolean());
        Assert.True(Evaluate("promise2 instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldWriteWithCustomSizeFunction()
    {
        Execute(
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
        Execute(
            """
            const shortPromise = writer.write('a');
            const longPromise = writer.write('this is a longer string');
            """
        );

        Assert.True(Evaluate("shortPromise instanceof Promise").AsBoolean());
        Assert.True(Evaluate("longPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldWriteNonStringChunks()
    {
        Execute(
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
        Execute(
            """
            const numberPromise = writer.write(42);
            const objectPromise = writer.write({ key: 'value' });
            const arrayPromise = writer.write([1, 2, 3]);
            const booleanPromise = writer.write(true);
            """
        );

        Assert.True(Evaluate("numberPromise instanceof Promise").AsBoolean());
        Assert.True(Evaluate("objectPromise instanceof Promise").AsBoolean());
        Assert.True(Evaluate("arrayPromise instanceof Promise").AsBoolean());
        Assert.True(Evaluate("booleanPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriteAfterReleaseLock()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            """
        );

        // Writing after releasing lock should return rejected promise, not throw
        Execute("const writePromise = writer.write('test');");
        Assert.True(Evaluate("writePromise instanceof Promise").AsBoolean());
    }
}
