using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class EdgeCaseTests : TestBase
{
    [Fact]
    public void ShouldHandleWriteWithNullUndefinedChunks()
    {
        Execute(
            """
            let receivedChunks = [];
            const stream = new WritableStream({
                write(chunk, controller) {
                    receivedChunks.push(chunk);
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            """
        );

        // Write null and undefined chunks
        Execute(
            """
            const nullPromise = writer.write(null);
            const undefinedPromise = writer.write(undefined);
            const explicitUndefinedPromise = writer.write();
            """
        );

        Assert.True(Evaluate("nullPromise instanceof Promise").AsBoolean());
        Assert.True(Evaluate("undefinedPromise instanceof Promise").AsBoolean());
        Assert.True(Evaluate("explicitUndefinedPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleDetachedWriter()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.releaseLock();
            """
        );

        // Method calls on detached writer should return rejected promises
        Execute("const writePromise = writer.write('test');");
        Execute("const closePromise = writer.close();");
        Execute("const abortPromise = writer.abort();");

        Assert.True(Evaluate("writePromise instanceof Promise").AsBoolean());
        Assert.True(Evaluate("closePromise instanceof Promise").AsBoolean());
        Assert.True(Evaluate("abortPromise instanceof Promise").AsBoolean());

        // Property access on detached writer should throw
        Assert.Throws<PromiseRejectedException>(() => Evaluate("writer.ready").UnwrapIfPromise());
        Assert.Throws<PromiseRejectedException>(() => Evaluate("writer.closed").UnwrapIfPromise());
        Assert.Throws<JavaScriptException>(() => Evaluate("writer.desiredSize"));
    }

    [Fact]
    public void ShouldHandleVeryLargeChunks()
    {
        Execute(
            """
            const stream = new WritableStream({
                write(chunk, controller) {
                    return Promise.resolve();
                }
            }, {
                highWaterMark: 100,
                size(chunk) {
                    return chunk.size || 1;
                }
            });

            const writer = stream.getWriter();
            """
        );

        // Write very large chunk
        Execute(
            """
            const largeChunk = { size: 1000000 };
            const largePromise = writer.write(largeChunk);
            """
        );

        Assert.True(Evaluate("largePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleZeroAndNegativeSizes()
    {
        Execute(
            """
            const stream = new WritableStream({
                write(chunk, controller) {
                    return Promise.resolve();
                }
            }, {
                highWaterMark: 5,
                size(chunk) {
                    return chunk.size;
                }
            });

            const writer = stream.getWriter();
            """
        );

        // Write chunks with zero and negative sizes
        Execute(
            """
            const zeroPromise = writer.write({ size: 0 });
            const negativePromise = writer.write({ size: -5 });
            """
        );

        Assert.True(Evaluate("zeroPromise instanceof Promise").AsBoolean());
        Assert.True(Evaluate("negativePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleNaNAndInfiniteSizes()
    {
        Execute(
            """
            const stream = new WritableStream({
                write(chunk, controller) {
                    return Promise.resolve();
                }
            }, {
                highWaterMark: 5,
                size(chunk) {
                    return chunk.size;
                }
            });

            const writer = stream.getWriter();
            """
        );

        // Write chunks with NaN and infinite sizes
        Execute(
            """
            const nanPromise = writer.write({ size: NaN });
            const infinitePromise = writer.write({ size: Infinity });
            const negInfinitePromise = writer.write({ size: -Infinity });
            """
        );

        Assert.True(Evaluate("nanPromise instanceof Promise").AsBoolean());
        Assert.True(Evaluate("infinitePromise instanceof Promise").AsBoolean());
        Assert.True(Evaluate("negInfinitePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleCircularChunks()
    {
        Execute(
            """
            const stream = new WritableStream({
                write(chunk, controller) {
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            """
        );

        // Create circular reference
        Execute(
            """
            const circularChunk = { data: 'test' };
            circularChunk.self = circularChunk;
            const circularPromise = writer.write(circularChunk);
            """
        );

        Assert.True(Evaluate("circularPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleWriteAfterMultipleReleaseAndReacquire()
    {
        Execute(
            """
            const stream = new WritableStream();

            const writer1 = stream.getWriter();
            writer1.releaseLock();

            const writer2 = stream.getWriter();
            writer2.releaseLock();

            const writer3 = stream.getWriter();
            writer3.releaseLock();

            const finalWriter = stream.getWriter();
            """
        );

        Execute("const writePromise = finalWriter.write('test');");
        Assert.True(Evaluate("writePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandlePromiseChainInWrite()
    {
        Execute(
            """
            let resolveWrite;
            const stream = new WritableStream({
                write(chunk, controller) {
                    return new Promise(resolve => {
                        resolveWrite = resolve;
                        resolve();
                    });
                }
            });

            const writer = stream.getWriter();
            const writePromise = writer.write('test');
            """
        );

        Assert.True(Evaluate("writePromise instanceof Promise").AsBoolean());
        Assert.True(Evaluate("typeof resolveWrite === 'function'").AsBoolean());
    }

    [Fact]
    public void ShouldHandleExceptionInSizeFunction()
    {
        Execute(
            """
            const stream = new WritableStream({
                write(chunk, controller) {
                    return Promise.resolve();
                }
            }, {
                highWaterMark: 5,
                size(chunk) {
                    if (chunk.throwError) {
                        throw new Error('Size calculation error');
                    }
                    return 1;
                }
            });

            const writer = stream.getWriter();
            """
        );

        // Write chunk that causes size function to throw
        Execute("const errorPromise = writer.write({ throwError: true });");
        Assert.True(Evaluate("errorPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHandleSymbolChunks()
    {
        Execute(
            """
            let receivedChunks = [];
            const stream = new WritableStream({
                write(chunk, controller) {
                    receivedChunks.push(chunk);
                    return Promise.resolve();
                }
            });

            const writer = stream.getWriter();
            """
        );

        // Write symbol chunks
        Execute(
            """
            const symbol1 = Symbol('test1');
            const symbol2 = Symbol.for('test2');
            const symbolPromise1 = writer.write(symbol1);
            const symbolPromise2 = writer.write(symbol2);
            """
        );

        Assert.True(Evaluate("symbolPromise1 instanceof Promise").AsBoolean());
        Assert.True(Evaluate("symbolPromise2 instanceof Promise").AsBoolean());
    }
}
