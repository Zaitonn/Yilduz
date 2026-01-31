using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldBeGlobalConstructor()
    {
        Assert.True(Evaluate("typeof WritableStreamDefaultWriter === 'function'").AsBoolean());
        Assert.True(Evaluate("WritableStreamDefaultWriter.prototype").IsObject());
    }

    [Fact]
    public void ShouldCreateWriterFromStream()
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
    public void ShouldCreateWriterWithConstructor()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = new WritableStreamDefaultWriter(stream);
            """
        );

        Assert.True(Evaluate("writer instanceof WritableStreamDefaultWriter").AsBoolean());
        Assert.True(Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenConstructorCalledWithoutArguments()
    {
        Assert.Throws<JavaScriptException>(() => Execute("new WritableStreamDefaultWriter();"));
    }

    [Fact]
    public void ShouldThrowWhenConstructorCalledWithInvalidArgument()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("new WritableStreamDefaultWriter('invalid');")
        );
    }

    [Fact]
    public void ShouldThrowWhenStreamIsAlreadyLocked()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer1 = stream.getWriter();
            """
        );

        Assert.Throws<JavaScriptException>(
            () => Execute("new WritableStreamDefaultWriter(stream);")
        );
    }

    [Fact]
    public void ShouldThrowWhenStreamIsClosed()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Execute("writer.close();");

        Assert.Throws<JavaScriptException>(
            () => Execute("new WritableStreamDefaultWriter(stream);")
        );
    }
}
