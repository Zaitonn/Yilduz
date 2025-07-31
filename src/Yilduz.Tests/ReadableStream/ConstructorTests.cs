using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.ReadableStream;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateReadableStreamWithoutArguments()
    {
        Engine.Execute("const stream = new ReadableStream();");
        Assert.Equal("ReadableStream", Engine.Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldCreateReadableStreamWithEmptyUnderlyingSource()
    {
        Engine.Execute("const stream = new ReadableStream({});");
        Assert.Equal("ReadableStream", Engine.Evaluate("stream.constructor.name"));
        Assert.False(Engine.Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldCreateReadableStreamWithStartMethod()
    {
        Engine.Execute(
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
        Assert.True(Engine.Evaluate("startCalled").AsBoolean());
    }

    [Fact]
    public void ShouldCreateReadableStreamWithPullMethod()
    {
        Engine.Execute(
            """
            const stream = new ReadableStream({
                pull(controller) {
                    controller.enqueue('data');
                    controller.close();
                }
            });
            """
        );
        Assert.Equal("ReadableStream", Engine.Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldCreateReadableStreamWithCancelMethod()
    {
        Engine.Execute(
            """
            let cancelCalled = false;
            const stream = new ReadableStream({
                cancel(reason) {
                    cancelCalled = true;
                }
            });
            """
        );
        Assert.Equal("ReadableStream", Engine.Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldCreateReadableStreamWithStrategy()
    {
        Engine.Execute(
            """
            const stream = new ReadableStream({}, {
                highWaterMark: 16,
                size() { return 1; }
            });
            """
        );
        Assert.Equal("ReadableStream", Engine.Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldThrowErrorForInvalidType()
    {
        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("new ReadableStream({ type: 'invalid' })")
        );
    }

    [Fact]
    public void ShouldThrowErrorForBytesTypeWithSizeStrategy()
    {
        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("new ReadableStream({ type: 'bytes' }, { size: () => 1 });")
        );
    }
}
