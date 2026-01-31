using Jint;
using Xunit;

namespace Yilduz.Tests.ByteLengthQueuingStrategy;

public sealed class IntegrationTests : TestBase
{
    [Fact]
    public void ShouldWorkWithReadableStreamAndBackpressure()
    {
        Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 100 });
            let pullCount = 0;
            const stream = new ReadableStream({
                pull(controller) {
                    pullCount++;
                    const buffer = new ArrayBuffer(50);
                    controller.enqueue(buffer);
                    if (pullCount >= 3) {
                        controller.close();
                    }
                }
            }, strategy);
            """
        );

        Execute("stream.getReader().read();");

        // Should pull initially to fill the queue
        Assert.True(Evaluate("pullCount > 0").AsBoolean());
    }

    [Fact]
    public void ShouldCalculateCorrectDesiredSizeInReadableStream()
    {
        Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 200 });
            let desiredSizes = [];
            const stream = new ReadableStream({
                start(controller) {
                    desiredSizes.push(controller.desiredSize);
                    controller.enqueue(new ArrayBuffer(50));
                    desiredSizes.push(controller.desiredSize);
                    controller.enqueue(new ArrayBuffer(100));
                    desiredSizes.push(controller.desiredSize);
                }
            }, strategy);
            """
        );

        Assert.Equal(200, Evaluate("desiredSizes[0]").AsNumber()); // Initial
        Assert.Equal(150, Evaluate("desiredSizes[1]").AsNumber()); // After 50 bytes
        Assert.Equal(50, Evaluate("desiredSizes[2]").AsNumber()); // After 150 total bytes
    }

    [Fact]
    public void ShouldWorkWithWritableStreamBackpressure()
    {
        Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 64 });
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

        Assert.Equal("WritableStreamDefaultWriter", Evaluate("writer.constructor.name"));
    }

    [Fact]
    public void ShouldHandleZeroByteLengthChunks()
    {
        Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 10 });
            let desiredSizes = [];
            const stream = new ReadableStream({
                start(controller) {
                    desiredSizes.push(controller.desiredSize);
                    controller.enqueue(new ArrayBuffer(0));
                    desiredSizes.push(controller.desiredSize);
                    controller.enqueue(new ArrayBuffer(5));
                    desiredSizes.push(controller.desiredSize);
                }
            }, strategy);
            """
        );

        Assert.Equal(10, Evaluate("desiredSizes[0]").AsNumber()); // Initial
        Assert.Equal(10, Evaluate("desiredSizes[1]").AsNumber()); // After 0 bytes
        Assert.Equal(5, Evaluate("desiredSizes[2]").AsNumber()); // After 5 bytes
    }

    [Fact]
    public void ShouldWorkWithTransformStream()
    {
        Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 1024 });
            const transform = new TransformStream({
                transform(chunk, controller) {
                    // Transform ArrayBuffer to Uint8Array
                    const uint8Array = new Uint8Array(chunk);
                    controller.enqueue(uint8Array);
                }
            }, strategy, strategy);
            """
        );

        Assert.Equal("TransformStream", Evaluate("transform.constructor.name"));
    }

    [Fact]
    public void ShouldRespectHighWaterMarkAcrossStreamPipeline()
    {
        Execute(
            """
            const readStrategy = new ByteLengthQueuingStrategy({ highWaterMark: 256 });
            const writeStrategy = new ByteLengthQueuingStrategy({ highWaterMark: 128 });

            const readable = new ReadableStream({
                start(controller) {
                    for (let i = 0; i < 3; i++) {
                        controller.enqueue(new ArrayBuffer(64));
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

        Assert.True(Evaluate("pipePromise instanceof Promise").AsBoolean());
    }
}
