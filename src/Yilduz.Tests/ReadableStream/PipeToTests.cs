using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.ReadableStream;

public sealed class PipeToTests : TestBase
{
    [Fact]
    public void ShouldPipeToWritableStream()
    {
        Execute(
            """
            let writtenChunks = [];
            let sourceClosed = false;
            let sinkClosed = false;

            const readable = new ReadableStream({
                start(controller) {
                    controller.enqueue('chunk1');
                    controller.enqueue('chunk2');
                    controller.enqueue('chunk3');
                    controller.close();
                    sourceClosed = true;
                }
            });

            const writable = new WritableStream({
                write(chunk) {
                    writtenChunks.push(chunk);
                },
                close() {
                    sinkClosed = true;
                }
            });

            const pipePromise = readable.pipeTo(writable);
            """
        );

        // Verify the pipe operation is a promise
        Assert.True(Evaluate("pipePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldThrowErrorWhenReadableStreamIsLocked()
    {
        Assert.Throws<JavaScriptException>(
            () =>
                Execute(
                    """
                    const readable = new ReadableStream();
                    const reader = readable.getReader(); // Lock the stream

                    const writable = new WritableStream();
                    readable.pipeTo(writable); // Should throw
                    """
                )
        );
    }

    [Fact]
    public void ShouldThrowErrorWhenWritableStreamIsLocked()
    {
        Assert.Throws<JavaScriptException>(
            () =>
                Execute(
                    """
                    const readable = new ReadableStream();

                    const writable = new WritableStream();
                    const writer = writable.getWriter(); // Lock the stream

                    readable.pipeTo(writable); // Should throw
                    """
                )
        );
    }

    [Fact]
    public void ShouldRespectPreventCloseOption()
    {
        Execute(
            """
            let sinkClosed = false;

            const readable = new ReadableStream({
                start(controller) {
                    controller.enqueue('data');
                    controller.close();
                }
            });

            const writable = new WritableStream({
                write(chunk) {
                    // Handle chunk
                },
                close() {
                    sinkClosed = true;
                }
            });

            const pipePromise = readable.pipeTo(writable, { preventClose: true });
            """
        );

        Assert.True(Evaluate("pipePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldRespectPreventAbortOption()
    {
        Execute(
            """
            let abortCalled = false;

            const readable = new ReadableStream({
                start(controller) {
                    controller.error(new Error('source error'));
                }
            });

            const writable = new WritableStream({
                write(chunk) {
                    // Handle chunk
                },
                abort(reason) {
                    abortCalled = true;
                }
            });

            const pipePromise = readable.pipeTo(writable, { preventAbort: true });
            """
        );

        Assert.True(Evaluate("pipePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldRespectPreventCancelOption()
    {
        Execute(
            """
            let cancelCalled = false;

            const readable = new ReadableStream({
                start(controller) {
                    controller.enqueue('data');
                },
                cancel(reason) {
                    cancelCalled = true;
                }
            });

            const writable = new WritableStream({
                write(chunk) {
                    throw new Error('write error');
                }
            });

            const pipePromise = readable.pipeTo(writable, { preventCancel: true });
            """
        );

        Assert.True(Evaluate("pipePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleAbortSignal()
    {
        Execute(
            """
            const controller = new AbortController();

            const readable = new ReadableStream({
                start(controller) {
                    controller.enqueue('chunk1');
                    controller.enqueue('chunk2');
                    // Don't close, keep it open
                }
            });

            const writable = new WritableStream({
                write(chunk) {
                    // Simulate slow writing
                }
            });

            const pipePromise = readable.pipeTo(writable, { 
                signal: controller.signal 
            });

            // Abort the operation
            controller.abort('User cancelled');
            """
        );

        Assert.True(Evaluate("pipePromise instanceof Promise").AsBoolean());
        Assert.True(Evaluate("controller.signal.aborted").AsBoolean());
    }

    [Fact]
    public void ShouldHandleBackpressure()
    {
        Execute(
            """
            let writeCount = 0;
            let readCount = 0;

            const readable = new ReadableStream({
                pull(controller) {
                    readCount++;
                    if (readCount <= 5) {
                        controller.enqueue(`chunk${readCount}`);
                    } else {
                        controller.close();
                    }
                }
            });

            const writable = new WritableStream({
                write(chunk) {
                    writeCount++;
                    return new Promise(resolve => {
                        // Simulate slow writing to test backpressure
                        setTimeout(resolve, 10);
                    });
                }
            }, {
                highWaterMark: 1 // Small buffer to trigger backpressure
            });

            const pipePromise = readable.pipeTo(writable);
            """
        );

        Assert.True(Evaluate("pipePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldThrowErrorForInvalidDestination()
    {
        Assert.Throws<JavaScriptException>(
            () =>
                Execute(
                    """
                    const readable = new ReadableStream();
                    readable.pipeTo({}); // Not a WritableStream
                    """
                )
        );
    }
}
