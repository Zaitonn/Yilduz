using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.ReadableStream;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldReportLockedStatusCorrectly()
    {
        Engine.Execute(
            """
            const stream = new ReadableStream();
            const initialLocked = stream.locked;
            const reader = stream.getReader();
            const lockedAfterReader = stream.locked;
            reader.releaseLock();
            const lockedAfterRelease = stream.locked;
            """
        );

        Assert.False(Engine.Evaluate("initialLocked").AsBoolean());
        Assert.True(Engine.Evaluate("lockedAfterReader").AsBoolean());
        Assert.False(Engine.Evaluate("lockedAfterRelease").AsBoolean());
    }

    [Fact]
    public void ShouldGetDefaultReader()
    {
        Engine.Execute(
            """
            const stream = new ReadableStream();
            const reader = stream.getReader();
            """
        );

        Assert.Equal(
            "ReadableStreamDefaultReader",
            Engine.Evaluate("reader.constructor.name").AsString()
        );
    }

    [Fact(Skip = "byob is not implemented yet")]
    public void ShouldGetBYOBReaderForBytesStream()
    {
        Engine.Execute(
            """
            const stream = new ReadableStream({ type: 'bytes' });
            const reader = stream.getReader({ mode: 'byob' });
            """
        );

        Assert.Equal(
            "ReadableStreamBYOBReader",
            Engine.Evaluate("reader.constructor.name").AsString()
        );
    }

    [Fact]
    public void ShouldThrowWhenGettingReaderOnLockedStream()
    {
        Engine.Execute(
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

        Assert.True(Engine.Evaluate("caughtError instanceof TypeError").AsBoolean());
    }

    [Fact]
    public void ShouldCancelStream()
    {
        Engine.Execute(
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

        Engine.Evaluate("stream.cancel('test reason')").UnwrapIfPromise();
        Assert.Equal("test reason", Engine.Evaluate("cancelReason").AsString());
    }

    [Fact]
    public void ShouldThrowWhenTeeingLockedStream()
    {
        Engine.Execute(
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

        Assert.True(Engine.Evaluate("caughtError instanceof TypeError").AsBoolean());
    }

    [Fact(Skip = "TransformStream is not implemented yet")]
    public void ShouldPipeThroughTransformStream()
    {
        Engine.Execute(
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

        Assert.Equal("ReadableStream", Engine.Evaluate("result.constructor.name").AsString());
    }
}
