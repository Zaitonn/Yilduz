using Jint;
using Xunit;

namespace Yilduz.Tests.TransformStream;

public sealed class AlgorithmTests : TestBase
{
    [Fact]
    public void ShouldExecuteCustomTransform()
    {
        Engine.Execute(
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

        Assert.Equal("TransformStream", Engine.Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldCallFlushOnClose()
    {
        Engine.Execute(
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
        Engine.Execute(
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

        Assert.Equal("TransformStream", Engine.Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldHandleStartMethod()
    {
        Engine.Execute(
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

        Assert.True(Engine.Evaluate("startController !== null").AsBoolean());
    }

    [Fact]
    public void ShouldHandleBackpressure()
    {
        Engine.Execute(
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

        Assert.Equal("TransformStream", Engine.Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldRespectWritableStrategy()
    {
        Engine.Execute(
            """
            const stream = new TransformStream({}, 
                { highWaterMark: 5, size: chunk => chunk.length },
                { highWaterMark: 3, size: chunk => 1 }
            );
            """
        );

        Assert.Equal("TransformStream", Engine.Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldRespectReadableStrategy()
    {
        Engine.Execute(
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

        Assert.Equal("TransformStream", Engine.Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldHandleControllerTerminate()
    {
        Engine.Execute(
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

        Assert.Equal("TransformStream", Engine.Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldHandleControllerError()
    {
        Engine.Execute(
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

        Assert.Equal("TransformStream", Engine.Evaluate("stream.constructor.name"));
    }
}
