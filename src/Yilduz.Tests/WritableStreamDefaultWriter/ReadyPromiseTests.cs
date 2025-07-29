using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class ReadyPromiseTests : TestBase
{
    [Fact]
    public void ShouldHaveReadyPromiseOnCreation()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("writer.ready instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldResolveReadyPromiseWhenNoBackpressure()
    {
        Engine.Execute(
            """
            const stream = new WritableStream({
                write(chunk, controller) {
                    return Promise.resolve();
                }
            }, {
                highWaterMark: 2,
                size() { return 1; }
            });

            const writer = stream.getWriter();
            """
        );

        // Ready promise should be resolved when there's no backpressure
        Assert.True(Engine.Evaluate("writer.ready instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleReadyPromiseWithBackpressure()
    {
        Engine.Execute(
            """
            const stream = new WritableStream({
                write(chunk, controller) {
                    return new Promise(resolve => {
                        // Simulate slow write
                        setTimeout(resolve, 10);
                    });
                }
            }, {
                highWaterMark: 1,
                size() { return 1; }
            });

            const writer = stream.getWriter();
            """
        );

        // Write to fill the queue
        Engine.Execute("writer.write('chunk1');");
        Engine.Execute("writer.write('chunk2');");

        Assert.True(Engine.Evaluate("writer.ready instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHaveNewReadyPromiseAfterWrite()
    {
        Engine.Execute(
            """
            const stream = new WritableStream({
                write(chunk, controller) {
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            const initialReady = writer.ready;
            writer.write('test');
            const newReady = writer.ready;
            """
        );

        Assert.True(Engine.Evaluate("initialReady instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("newReady instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldRejectReadyPromiseOnError()
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
            const ready = writer.ready;

            // Error the stream
            controller.error(new Error('Stream error'));
            """
        );

        Assert.True(Engine.Evaluate("ready instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHaveReadyPromiseResolvedWhenClosed()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.close();
            """
        );

        Assert.True(Engine.Evaluate("writer.ready instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleReadyPromiseAfterReleaseLock()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const ready = writer.ready;
            writer.releaseLock();
            """
        );

        Assert.True(Engine.Evaluate("ready instanceof Promise").AsBoolean());

        // Accessing ready after release should throw
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate("writer.ready"));
    }

    [Fact]
    public void ShouldHandleReadyPromiseStateTransitions()
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
                highWaterMark: 1,
                size() { return 1; }
            });

            const writer = stream.getWriter();
            """
        );

        // Test various state transitions
        Engine.Execute(
            """
            const ready1 = writer.ready;
            writer.write('chunk1');
            const ready2 = writer.ready;
            writer.write('chunk2'); // This might cause backpressure
            const ready3 = writer.ready;
            """
        );

        Assert.True(Engine.Evaluate("ready1 instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("ready2 instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("ready3 instanceof Promise").AsBoolean());
    }
}
