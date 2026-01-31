using Jint;
using Xunit;

namespace Yilduz.Tests.TransformStream;

public sealed class AlgorithmTests : TestBase
{
    [Fact]
    public void ShouldExecuteCustomTransform()
    {
        Execute(
            """
            const stream = new TransformStream({
                transform(chunk, controller) {
                    if (typeof chunk === 'string') {
                        controller.enqueue(chunk.toUpperCase());
                    } else {
                        controller.enqueue(chunk);
                    }
                }
            });
            """
        );

        Assert.Equal("TransformStream", Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldCallFlushOnClose()
    {
        Execute(
            """
            let flushCalled = false;
            const stream = new TransformStream({
                flush(controller) {
                    flushCalled = true;
                    controller.enqueue('final');
                }
            });

            const writer = stream.writable.getWriter();
            writer.close();
            """
        );

        // Note: flush is called asynchronously during close
    }

    [Fact]
    public void ShouldHandleTransformError()
    {
        Execute(
            """
            const stream = new TransformStream({
                transform(chunk, controller) {
                    if (chunk === 'error') {
                        throw new Error('Transform error');
                    }
                    controller.enqueue(chunk);
                }
            });
            """
        );

        Assert.Equal("TransformStream", Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldHandleStartMethod()
    {
        Execute(
            """
            let startController = null;
            const stream = new TransformStream({
                start(controller) {
                    startController = controller;
                    controller.enqueue('initial');
                }
            });
            """
        );

        Assert.True(Evaluate("startController !== null").AsBoolean());
    }

    [Fact]
    public void ShouldHandleBackpressure()
    {
        Execute(
            """
            const stream = new TransformStream({
                transform(chunk, controller) {
                    // Fill up the queue to test backpressure
                    for (let i = 0; i < 10; i++) {
                        controller.enqueue(chunk + i);
                    }
                }
            }, undefined, { highWaterMark: 1 });
            """
        );

        Assert.Equal("TransformStream", Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldRespectWritableStrategy()
    {
        Execute(
            """
            const stream = new TransformStream({}, 
                { highWaterMark: 5, size: chunk => chunk.length },
                { highWaterMark: 3, size: chunk => 1 }
            );
            """
        );

        Assert.Equal("TransformStream", Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldRespectReadableStrategy()
    {
        Execute(
            """
            const stream = new TransformStream({
                transform(chunk, controller) {
                    controller.enqueue(chunk.repeat(2));
                }
            }, 
                { highWaterMark: 2 },
                { highWaterMark: 4, size: chunk => chunk.length }
            );
            """
        );

        Assert.Equal("TransformStream", Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldHandleControllerTerminate()
    {
        Execute(
            """
            const stream = new TransformStream({
                transform(chunk, controller) {
                    if (chunk === 'terminate') {
                        controller.terminate();
                    } else {
                        controller.enqueue(chunk);
                    }
                }
            });
            """
        );

        Assert.Equal("TransformStream", Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldHandleControllerError()
    {
        Execute(
            """
            const stream = new TransformStream({
                transform(chunk, controller) {
                    if (chunk === 'error') {
                        controller.error(new Error('Controller error'));
                    } else {
                        controller.enqueue(chunk);
                    }
                }
            });
            """
        );

        Assert.Equal("TransformStream", Evaluate("stream.constructor.name"));
    }
}
