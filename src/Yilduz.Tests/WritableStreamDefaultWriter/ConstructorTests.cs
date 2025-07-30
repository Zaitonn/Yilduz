using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldBeGlobalConstructor()
    {
        Assert.True(
            Engine.Evaluate("typeof WritableStreamDefaultWriter === 'function'").AsBoolean()
        );
        Assert.True(Engine.Evaluate("WritableStreamDefaultWriter.prototype").IsObject());
    }

    [Fact]
    public void ShouldCreateWriterFromStream()
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
    public void ShouldCreateWriterWithConstructor()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = new WritableStreamDefaultWriter(stream);
            """
        );

        Assert.True(Engine.Evaluate("writer instanceof WritableStreamDefaultWriter").AsBoolean());
        Assert.True(Engine.Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenConstructorCalledWithoutArguments()
    {
        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("new WritableStreamDefaultWriter();")
        );
    }

    [Fact]
    public void ShouldThrowWhenConstructorCalledWithInvalidArgument()
    {
        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("new WritableStreamDefaultWriter('invalid');")
        );
    }

    [Fact]
    public void ShouldThrowWhenStreamIsAlreadyLocked()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer1 = stream.getWriter();
            """
        );

        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("new WritableStreamDefaultWriter(stream);")
        );
    }

    [Fact]
    public void ShouldThrowWhenStreamIsClosed()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Engine.Execute("writer.close();");

        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("new WritableStreamDefaultWriter(stream);")
        );
    }
}
