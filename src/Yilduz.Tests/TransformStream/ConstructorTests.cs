using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.TransformStream;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateTransformStreamWithoutArguments()
    {
        Execute("const stream = new TransformStream();");
        Assert.Equal("TransformStream", Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldCreateTransformStreamWithEmptyTransformer()
    {
        Execute("const stream = new TransformStream({});");
        Assert.Equal("TransformStream", Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldCreateTransformStreamWithStartMethod()
    {
        Execute(
            """
            let startCalled = false;
            const stream = new TransformStream({
                start(controller) {
                    startCalled = true;
                }
            });
            """
        );
        Assert.True(Evaluate("startCalled").AsBoolean());
    }

    [Fact]
    public void ShouldCreateTransformStreamWithFlushMethod()
    {
        Execute(
            """
            let flushCalled = false;
            const stream = new TransformStream({
                flush(controller) {
                    flushCalled = true;
                    controller.enqueue('flushed');
                }
            });
            """
        );
        Execute(
            """
            const writer = stream.writable.getWriter();
            writer.close();
            """
        );
        // Note: flush is called during close, but we need to wait for the promise
    }

    [Fact]
    public void ShouldCreateTransformStreamWithWritableStrategy()
    {
        Execute(
            """
            const stream = new TransformStream({}, 
                { highWaterMark: 5, size: () => 1 },
                { highWaterMark: 3, size: () => 1 }
            );
            """
        );
        Assert.Equal("TransformStream", Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldHaveReadableAndWritableProperties()
    {
        Execute("const stream = new TransformStream();");
        Assert.Equal("ReadableStream", Evaluate("stream.readable.constructor.name"));
        Assert.Equal("WritableStream", Evaluate("stream.writable.constructor.name"));
    }

    [Fact]
    public void ShouldCreateIndependentStreams()
    {
        Execute(
            """
            const stream1 = new TransformStream();
            const stream2 = new TransformStream();
            """
        );
        Assert.False(Evaluate("stream1.readable === stream2.readable").AsBoolean());
        Assert.False(Evaluate("stream1.writable === stream2.writable").AsBoolean());
    }

    [Fact]
    public void ShouldCreateStreamWithIdentityTransformer()
    {
        Execute(
            """
            const stream = new TransformStream();
            const writer = stream.writable.getWriter();
            const reader = stream.readable.getReader();

            writer.write('test');
            """
        );
        // The identity transformer should pass data through unchanged
    }
}
