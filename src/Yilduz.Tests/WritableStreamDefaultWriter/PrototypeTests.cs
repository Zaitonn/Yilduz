using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("constructor")]
    [InlineData("closed")]
    [InlineData("ready")]
    [InlineData("desiredSize")]
    [InlineData("abort")]
    [InlineData("close")]
    [InlineData("releaseLock")]
    [InlineData("write")]
    public void ShouldHaveCorrectPrototypeProperties(string propertyName)
    {
        Assert.True(
            Engine
                .Evaluate($"WritableStreamDefaultWriter.prototype.hasOwnProperty('{propertyName}')")
                .AsBoolean()
        );
    }

    [Theory]
    [InlineData("WritableStreamDefaultWriter.prototype.closed")]
    [InlineData("WritableStreamDefaultWriter.prototype.ready")]
    [InlineData("WritableStreamDefaultWriter.prototype.desiredSize")]
    [InlineData("WritableStreamDefaultWriter.prototype.abort()")]
    [InlineData("WritableStreamDefaultWriter.prototype.close()")]
    [InlineData("WritableStreamDefaultWriter.prototype.releaseLock()")]
    [InlineData("WritableStreamDefaultWriter.prototype.write()")]
    public void ShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate(expression));
    }

    [Fact]
    public void ShouldHaveCorrectConstructorName()
    {
        Assert.Equal(
            "WritableStreamDefaultWriter",
            Engine.Evaluate("WritableStreamDefaultWriter.name").AsString()
        );
    }

    [Fact]
    public void ShouldHaveCorrectPrototypeChain()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(
            Engine
                .Evaluate("Object.getPrototypeOf(writer) === WritableStreamDefaultWriter.prototype")
                .AsBoolean()
        );
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
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

    [Fact]
    public void ShouldNotBeEnumerable()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const keys = Object.keys(writer);
            """
        );

        Assert.Equal(0, Engine.Evaluate("keys.length").AsNumber());
    }

    [Fact]
    public void ShouldHaveNonEnumerableProperties()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        // Check that properties are non-enumerable
        Assert.False(
            Engine.Evaluate("Object.propertyIsEnumerable.call(writer, 'closed')").AsBoolean()
        );
        Assert.False(
            Engine.Evaluate("Object.propertyIsEnumerable.call(writer, 'ready')").AsBoolean()
        );
        Assert.False(
            Engine.Evaluate("Object.propertyIsEnumerable.call(writer, 'desiredSize')").AsBoolean()
        );
    }

    [Fact]
    public void ShouldHaveCorrectMethodNames()
    {
        Assert.Equal(
            "write",
            Engine.Evaluate("WritableStreamDefaultWriter.prototype.write.name").AsString()
        );
        Assert.Equal(
            "close",
            Engine.Evaluate("WritableStreamDefaultWriter.prototype.close.name").AsString()
        );
        Assert.Equal(
            "abort",
            Engine.Evaluate("WritableStreamDefaultWriter.prototype.abort.name").AsString()
        );
        Assert.Equal(
            "releaseLock",
            Engine.Evaluate("WritableStreamDefaultWriter.prototype.releaseLock.name").AsString()
        );
    }

    [Fact]
    public void ShouldHaveCorrectMethodLengths()
    {
        Assert.Equal(
            1,
            Engine.Evaluate("WritableStreamDefaultWriter.prototype.write.length").AsNumber()
        );
        Assert.Equal(
            0,
            Engine.Evaluate("WritableStreamDefaultWriter.prototype.close.length").AsNumber()
        );
        Assert.Equal(
            1,
            Engine.Evaluate("WritableStreamDefaultWriter.prototype.abort.length").AsNumber()
        );
        Assert.Equal(
            0,
            Engine.Evaluate("WritableStreamDefaultWriter.prototype.releaseLock.length").AsNumber()
        );
    }

    [Fact]
    public void ShouldNotAllowPrototypeModification()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            const originalWrite = writer.write;
            """
        );

        // Try to modify prototype method
        Engine.Execute(
            "WritableStreamDefaultWriter.prototype.write = function() { return 'modified'; };"
        );

        // Writer instance should still use original method
        Assert.True(Engine.Evaluate("writer.write !== originalWrite").AsBoolean());
    }
}
