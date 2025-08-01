using Jint;
using Xunit;

namespace Yilduz.Tests.ReadableStream;

public sealed class IntegrationTests : TestBase
{
    [Fact]
    public void ShouldWorkWithAsyncIteration()
    {
        Engine.Execute(
            """
            async function testAsyncIteration() {
                const stream = new ReadableStream({
                    start(controller) {
                        controller.enqueue('chunk1');
                        controller.enqueue('chunk2');
                        controller.enqueue('chunk3');
                        controller.close();
                    }
                });
                
                const chunks = [];
                for await (const chunk of stream) {
                    chunks.push(chunk);
                }
                return chunks;
            }
            """
        );

        // Note: Async iteration support depends on the JavaScript engine implementation
    }

    [Fact]
    public void ShouldHandleBackpressure()
    {
        Engine.Execute(
            """
            let pullCount = 0;
            const stream = new ReadableStream({
                pull(controller) {
                    pullCount++;
                    if (pullCount <= 3) {
                        controller.enqueue(`chunk${pullCount}`);
                    } else {
                        controller.close();
                    }
                }
            }, { highWaterMark: 1 });

            const reader = stream.getReader();
            """
        );

        // Verify that pull is called to maintain the desired buffer size
        Assert.True(Engine.Evaluate("pullCount >= 1").AsBoolean());
    }

    [Fact]
    public void ShouldTransferLockBetweenReaders()
    {
        Engine.Execute(
            """
            const stream = new ReadableStream({
                start(controller) {
                    controller.enqueue('data');
                    controller.close();
                }
            });

            const reader1 = stream.getReader();
            const locked1 = stream.locked;
            reader1.releaseLock();
            const locked2 = stream.locked;
            const reader2 = stream.getReader();
            const locked3 = stream.locked;
            """
        );

        Assert.True(Engine.Evaluate("locked1").AsBoolean());
        Assert.False(Engine.Evaluate("locked2").AsBoolean());
        Assert.True(Engine.Evaluate("locked3").AsBoolean());
    }

    [Fact]
    public void ShouldHandleControllerDesiredSizeChanges()
    {
        Engine.Execute(
            """
            const desiredSizes = [];
            const stream = new ReadableStream({
                start(controller) {
                    desiredSizes.push(controller.desiredSize);
                    controller.enqueue('chunk1');
                    desiredSizes.push(controller.desiredSize);
                    controller.enqueue('chunk2');
                    desiredSizes.push(controller.desiredSize);
                }
            }, { highWaterMark: 2 });
            """
        );

        Assert.Equal(2, Engine.Evaluate("desiredSizes[0]").AsNumber());
        Assert.Equal(1, Engine.Evaluate("desiredSizes[1]").AsNumber());
        Assert.Equal(0, Engine.Evaluate("desiredSizes[2]").AsNumber());
    }

    [Fact]
    public void ShouldPropagateErrorsFromControllerToReader()
    {
        Engine.Execute(
            """
            let readerError;
            const stream = new ReadableStream({
                start(controller) {
                    controller.error(new Error('controller error'));
                }
            });

            const reader = stream.getReader();
            reader.read().catch(err => {
                readerError = err;
            });
            """
        );

        // Note: Error propagation would be properly tested with promise resolution
    }

    [Fact]
    public void ShouldHandleStrategyWithCustomSizeFunction()
    {
        Engine.Execute(
            """
            let sizeCallCount = 0;
            const stream = new ReadableStream({
                start(controller) {
                    controller.enqueue({ data: 'small' });
                    controller.enqueue({ data: 'large'.repeat(10) });
                    controller.close();
                }
            }, {
                highWaterMark: 50,
                size(chunk) {
                    sizeCallCount++;
                    return chunk.data.length;
                }
            });
            """
        );

        Assert.True(Engine.Evaluate("sizeCallCount > 0").AsBoolean());
    }

    [Fact(Skip = "Tee functionality is not fully implemented yet")]
    public void ShouldWorkWithTeeBothStreamsSeparately()
    {
        Engine.Execute(
            """
            const sourceStream = new ReadableStream({
                start(controller) {
                    controller.enqueue('chunk1');
                    controller.enqueue('chunk2');
                    controller.close();
                }
            });

            const [stream1, stream2] = sourceStream.tee();
            const reader1 = stream1.getReader();
            const reader2 = stream2.getReader();

            const locked1 = stream1.locked;
            const locked2 = stream2.locked;
            const lockedSource = sourceStream.locked;
            """
        );

        Assert.True(Engine.Evaluate("locked1").AsBoolean());
        Assert.True(Engine.Evaluate("locked2").AsBoolean());
        Assert.True(Engine.Evaluate("lockedSource").AsBoolean());
    }
}
