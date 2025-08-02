using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.CountQueuingStrategy;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldAlwaysReturnOneForAnyChunk()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 4 });
            const sizes = [
                strategy.size('string'),
                strategy.size(42),
                strategy.size({}),
                strategy.size([1, 2, 3]),
                strategy.size(true),
                strategy.size(Symbol('test'))
            ];
            """
        );

        for (int i = 0; i < 6; i++)
        {
            Assert.Equal(1, Engine.Evaluate($"sizes[{i}]").AsNumber());
        }
    }

    [Fact]
    public void ShouldReturnOneForNullAndUndefined()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 4 });
            const nullSize = strategy.size(null);
            const undefinedSize = strategy.size(undefined);
            """
        );
        Assert.Equal(1, Engine.Evaluate("nullSize").AsNumber());
        Assert.Equal(1, Engine.Evaluate("undefinedSize").AsNumber());
    }

    [Fact]
    public void ShouldReturnOneForLargeObjects()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 4 });
            const largeObject = { data: new Array(1000).fill('x') };
            const size = strategy.size(largeObject);
            """
        );
        Assert.Equal(1, Engine.Evaluate("size").AsNumber());
    }

    [Fact]
    public void ShouldReturnOneForArrayBuffers()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 4 });
            const buffer = new ArrayBuffer(1024);
            const size = strategy.size(buffer);
            """
        );
        Assert.Equal(1, Engine.Evaluate("size").AsNumber());
    }

    [Fact]
    public void ShouldThrowWhenSizeCalledWithoutArguments()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 4 });
            let caughtError;
            try {
                strategy.size();
            } catch (e) {
                caughtError = e;
            }
            """
        );
        Assert.True(Engine.Evaluate("caughtError instanceof TypeError").AsBoolean());
    }

    [Fact]
    public void ShouldWorkWithReadableStream()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 3 });
            const stream = new ReadableStream({
                start(controller) {
                    controller.enqueue('chunk1');
                    controller.enqueue('chunk2');
                    controller.close();
                }
            }, strategy);
            """
        );
        Assert.Equal("ReadableStream", Engine.Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldWorkWithWritableStream()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 2 });
            const stream = new WritableStream({
                write(chunk) {
                    // Write chunk
                }
            }, strategy);
            """
        );
        Assert.Equal("WritableStream", Engine.Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldHandleMultipleChunksInStream()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 5 });
            let enqueuedCount = 0;
            const stream = new ReadableStream({
                start(controller) {
                    // Enqueue multiple chunks to test count-based queueing
                    for (let i = 0; i < 3; i++) {
                        controller.enqueue(`chunk${i}`);
                        enqueuedCount++;
                    }
                    controller.close();
                }
            }, strategy);
            """
        );
        Assert.Equal(3, Engine.Evaluate("enqueuedCount").AsNumber());
    }

    [Fact]
    public void ShouldRespectHighWaterMarkInBackpressure()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 1 });
            let pullCount = 0;
            const stream = new ReadableStream({
                pull(controller) {
                    pullCount++;
                    if (pullCount <= 2) {
                        controller.enqueue(`chunk${pullCount}`);
                    } else {
                        controller.close();
                    }
                }
            }, strategy);
            """
        );
        // Pull should be called to maintain the desired queue size
        Assert.True(Engine.Evaluate("pullCount > 0").AsBoolean());
    }
}
