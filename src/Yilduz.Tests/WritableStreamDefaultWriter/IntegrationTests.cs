using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class IntegrationTests : TestBase
{
    [Fact]
    public void ShouldWorkWithBasicWriteAndCloseFlow()
    {
        Engine.Execute(
            """
            const writtenChunks = [];
            let closeCalled = false;

            const stream = new WritableStream({
                write(chunk, controller) {
                    writtenChunks.push(chunk);
                },
                close() {
                    closeCalled = true;
                }
            });

            const writer = stream.getWriter();
            writer.write('chunk1');
            writer.write('chunk2');
            writer.close();
            """
        );

        Assert.Equal(2, Engine.Evaluate("writtenChunks.length").AsNumber());
        Assert.Equal("chunk1", Engine.Evaluate("writtenChunks[0]").AsString());
        Assert.Equal("chunk2", Engine.Evaluate("writtenChunks[1]").AsString());
        Assert.True(Engine.Evaluate("closeCalled").AsBoolean());
    }

    [Fact]
    public async Task ShouldWorkWithAsyncWriteOperations()
    {
        Engine.Execute(
            """
            const writtenChunks = [];
            const writePromises = [];
            let allWritesComplete = false;

            const stream = new WritableStream({
                write(chunk, controller) {
                    return new Promise(resolve => {
                        setTimeout(() => {
                            writtenChunks.push(chunk);
                            resolve();
                        }, 10);
                    });
                }
            });

            const writer = stream.getWriter();
            writePromises.push(writer.write('chunk1'));
            writePromises.push(writer.write('chunk2'));

            Promise.all(writePromises).then(() => {
                allWritesComplete = true;
            });
            """
        );

        // Wait for async operations to complete
        await Task.Delay(500);

        Assert.True(Engine.Evaluate("allWritesComplete").AsBoolean());
        Assert.Equal(2, Engine.Evaluate("writtenChunks.length").AsNumber());
    }

    [Fact]
    public async Task ShouldHandleErrorsDuringWrite()
    {
        Engine.Execute(
            """
            let writeError = undefined;

            const stream = new WritableStream({
                write(chunk, controller) {
                    if (chunk === 'error') {
                        throw new Error('Write failed');
                    }
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            // writer.write('good');
            writer.write('error').catch(e => {
                writeError = e.message;
            });
            """
        );

        await Task.Delay(500);

        Assert.Equal("Write failed", Engine.Evaluate("writeError"));
    }

    [Fact]
    public async Task ShouldMaintainWriteOrder()
    {
        Engine.Execute(
            """
            const writtenChunks = [];

            const stream = new WritableStream({
                write(chunk, controller) {
                    return new Promise(resolve => {
                        // Simulate random delay
                        const delay = Math.random() * 10;
                        setTimeout(() => {
                            writtenChunks.push(chunk);
                            resolve();
                        }, delay);
                    });
                }
            });

            const writer = stream.getWriter();
            writer.write('chunk1');
            writer.write('chunk2');
            writer.write('chunk3');
            writer.write('chunk4');
            writer.write('chunk5');
            """
        );

        // Wait for all writes to complete
        await Task.Delay(1000);

        // Chunks should be written in order despite random delays
        Assert.Equal(5, Engine.Evaluate("writtenChunks.length").AsNumber());
        Assert.Equal(
            ("chunk1", "chunk2", "chunk3", "chunk4", "chunk5"),
            (
                Engine.Evaluate("writtenChunks[0]").AsString(),
                Engine.Evaluate("writtenChunks[1]").AsString(),
                Engine.Evaluate("writtenChunks[2]").AsString(),
                Engine.Evaluate("writtenChunks[3]").AsString(),
                Engine.Evaluate("writtenChunks[4]").AsString()
            )
        );
    }

    [Fact]
    public void ShouldWorkWithTransformStream()
    {
        Engine.Execute(
            """
            const transformedChunks = [];

            const transform = new TransformStream({
                transform(chunk, controller) {
                    controller.enqueue(chunk.toUpperCase());
                }
            });

            const writableStream = new WritableStream({
                write(chunk) {
                    transformedChunks.push(chunk);
                }
            });

            const writer = transform.writable.getWriter();
            transform.readable.pipeTo(writableStream);

            writer.write('hello');
            writer.write('world');
            writer.close();
            """
        );

        // Wait for pipeline to complete
        System.Threading.Thread.Sleep(50);

        Assert.Equal(2, Engine.Evaluate("transformedChunks.length").AsNumber());
        Assert.Equal("HELLO", Engine.Evaluate("transformedChunks[0]").AsString());
        Assert.Equal("WORLD", Engine.Evaluate("transformedChunks[1]").AsString());
    }

    [Fact]
    public async Task ShouldHandleBackpressureWithMultipleWriters()
    {
        Engine.Execute(
            """
            let slowWriteResolve = null;
            const stream = new WritableStream({
                write(chunk, controller) {
                    if (chunk === 'slow') {
                        return new Promise(resolve => {
                            slowWriteResolve = resolve;
                        });
                    }
                    return Promise.resolve();
                }
            }, {
                highWaterMark: 1
            });

            const writer1 = stream.getWriter();
            writer1.write('slow'); // This will block

            writer1.releaseLock();

            const writer2 = stream.getWriter();
            let writer2ReadyResolved = false;
            writer2.ready.then(() => {
                writer2ReadyResolved = true;
            });
            """
        );

        // Writer2's ready promise should be pending due to backpressure
        Assert.False(Engine.Evaluate("writer2ReadyResolved").AsBoolean());

        Engine.Execute(
            """
            // Resolve the slow write to relieve backpressure
            if (slowWriteResolve) {
                slowWriteResolve();
            }
            """
        );

        await Task.Delay(100);

        // Now writer2's ready promise should resolve
        Assert.True(Engine.Evaluate("writer2ReadyResolved").AsBoolean());
    }
}
