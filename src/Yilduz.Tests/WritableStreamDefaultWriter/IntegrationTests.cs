using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class IntegrationTests : TestBase
{
    [Fact]
    public void ShouldWorkWithBasicWriteAndCloseFlow()
    {
        Execute(
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

        Assert.Equal(2, Evaluate("writtenChunks.length").AsNumber());
        Assert.Equal("chunk1", Evaluate("writtenChunks[0]").AsString());
        Assert.Equal("chunk2", Evaluate("writtenChunks[1]").AsString());
        Assert.True(Evaluate("closeCalled").AsBoolean());
    }

    [Fact]
    public void ShouldWorkWithAsyncWriteOperations()
    {
        Execute(
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
            """
        );

        Engine
            .Evaluate(
                """
                Promise.all(writePromises).then(() => {
                    allWritesComplete = true;
                });
                """
            )
            .UnwrapIfPromise();

        Assert.True(Evaluate("allWritesComplete").AsBoolean());
        Assert.Equal(2, Evaluate("writtenChunks.length").AsNumber());
    }

    [Fact]
    public async Task ShouldHandleErrorsDuringWrite()
    {
        Execute(
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

        Assert.Equal("Write failed", Evaluate("writeError"));
    }

    [Fact]
    public void ShouldMaintainWriteOrder()
    {
        Execute(
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
            """
        );

        Engine
            .Evaluate(
                """
                async function writeChunks() {
                    await writer.write('chunk1');
                    await writer.write('chunk2');
                    await writer.write('chunk3');
                    await writer.write('chunk4');
                    await writer.write('chunk5');
                }

                writeChunks();
                """
            )
            .UnwrapIfPromise();

        // Chunks should be written in order despite random delays
        Assert.Equal(5, Evaluate("writtenChunks.length").AsNumber());
        Assert.Equal(
            ("chunk1", "chunk2", "chunk3", "chunk4", "chunk5"),
            (
                Evaluate("writtenChunks[0]").AsString(),
                Evaluate("writtenChunks[1]").AsString(),
                Evaluate("writtenChunks[2]").AsString(),
                Evaluate("writtenChunks[3]").AsString(),
                Evaluate("writtenChunks[4]").AsString()
            )
        );
    }

    [Fact]
    public void ShouldWorkWithTransformStream()
    {
        Execute(
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

            const p1 = writer.write('hello');
            const p2 = writer.write('world');
            """
        );

        Evaluate("Promise.all([p1, p2, writer.close()])").UnwrapIfPromise();

        Assert.Equal(2, Evaluate("transformedChunks.length").AsNumber());
        Assert.Equal("HELLO", Evaluate("transformedChunks[0]").AsString());
        Assert.Equal("WORLD", Evaluate("transformedChunks[1]").AsString());
    }

    [Fact]
    public async Task ShouldHandleBackpressureWithMultipleWriters()
    {
        Execute(
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
        Assert.False(Evaluate("writer2ReadyResolved").AsBoolean());

        Execute(
            """
            // Resolve the slow write to relieve backpressure
            if (slowWriteResolve) {
                slowWriteResolve();
            }
            """
        );

        await Task.Delay(100);

        // Now writer2's ready promise should resolve
        Assert.True(Evaluate("writer2ReadyResolved").AsBoolean());
    }
}
