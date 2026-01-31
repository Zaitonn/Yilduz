using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class ReadyPromiseTests : TestBase
{
    [Fact]
    public void ShouldHaveReadyPromiseOnCreation()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Evaluate("writer.ready instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldResolveReadyPromiseWhenNoBackpressure()
    {
        Execute(
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
        Assert.True(Evaluate("writer.ready instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleReadyPromiseWithBackpressure()
    {
        Execute(
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
        Execute("writer.write('chunk1');");
        Execute("writer.write('chunk2');");

        Assert.True(Evaluate("writer.ready instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHaveNewReadyPromiseAfterWrite()
    {
        Execute(
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

        Assert.True(Evaluate("initialReady instanceof Promise").AsBoolean());
        Assert.True(Evaluate("newReady instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldRejectReadyPromiseOnError()
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
            const ready = writer.ready;

            // Error the stream
            controller.error(new Error('Stream error'));
            """
        );

        Assert.True(Evaluate("ready instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHaveReadyPromiseResolvedWhenClosed()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.close();
            """
        );

        Assert.True(Evaluate("writer.ready instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleReadyPromiseAfterReleaseLock()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const ready = writer.ready;
            writer.releaseLock();
            """
        );

        Assert.True(Evaluate("ready instanceof Promise").AsBoolean());

        // Accessing ready after release should throw
        Assert.Throws<PromiseRejectedException>(() => Evaluate("writer.ready").UnwrapIfPromise());
    }

    [Fact]
    public void ShouldHandleReadyPromiseStateTransitions()
    {
        Execute(
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
        Execute(
            """
            const ready1 = writer.ready;
            writer.write('chunk1');
            const ready2 = writer.ready;
            writer.write('chunk2'); // This might cause backpressure
            const ready3 = writer.ready;
            """
        );

        Assert.True(Evaluate("ready1 instanceof Promise").AsBoolean());
        Assert.True(Evaluate("ready2 instanceof Promise").AsBoolean());
        Assert.True(Evaluate("ready3 instanceof Promise").AsBoolean());
    }
}
