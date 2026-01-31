using Jint;
using Jint.Native;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class DesiredSizeTests : TestBase
{
    [Fact]
    public void ShouldHaveDesiredSizeProperty()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Evaluate("'desiredSize' in writer").AsBoolean());
    }

    [Fact]
    public void ShouldReturnCorrectDesiredSizeForEmptyStream()
    {
        Execute(
            """
            const stream = new WritableStream({}, {
                highWaterMark: 5,
                size() { return 1; }
            });
            const writer = stream.getWriter();
            """
        );

        Assert.True(Evaluate("typeof writer.desiredSize === 'number'").AsBoolean());
        Assert.Equal(5, Evaluate("writer.desiredSize").AsNumber());
    }

    [Fact]
    public void ShouldUpdateDesiredSizeAfterWrite()
    {
        Execute(
            """
            const stream = new WritableStream({
                write(chunk, controller) {
                    return Promise.resolve();
                }
            }, {
                highWaterMark: 3,
                size() { return 1; }
            });

            const writer = stream.getWriter();
            const initialSize = writer.desiredSize;
            writer.write('chunk1');
            const afterWriteSize = writer.desiredSize;
            """
        );

        Assert.Equal(3, Evaluate("initialSize").AsNumber());
        Assert.Equal(2, Evaluate("afterWriteSize").AsNumber());
    }

    [Fact]
    public void ShouldHandleDesiredSizeWithCustomSizeFunction()
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
            const initialSize = writer.desiredSize;
            writer.write('hello'); // size = 5
            const afterWriteSize = writer.desiredSize;
            """
        );

        Assert.Equal(10, Evaluate("initialSize").AsNumber());
        Assert.Equal(5, Evaluate("afterWriteSize").AsNumber());
    }

    [Fact]
    public void ShouldReturnNegativeDesiredSizeWhenOverCapacity()
    {
        Execute(
            """
            const stream = new WritableStream({
                write(chunk, controller) {
                    return new Promise(resolve => {
                        // Slow write to keep chunks in queue
                        setTimeout(resolve, 100);
                    });
                }
            }, {
                highWaterMark: 2,
                size() { return 1; }
            });

            const writer = stream.getWriter();
            writer.write('chunk1');
            writer.write('chunk2');
            writer.write('chunk3'); // This should exceed capacity
            """
        );

        Assert.True(Evaluate("writer.desiredSize < 0").AsBoolean());
    }

    [Fact]
    public void ShouldReturnZeroDesiredSizeWhenClosed()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Evaluate("writer.close()").UnwrapIfPromise();

        Assert.Equal(0, Evaluate("writer.desiredSize"));
    }

    [Fact]
    public void ShouldReturnNullDesiredSizeWhenErrored()
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

        Assert.True(Evaluate("writer.desiredSize === null").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenAccessingDesiredSizeAfterReleaseLock()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            """
        );

        Assert.Throws<JavaScriptException>(() => Evaluate("writer.desiredSize"));
    }

    [Fact]
    public void ShouldHandleDesiredSizeWithZeroHighWaterMark()
    {
        Execute(
            """
            const stream = new WritableStream({}, {
                highWaterMark: 0,
                size() { return 1; }
            });
            const writer = stream.getWriter();
            """
        );

        Assert.Equal(0, Evaluate("writer.desiredSize").AsNumber());
    }

    [Fact]
    public void ShouldHandleDesiredSizeWithLargeChunks()
    {
        Execute(
            """
            const stream = new WritableStream({
                write(chunk, controller) {
                    return Promise.resolve();
                }
            }, {
                highWaterMark: 5,
                size(chunk) {
                    return chunk.size || 1;
                }
            });

            const writer = stream.getWriter();
            const initialSize = writer.desiredSize;
            writer.write({ size: 10 }); // Large chunk
            const afterWriteSize = writer.desiredSize;
            """
        );

        Assert.Equal(5, Evaluate("initialSize").AsNumber());
        Assert.Equal(-5, Evaluate("afterWriteSize").AsNumber());
    }
}
