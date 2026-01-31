using System.Threading.Tasks;
using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class AdvancedIntegrationTests : TestBase
{
    [Fact]
    public void ShouldHandleWriterControllerInteraction()
    {
        Execute(
            """
            let controller = null;
            let receivedChunks = [];
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                write(chunk, ctrl) {
                    receivedChunks.push(chunk);
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            """
        );

        Assert.True(Evaluate("writer instanceof WritableStreamDefaultWriter").AsBoolean());
        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleMultipleWritersSequentially()
    {
        Execute(
            """
            const stream = new WritableStream();

            const writer1 = stream.getWriter();
            writer1.write('chunk1');
            writer1.releaseLock();

            const writer2 = stream.getWriter();
            writer2.write('chunk2');
            writer2.releaseLock();

            const writer3 = stream.getWriter();
            """
        );

        Assert.True(Evaluate("writer3 instanceof WritableStreamDefaultWriter").AsBoolean());
        Assert.True(Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriterErrorAndRecovery()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                write(chunk, ctrl) {
                    if (chunk === 'error') {
                        throw new Error('Write error');
                    }
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            writer.write('normal');
            const errorPromise = writer.write('error');
            """
        );

        Assert.True(Evaluate("errorPromise instanceof Promise").AsBoolean());
        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public async Task ShouldHandleBackpressureAndReady()
    {
        Execute(
            """
            let writeCount = 0;
            let writeResolvers = [];
            const stream = new WritableStream({
                write(chunk, controller) {
                    writeCount++;
                    return new Promise(resolve => {
                        writeResolvers.push(resolve());
                    });
                }
            }, {
                highWaterMark: 1,
                size() { return 1; }
            });

            const writer = stream.getWriter();
            const ready1 = writer.ready;

            writer.write('chunk1');
            const ready2 = writer.ready;

            writer.write('chunk2');
            const ready3 = writer.ready;
            """
        );

        await Task.Delay(100);

        Assert.True(Evaluate("ready1 instanceof Promise").AsBoolean());
        Assert.True(Evaluate("ready2 instanceof Promise").AsBoolean());
        Assert.True(Evaluate("ready3 instanceof Promise").AsBoolean());
        Assert.Equal(2, Evaluate("writeCount").AsNumber());
        Assert.Equal(2, Evaluate("writeResolvers.length").AsNumber());
    }

    [Fact]
    public void ShouldHandleStreamStateTransitions()
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
            const initialReady = writer.ready;
            const initialClosed = writer.closed;

            writer.write('test');
            controller.error(new Error('Test error'));

            const errorReady = writer.ready;
            const errorClosed = writer.closed;
            """
        );

        Assert.True(Evaluate("initialReady instanceof Promise").AsBoolean());
        Assert.True(Evaluate("initialClosed instanceof Promise").AsBoolean());
        Assert.True(Evaluate("errorReady instanceof Promise").AsBoolean());
        Assert.True(Evaluate("errorClosed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriterAbortAndControllerError()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                abort(reason) {
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            writer.write('chunk1');
            writer.write('chunk2');

            const abortPromise = writer.abort('user abort');
            """
        );

        Assert.True(Evaluate("abortPromise instanceof Promise").AsBoolean());
        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleControllerSignalAbort()
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
            writer.write('chunk1');

            // Access signal property
            const signal = controller.signal;
            """
        );

        Assert.True(Evaluate("signal !== undefined").AsBoolean());
        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriterDesiredSizeWithController()
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
                highWaterMark: 3,
                size(chunk) {
                    return chunk.weight || 1;
                }
            });

            const writer = stream.getWriter();
            const initialSize = writer.desiredSize;

            writer.write({ data: 'light', weight: 1 });
            const afterLightSize = writer.desiredSize;

            writer.write({ data: 'heavy', weight: 2 });
            const afterHeavySize = writer.desiredSize;
            """
        );

        Assert.True(Evaluate("typeof initialSize === 'number'").AsBoolean());
        Assert.True(Evaluate("typeof afterLightSize === 'number'").AsBoolean());
        Assert.True(Evaluate("typeof afterHeavySize === 'number'").AsBoolean());
    }

    [Fact]
    public async Task ShouldHandleStreamCloseWithPendingWrites()
    {
        Execute(
            """
            let writeResolvers = [];
            const stream = new WritableStream({
                write(chunk, controller) {
                    return new Promise(resolve => {
                        writeResolvers.push(resolve());
                    });
                },
                close() {
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            writer.write('chunk1');
            writer.write('chunk2');

            const closePromise = writer.close();
            """
        );

        await Task.Delay(100);

        Assert.True(Evaluate("closePromise instanceof Promise").AsBoolean());
        Assert.Equal(2, Evaluate("writeResolvers.length"));
    }
}
