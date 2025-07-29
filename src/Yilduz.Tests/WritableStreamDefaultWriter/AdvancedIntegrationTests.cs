using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class AdvancedIntegrationTests : TestBase
{
    [Fact]
    public void ShouldHandleWriterControllerInteraction()
    {
        Engine.Execute(
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

        Assert.True(Engine.Evaluate("writer instanceof WritableStreamDefaultWriter").AsBoolean());
        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldHandleMultipleWritersSequentially()
    {
        Engine.Execute(
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

        Assert.True(Engine.Evaluate("writer3 instanceof WritableStreamDefaultWriter").AsBoolean());
        Assert.True(Engine.Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriterErrorAndRecovery()
    {
        Engine.Execute(
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

        Assert.True(Engine.Evaluate("errorPromise instanceof Promise").AsBoolean());
        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldHandleBackpressureAndReady()
    {
        Engine.Execute(
            """
            let writeCount = 0;
            const stream = new WritableStream({
                write(chunk, controller) {
                    writeCount++;
                    return new Promise(resolve => {
                        setTimeout(resolve, 10);
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

        Assert.True(Engine.Evaluate("ready1 instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("ready2 instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("ready3 instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleStreamStateTransitions()
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
            const initialReady = writer.ready;
            const initialClosed = writer.closed;
            
            writer.write('test');
            controller.error(new Error('Test error'));
            
            const errorReady = writer.ready;
            const errorClosed = writer.closed;
            """
        );

        Assert.True(Engine.Evaluate("initialReady instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("initialClosed instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("errorReady instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("errorClosed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleComplexWriteSequence()
    {
        Engine.Execute(
            """
            let processedChunks = [];
            const stream = new WritableStream({
                write(chunk, controller) {
                    processedChunks.push(chunk);
                    if (chunk.delay) {
                        return new Promise(resolve => {
                            setTimeout(resolve, chunk.delay);
                        });
                    }
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            """
        );

        // Write chunks with different processing times
        Engine.Execute(
            """
            const promises = [
                writer.write({ data: 'fast', delay: 5 }),
                writer.write({ data: 'medium', delay: 15 }),
                writer.write({ data: 'slow', delay: 25 }),
                writer.write({ data: 'instant' })
            ];
            """
        );

        Assert.True(Engine.Evaluate("promises.every(p => p instanceof Promise)").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriterAbortAndControllerError()
    {
        Engine.Execute(
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

        Assert.True(Engine.Evaluate("abortPromise instanceof Promise").AsBoolean());
        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldHandleControllerSignalAbort()
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
            writer.write('chunk1');
            
            // Access signal property
            const signal = controller.signal;
            """
        );

        Assert.True(Engine.Evaluate("signal !== undefined").AsBoolean());
        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldHandleWriterDesiredSizeWithController()
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

        Assert.True(Engine.Evaluate("typeof initialSize === 'number'").AsBoolean());
        Assert.True(Engine.Evaluate("typeof afterLightSize === 'number'").AsBoolean());
        Assert.True(Engine.Evaluate("typeof afterHeavySize === 'number'").AsBoolean());
    }

    [Fact]
    public void ShouldHandleStreamCloseWithPendingWrites()
    {
        Engine.Execute(
            """
            let writeResolvers = [];
            const stream = new WritableStream({
                write(chunk, controller) {
                    return new Promise(resolve => {
                        writeResolvers.push(resolve);
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

        Assert.True(Engine.Evaluate("closePromise instanceof Promise").AsBoolean());
        Assert.True(Engine.Evaluate("writeResolvers.length === 2").AsBoolean());
    }
}
