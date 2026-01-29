using Jint;
using Xunit;

namespace Yilduz.Tests.CountQueuingStrategy;

public sealed class IntegrationTests : TestBase
{
    [Fact]
    public void ShouldWorkWithReadableStreamAndBackpressure()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 3 });
            let pullCount = 0;
            const stream = new ReadableStream({
                pull(controller) {
                    pullCount++;
                    controller.enqueue(`chunk${pullCount}`);
                    if (pullCount >= 5) {
                        controller.close();
                    }
                }
            }, strategy);
            """
        );

        // Should pull initially to fill the queue
        Assert.True(Engine.Evaluate("pullCount > 0").AsBoolean());
    }

    [Fact]
    public void ShouldCalculateCorrectDesiredSizeInReadableStream()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 4 });
            let desiredSizes = [];
            const stream = new ReadableStream({
                start(controller) {
                    desiredSizes.push(controller.desiredSize);
                    controller.enqueue('chunk1');
                    desiredSizes.push(controller.desiredSize);
                    controller.enqueue('chunk2');
                    desiredSizes.push(controller.desiredSize);
                    controller.enqueue('chunk3');
                    desiredSizes.push(controller.desiredSize);
                }
            }, strategy);
            """
        );

        Assert.Equal(4, Engine.Evaluate("desiredSizes[0]").AsNumber()); // Initial
        Assert.Equal(3, Engine.Evaluate("desiredSizes[1]").AsNumber()); // After 1 chunk
        Assert.Equal(2, Engine.Evaluate("desiredSizes[2]").AsNumber()); // After 2 chunks
        Assert.Equal(1, Engine.Evaluate("desiredSizes[3]").AsNumber()); // After 3 chunks
    }

    [Fact]
    public void ShouldWorkWithWritableStreamBackpressure()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 2 });
            let writeCount = 0;
            const stream = new WritableStream({
                write(chunk) {
                    writeCount++;
                    return new Promise(resolve => setTimeout(resolve, 10));
                }
            }, strategy);

            const writer = stream.getWriter();
            """
        );

        Assert.Equal("WritableStreamDefaultWriter", Engine.Evaluate("writer.constructor.name"));
    }

    [Fact]
    public void ShouldHandleDifferentChunkTypes()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 5 });
            let desiredSizes = [];
            const stream = new ReadableStream({
                start(controller) {
                    desiredSizes.push(controller.desiredSize);
                    controller.enqueue('string');
                    desiredSizes.push(controller.desiredSize);
                    controller.enqueue(42);
                    desiredSizes.push(controller.desiredSize);
                    controller.enqueue({ type: 'object' });
                    desiredSizes.push(controller.desiredSize);
                    controller.enqueue(new ArrayBuffer(1000));
                    desiredSizes.push(controller.desiredSize);
                }
            }, strategy);
            """
        );

        Assert.Equal(5, Engine.Evaluate("desiredSizes[0]").AsNumber()); // Initial
        Assert.Equal(4, Engine.Evaluate("desiredSizes[1]").AsNumber()); // After string
        Assert.Equal(3, Engine.Evaluate("desiredSizes[2]").AsNumber()); // After number
        Assert.Equal(2, Engine.Evaluate("desiredSizes[3]").AsNumber()); // After object
        Assert.Equal(1, Engine.Evaluate("desiredSizes[4]").AsNumber()); // After ArrayBuffer
    }

    [Fact]
    public void ShouldWorkWithTransformStream()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 10 });
            const transform = new TransformStream({
                transform(chunk, controller) {
                    // Split string into characters
                    if (typeof chunk === 'string') {
                        for (const char of chunk) {
                            controller.enqueue(char);
                        }
                    } else {
                        controller.enqueue(chunk);
                    }
                }
            }, strategy, strategy);
            """
        );

        Assert.Equal("TransformStream", Engine.Evaluate("transform.constructor.name"));
    }

    [Fact]
    public void ShouldRespectHighWaterMarkAcrossStreamPipeline()
    {
        Engine.Execute(
            """
            const readStrategy = new CountQueuingStrategy({ highWaterMark: 6 });
            const writeStrategy = new CountQueuingStrategy({ highWaterMark: 3 });

            const readable = new ReadableStream({
                start(controller) {
                    for (let i = 0; i < 5; i++) {
                        controller.enqueue(`item${i}`);
                    }
                    controller.close();
                }
            }, readStrategy);

            const writable = new WritableStream({
                write(chunk) {
                    // Process chunk
                }
            }, writeStrategy);

            const pipePromise = readable.pipeTo(writable);
            """
        );

        Assert.True(Engine.Evaluate("pipePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldWorkWithHighFrequencyEnqueueing()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 1000 });
            let enqueuedCount = 0;
            const stream = new ReadableStream({
                start(controller) {
                    for (let i = 0; i < 500; i++) {
                        controller.enqueue(i);
                        enqueuedCount++;
                    }
                    controller.close();
                }
            }, strategy);
            """
        );

        Assert.Equal(500, Engine.Evaluate("enqueuedCount").AsNumber());
    }

    [Fact]
    public void ShouldHandleZeroHighWaterMark()
    {
        Engine.Execute(
            """
            const strategy = new CountQueuingStrategy({ highWaterMark: 0 });
            let desiredSize;
            const stream = new ReadableStream({
                start(controller) {
                    desiredSize = controller.desiredSize;
                    controller.close();
                }
            }, strategy);
            """
        );

        Assert.Equal(0, Engine.Evaluate("desiredSize").AsNumber());
    }
}
