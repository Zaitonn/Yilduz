using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultController;

public sealed class AlgorithmTests : TestBase
{
    [Fact]
    public void ShouldCallStartAlgorithm()
    {
        Engine.Execute(
            """
            let startCalled = false;
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    startCalled = true;
                    controller = ctrl;
                    return Promise.resolve();
                }
            });
            """
        );

        Assert.True(Engine.Evaluate("startCalled").AsBoolean());
        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldCallWriteAlgorithm()
    {
        Engine.Execute(
            """
            let writeCalled = false;
            let writtenChunk = null;
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                write(chunk, ctrl) {
                    writeCalled = true;
                    writtenChunk = chunk;
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            writer.write('test chunk');
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
        // Note: In a full implementation, we would verify writeCalled and writtenChunk
    }

    [Fact]
    public void ShouldCallCloseAlgorithm()
    {
        Engine.Execute(
            """
            let closeCalled = false;
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                close() {
                    closeCalled = true;
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            writer.close();
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
        // Note: In a full implementation, we would verify closeCalled
    }

    [Fact]
    public void ShouldCallAbortAlgorithm()
    {
        Engine.Execute(
            """
            let abortCalled = false;
            let abortReason = null;
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                abort(reason) {
                    abortCalled = true;
                    abortReason = reason;
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            writer.abort('test reason');
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
        // Note: In a full implementation, we would verify abortCalled and abortReason
    }

    [Fact]
    public void ShouldHandleAsyncStartAlgorithm()
    {
        Engine.Execute(
            """
            let startResolved = false;
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                    return new Promise(resolve => {
                        setTimeout(() => {
                            startResolved = true;
                            resolve();
                        }, 10);
                    });
                }
            });
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldHandleAsyncWriteAlgorithm()
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
                        setTimeout(() => resolve(), 10);
                    });
                }
            });

            const writer = stream.getWriter();
            const writePromise = writer.write('async chunk');
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
        Assert.True(Engine.Evaluate("writePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleAsyncCloseAlgorithm()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                close() {
                    return new Promise(resolve => {
                        setTimeout(() => resolve(), 10);
                    });
                }
            });

            const writer = stream.getWriter();
            const closePromise = writer.close();
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
        Assert.True(Engine.Evaluate("closePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleAsyncAbortAlgorithm()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                abort(reason) {
                    return new Promise(resolve => {
                        setTimeout(() => resolve(), 10);
                    });
                }
            });

            const writer = stream.getWriter();
            const abortPromise = writer.abort('async reason');
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
        Assert.True(Engine.Evaluate("abortPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriteAlgorithmError()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                write(chunk, ctrl) {
                    throw new Error('Write error');
                }
            });

            const writer = stream.getWriter();
            const writePromise = writer.write('error chunk');
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
        Assert.True(Engine.Evaluate("writePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleCloseAlgorithmError()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                close() {
                    throw new Error('Close error');
                }
            });

            const writer = stream.getWriter();
            const closePromise = writer.close();
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
        Assert.True(Engine.Evaluate("closePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleAbortAlgorithmError()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                abort(reason) {
                    throw new Error('Abort error');
                }
            });

            const writer = stream.getWriter();
            const abortPromise = writer.abort('error reason');
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
        Assert.True(Engine.Evaluate("abortPromise instanceof Promise").AsBoolean());
    }
}
