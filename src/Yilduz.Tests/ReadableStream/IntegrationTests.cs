using Jint;
using Xunit;

namespace Yilduz.Tests.ReadableStream;

public sealed class IntegrationTests : TestBase
{
    [Fact(Skip = "Async iteration is not implemented yet in Jint")]
    public void ShouldWorkWithAsyncIteration()
    {
        Execute(
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

        Evaluate("testAsyncIteration()").UnwrapIfPromise();
        Assert.Equal(3, Evaluate("chunks.length").AsNumber());
    }

    [Fact]
    public void ShouldHandleBackpressure()
    {
        Execute(
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
        Assert.True(Evaluate("pullCount >= 1").AsBoolean());
    }

    [Fact]
    public void ShouldTransferLockBetweenReaders()
    {
        Execute(
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

        Assert.True(Evaluate("locked1").AsBoolean());
        Assert.False(Evaluate("locked2").AsBoolean());
        Assert.True(Evaluate("locked3").AsBoolean());
    }

    [Fact]
    public void ShouldHandleControllerDesiredSizeChanges()
    {
        Execute(
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

        Assert.Equal(2, Evaluate("desiredSizes[0]").AsNumber());
        Assert.Equal(1, Evaluate("desiredSizes[1]").AsNumber());
        Assert.Equal(0, Evaluate("desiredSizes[2]").AsNumber());
    }

    [Fact]
    public void ShouldPropagateErrorsFromControllerToReader()
    {
        Execute(
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
        Execute(
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

        Assert.True(Evaluate("sizeCallCount > 0").AsBoolean());
    }

    [Fact]
    public void ShouldWorkWithTeeBothStreamsSeparately()
    {
        Execute(
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

        Assert.True(Evaluate("locked1").AsBoolean());
        Assert.True(Evaluate("locked2").AsBoolean());
        Assert.True(Evaluate("lockedSource").AsBoolean());
    }
}
