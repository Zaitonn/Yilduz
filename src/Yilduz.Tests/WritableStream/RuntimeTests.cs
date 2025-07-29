using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStream;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldBeGlobalConstructor()
    {
        Assert.True(Engine.Evaluate("typeof WritableStream === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("WritableStream.prototype").IsObject());
    }

    [Fact]
    public void ShouldCreateWritableStreamInstance()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            """
        );

        Assert.True(Engine.Evaluate("stream instanceof WritableStream").AsBoolean());
        Assert.True(Engine.Evaluate("typeof stream.locked === 'boolean'").AsBoolean());
        Assert.False(Engine.Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldCreateWritableStreamWithUnderlyingSink()
    {
        Engine.Execute(
            """
            let startCalled = false;
            const stream = new WritableStream({
                start(controller) {
                    startCalled = true;
                }
            });
            """
        );

        Assert.True(Engine.Evaluate("stream instanceof WritableStream").AsBoolean());
        Assert.True(Engine.Evaluate("startCalled").AsBoolean());
    }

    [Fact]
    public void ShouldGetWriter()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("writer instanceof WritableStreamDefaultWriter").AsBoolean());
        Assert.True(Engine.Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenGettingWriterOnLockedStream()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer1 = stream.getWriter();
            """
        );

        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("const writer2 = stream.getWriter();")
        );
    }

    [Fact]
    public void ShouldHaveCorrectMethods()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            """
        );

        Assert.True(Engine.Evaluate("typeof stream.abort === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("typeof stream.close === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("typeof stream.getWriter === 'function'").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectInheritanceChain()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            """
        );

        Assert.True(Engine.Evaluate("stream instanceof WritableStream").AsBoolean());
        Assert.True(Engine.Evaluate("stream instanceof Object").AsBoolean());
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
