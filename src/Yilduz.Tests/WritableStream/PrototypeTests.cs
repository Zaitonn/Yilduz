using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStream;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("constructor")]
    [InlineData("locked")]
    [InlineData("abort")]
    [InlineData("close")]
    [InlineData("getWriter")]
    public void WritableStreamShouldHaveCorrectPrototype(string propertyName)
    {
        Assert.True(
            Engine
                .Evaluate($"WritableStream.prototype.hasOwnProperty('{propertyName}')")
                .AsBoolean()
        );
    }

    [Theory]
    [InlineData("WritableStream.prototype.locked")]
    [InlineData("WritableStream.prototype.abort()")]
    [InlineData("WritableStream.prototype.close()")]
    [InlineData("WritableStream.prototype.getWriter()")]
    public void WritableStreamShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }

    [Theory]
    [InlineData("constructor")]
    [InlineData("closed")]
    [InlineData("ready")]
    [InlineData("desiredSize")]
    [InlineData("abort")]
    [InlineData("close")]
    [InlineData("releaseLock")]
    [InlineData("write")]
    public void WritableStreamDefaultWriterShouldHaveCorrectPrototype(string propertyName)
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
    public void WritableStreamDefaultWriterShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }

    [Theory]
    [InlineData("constructor")]
    [InlineData("error")]
    public void WritableStreamDefaultControllerShouldHaveCorrectPrototype(string propertyName)
    {
        Assert.True(
            Engine
                .Evaluate(
                    $"WritableStreamDefaultController.prototype.hasOwnProperty('{propertyName}')"
                )
                .AsBoolean()
        );
    }

    [Theory]
    [InlineData("WritableStreamDefaultController.prototype.error()")]
    public void WritableStreamDefaultControllerShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }
}
