using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.ReadableStream;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateReadableStreamWithoutArguments()
    {
        Execute("const stream = new ReadableStream();");
        Assert.Equal("ReadableStream", Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldCreateReadableStreamWithEmptyUnderlyingSource()
    {
        Execute("const stream = new ReadableStream({});");
        Assert.Equal("ReadableStream", Evaluate("stream.constructor.name"));
        Assert.False(Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldCreateReadableStreamWithStartMethod()
    {
        Execute(
            """
            let startCalled = false;
            const stream = new ReadableStream({
                start(controller) {
                    startCalled = true;
                    controller.enqueue('hello');
                    controller.close();
                }
            });
            """
        );
        Assert.True(Evaluate("startCalled").AsBoolean());
    }

    [Fact]
    public void ShouldCreateReadableStreamWithPullMethod()
    {
        Execute(
            """
            const stream = new ReadableStream({
                pull(controller) {
                    controller.enqueue('data');
                    controller.close();
                }
            });
            """
        );
        Assert.Equal("ReadableStream", Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldCreateReadableStreamWithCancelMethod()
    {
        Execute(
            """
            let cancelCalled = false;
            const stream = new ReadableStream({
                cancel(reason) {
                    cancelCalled = true;
                }
            });
            """
        );
        Assert.Equal("ReadableStream", Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldCreateReadableStreamWithStrategy()
    {
        Execute(
            """
            const stream = new ReadableStream({}, {
                highWaterMark: 16,
                size() { return 1; }
            });
            """
        );
        Assert.Equal("ReadableStream", Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldThrowErrorForInvalidType()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("new ReadableStream({ type: 'invalid' })")
        );
    }

    [Fact]
    public void ShouldThrowErrorForBytesTypeWithSizeStrategy()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("new ReadableStream({ type: 'bytes' }, { size: () => 1 });")
        );
    }
}
