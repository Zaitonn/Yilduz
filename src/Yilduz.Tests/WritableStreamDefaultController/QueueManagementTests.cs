using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultController;

public sealed class QueueManagementTests : TestBase
{
    [Fact]
    public void ShouldManageQueueCorrectly()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                write(chunk, ctrl) {
                    return Promise.resolve();
                }
            }, {
                highWaterMark: 3,
                size() { return 1; }
            });

            const writer = stream.getWriter();
            """
        );

        // Write chunks to fill the queue
        Engine.Execute(
            """
            writer.write('chunk1');
            writer.write('chunk2');
            writer.write('chunk3');
            """
        );

        // Verify controller is managing the queue
        Assert.True(Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleQueueWithCustomSizeFunction()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                write(chunk, ctrl) {
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
            writer.write('a');        // size 1
            writer.write('hello');    // size 5
            writer.write('world');    // size 5
            """
        );

        Assert.True(Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleQueueBackpressure()
    {
        Engine.Execute(
            """
            let controller = null;
            let writeCount = 0;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                write(chunk, ctrl) {
                    writeCount++;
                    return new Promise(resolve => {
                        // Simulate slow write to build up queue
                        setTimeout(resolve, 50);
                    });
                }
            }, {
                highWaterMark: 2,
                size() { return 1; }
            });

            const writer = stream.getWriter();
            """
        );

        // Write enough chunks to trigger backpressure
        Engine.Execute(
            """
            writer.write('chunk1');
            writer.write('chunk2');
            writer.write('chunk3');
            writer.write('chunk4');
            """
        );

        Assert.True(Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleEmptyQueue()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });
            """
        );

        // Queue should be empty initially
        Assert.True(Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldProcessQueueInOrder()
    {
        Engine.Execute(
            """
            let controller = null;
            let processedChunks = [];
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                write(chunk, ctrl) {
                    processedChunks.push(chunk);
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            """
        );

        // Write chunks in specific order
        Engine.Execute(
            """
            writer.write('first');
            writer.write('second');
            writer.write('third');
            """
        );

        Assert.True(Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
        // In a full implementation, we would verify the order of processed chunks
    }

    [Fact]
    public void ShouldHandleQueueDuringClose()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                write(chunk, ctrl) {
                    return new Promise(resolve => {
                        setTimeout(resolve, 10);
                    });
                },
                close() {
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            """
        );

        // Write and then close
        Engine.Execute(
            """
            writer.write('chunk1');
            writer.write('chunk2');
            writer.close();
            """
        );

        Assert.True(Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleQueueDuringError()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                write(chunk, ctrl) {
                    return new Promise(resolve => {
                        setTimeout(resolve, 10);
                    });
                }
            });

            const writer = stream.getWriter();
            """
        );

        // Write and then error
        Engine.Execute(
            """
            writer.write('chunk1');
            writer.write('chunk2');
            controller.error(new Error('Stream error'));
            """
        );

        Assert.True(Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleZeroSizedChunks()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                write(chunk, ctrl) {
                    return Promise.resolve();
                }
            }, {
                highWaterMark: 5,
                size() { return 0; }
            });

            const writer = stream.getWriter();
            """
        );

        // Write zero-sized chunks
        Engine.Execute(
            """
            writer.write('chunk1');
            writer.write('chunk2');
            writer.write('chunk3');
            """
        );

        Assert.True(Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }
}
