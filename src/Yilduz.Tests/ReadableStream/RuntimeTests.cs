using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.ReadableStream;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldReportLockedStatusCorrectly()
    {
        Execute(
            """
            const stream = new ReadableStream();
            const initialLocked = stream.locked;
            const reader = stream.getReader();
            const lockedAfterReader = stream.locked;
            reader.releaseLock();
            const lockedAfterRelease = stream.locked;
            """
        );

        Assert.False(Evaluate("initialLocked").AsBoolean());
        Assert.True(Evaluate("lockedAfterReader").AsBoolean());
        Assert.False(Evaluate("lockedAfterRelease").AsBoolean());
    }

    [Fact]
    public void ShouldGetDefaultReader()
    {
        Execute(
            """
            const stream = new ReadableStream();
            const reader = stream.getReader();
            """
        );

        Assert.Equal("ReadableStreamDefaultReader", Evaluate("reader.constructor.name").AsString());
    }

    [Fact(Skip = "byob is not implemented yet")]
    public void ShouldGetBYOBReaderForBytesStream()
    {
        Execute(
            """
            const stream = new ReadableStream({ type: 'bytes' });
            const reader = stream.getReader({ mode: 'byob' });
            """
        );

        Assert.Equal("ReadableStreamBYOBReader", Evaluate("reader.constructor.name").AsString());
    }

    [Fact]
    public void ShouldThrowWhenGettingReaderOnLockedStream()
    {
        Execute(
            """
            const stream = new ReadableStream();
            const reader1 = stream.getReader();
            let caughtError;
            try {
                const reader2 = stream.getReader();
            } catch (e) {
                caughtError = e;
            }
            """
        );

        Assert.True(Evaluate("caughtError instanceof TypeError").AsBoolean());
    }

    [Fact]
    public void ShouldCancelStream()
    {
        Execute(
            """
            let cancelReason;
            const stream = new ReadableStream({
                cancel(reason) {
                    cancelReason = reason;
                    return 'cancelled';
                }
            });
            """
        );

        Evaluate("stream.cancel('test reason')").UnwrapIfPromise();
        Assert.Equal("test reason", Evaluate("cancelReason").AsString());
    }

    [Fact]
    public void ShouldThrowWhenTeeingLockedStream()
    {
        Execute(
            """
            const stream = new ReadableStream();
            const reader = stream.getReader();
            let caughtError;
            try {
                stream.tee();
            } catch (e) {
                caughtError = e;
            }
            """
        );

        Assert.True(Evaluate("caughtError instanceof TypeError").AsBoolean());
    }

    [Fact(Skip = "TransformStream is not implemented yet")]
    public void ShouldPipeThroughTransformStream()
    {
        Execute(
            """
            const readable = new ReadableStream({
                start(controller) {
                    controller.enqueue('data');
                    controller.close();
                }
            });
            const transform = new TransformStream();
            const result = readable.pipeThrough(transform);
            """
        );

        Assert.Equal("ReadableStream", Evaluate("result.constructor.name").AsString());
    }
}
