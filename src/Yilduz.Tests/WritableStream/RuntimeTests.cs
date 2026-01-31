using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStream;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldBeGlobalConstructor()
    {
        Assert.True(Evaluate("typeof WritableStream === 'function'").AsBoolean());
        Assert.True(Evaluate("WritableStream.prototype").IsObject());
    }

    [Fact]
    public void ShouldCreateWritableStreamInstance()
    {
        Execute(
            """
            const stream = new WritableStream();
            """
        );

        Assert.True(Evaluate("stream instanceof WritableStream").AsBoolean());
        Assert.True(Evaluate("typeof stream.locked === 'boolean'").AsBoolean());
        Assert.False(Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldCreateWritableStreamWithUnderlyingSink()
    {
        Execute(
            """
            let startCalled = false;
            const stream = new WritableStream({
                start(controller) {
                    startCalled = true;
                }
            });
            """
        );

        Assert.True(Evaluate("stream instanceof WritableStream").AsBoolean());
        Assert.True(Evaluate("startCalled").AsBoolean());
    }

    [Fact]
    public void ShouldGetWriter()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Evaluate("writer instanceof WritableStreamDefaultWriter").AsBoolean());
        Assert.True(Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenGettingWriterOnLockedStream()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer1 = stream.getWriter();
            """
        );

        Assert.Throws<JavaScriptException>(() => Execute("const writer2 = stream.getWriter();"));
    }

    [Fact]
    public void ShouldHaveCorrectMethods()
    {
        Execute(
            """
            const stream = new WritableStream();
            """
        );

        Assert.True(Evaluate("typeof stream.abort === 'function'").AsBoolean());
        Assert.True(Evaluate("typeof stream.close === 'function'").AsBoolean());
        Assert.True(Evaluate("typeof stream.getWriter === 'function'").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectInheritanceChain()
    {
        Execute(
            """
            const stream = new WritableStream();
            """
        );

        Assert.True(Evaluate("stream instanceof WritableStream").AsBoolean());
        Assert.True(Evaluate("stream instanceof Object").AsBoolean());
    }

    [Fact]
    public void ShouldHaveToStringTag()
    {
        Assert.Equal(
            "WritableStream",
            Engine
                .Evaluate("Object.prototype.toString.call(new WritableStream())")
                .AsString()
                .Replace("[object ", "")
                .Replace("]", "")
        );
    }
}
