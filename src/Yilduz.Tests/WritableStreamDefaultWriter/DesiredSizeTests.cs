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
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("'desiredSize' in writer").AsBoolean());
    }

    [Fact]
    public void ShouldReturnCorrectDesiredSizeForEmptyStream()
    {
        Engine.Execute(
            """
            const stream = new WritableStream({}, {
                highWaterMark: 5,
                size() { return 1; }
            });
            const writer = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("typeof writer.desiredSize === 'number'").AsBoolean());
        Assert.Equal(5, Engine.Evaluate("writer.desiredSize").AsNumber());
    }

    [Fact]
    public void ShouldUpdateDesiredSizeAfterWrite()
    {
        Engine.Execute(
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

        Assert.Equal(3, Engine.Evaluate("initialSize").AsNumber());
        Assert.Equal(2, Engine.Evaluate("afterWriteSize").AsNumber());
    }

    [Fact]
    public void ShouldHandleDesiredSizeWithCustomSizeFunction()
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
            const initialSize = writer.desiredSize;
            writer.write('hello'); // size = 5
            const afterWriteSize = writer.desiredSize;
            """
        );

        Assert.Equal(10, Engine.Evaluate("initialSize").AsNumber());
        Assert.Equal(5, Engine.Evaluate("afterWriteSize").AsNumber());
    }

    [Fact]
    public void ShouldReturnNegativeDesiredSizeWhenOverCapacity()
    {
        Engine.Execute(
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

        Assert.True(Engine.Evaluate("writer.desiredSize < 0").AsBoolean());
    }

    [Fact]
    public void ShouldReturnZeroDesiredSizeWhenClosed()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.close();
            """
        );

        Assert.Equal(0, Engine.Evaluate("writer.desiredSize"));
    }

    [Fact]
    public void ShouldReturnNullDesiredSizeWhenErrored()
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

        Assert.True(Engine.Evaluate("writer.desiredSize === null").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenAccessingDesiredSizeAfterReleaseLock()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            """
        );

        Assert.Throws<JavaScriptException>(() => Engine.Evaluate("writer.desiredSize"));
    }

    [Fact]
    public void ShouldHandleDesiredSizeWithZeroHighWaterMark()
    {
        Engine.Execute(
            """
            const stream = new WritableStream({}, {
                highWaterMark: 0,
                size() { return 1; }
            });
            const writer = stream.getWriter();
            """
        );

        Assert.Equal(0, Engine.Evaluate("writer.desiredSize").AsNumber());
    }

    [Fact]
    public void ShouldHandleDesiredSizeWithLargeChunks()
    {
        Engine.Execute(
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

        Assert.Equal(5, Engine.Evaluate("initialSize").AsNumber());
        Assert.Equal(-5, Engine.Evaluate("afterWriteSize").AsNumber());
    }
}
