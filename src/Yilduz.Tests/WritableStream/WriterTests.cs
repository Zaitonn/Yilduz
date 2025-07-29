using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStream;

public sealed class WriterTests : TestBase
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
            () => Engine.Execute("new WritableStreamDefaultWriter('not a stream');")
        );
    }

    [Fact]
    public void ShouldHaveCorrectProperties()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("typeof writer.closed === 'object'").AsBoolean());
        Assert.True(Engine.Evaluate("typeof writer.ready === 'object'").AsBoolean());
        Assert.True(
            Engine
                .Evaluate("writer.desiredSize === null || typeof writer.desiredSize === 'number'")
                .AsBoolean()
        );
    }

    [Fact]
    public void ShouldHaveCorrectMethods()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("typeof writer.abort === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("typeof writer.close === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("typeof writer.releaseLock === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("typeof writer.write === 'function'").AsBoolean());
    }

    [Fact]
    public void ShouldWriteToStream()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const writePromise = writer.write('Hello World');
            """
        );

        Assert.True(Engine.Evaluate("writePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldReleaseLock()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("stream.locked").AsBoolean());

        Engine.Execute("writer.releaseLock();");

        Assert.False(Engine.Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldCloseWriter()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const closePromise = writer.close();
            """
        );

        Assert.True(Engine.Evaluate("closePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldAbortWriter()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const abortPromise = writer.abort('Test reason');
            """
        );

        Assert.True(Engine.Evaluate("abortPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHaveToStringTag()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.Equal(
            "WritableStreamDefaultWriter",
            Engine
                .Evaluate("Object.prototype.toString.call(writer)")
                .AsString()
                .Replace("[object ", "")
                .Replace("]", "")
        );
    }
}
