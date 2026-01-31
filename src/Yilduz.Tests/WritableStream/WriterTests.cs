using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStream;

public sealed class WriterTests : TestBase
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
            () => Execute("new WritableStreamDefaultWriter('not a stream');")
        );
    }

    [Fact]
    public void ShouldHaveCorrectProperties()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Evaluate("typeof writer.closed === 'object'").AsBoolean());
        Assert.True(Evaluate("typeof writer.ready === 'object'").AsBoolean());
        Assert.True(
            Engine
                .Evaluate("writer.desiredSize === null || typeof writer.desiredSize === 'number'")
                .AsBoolean()
        );
    }

    [Fact]
    public void ShouldHaveCorrectMethods()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Evaluate("typeof writer.abort === 'function'").AsBoolean());
        Assert.True(Evaluate("typeof writer.close === 'function'").AsBoolean());
        Assert.True(Evaluate("typeof writer.releaseLock === 'function'").AsBoolean());
        Assert.True(Evaluate("typeof writer.write === 'function'").AsBoolean());
    }

    [Fact]
    public void ShouldWriteToStream()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const writePromise = writer.write('Hello World');
            """
        );

        Assert.True(Evaluate("writePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldReleaseLock()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Evaluate("stream.locked").AsBoolean());

        Execute("writer.releaseLock();");

        Assert.False(Evaluate("stream.locked").AsBoolean());
    }

    [Fact]
    public void ShouldCloseWriter()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const closePromise = writer.close();
            """
        );

        Assert.True(Evaluate("closePromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldAbortWriter()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const abortPromise = writer.abort('Test reason');
            """
        );

        Assert.True(Evaluate("abortPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHaveToStringTag()
    {
        Execute(
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
