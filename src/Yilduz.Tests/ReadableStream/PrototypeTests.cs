using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.ReadableStream;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("constructor")]
    [InlineData("locked")]
    [InlineData("cancel")]
    [InlineData("getReader")]
    [InlineData("pipeThrough")]
    [InlineData("pipeTo")]
    [InlineData("tee")]
    public void ReadableStreamShouldHaveCorrectPrototype(string propertyName)
    {
        Assert.True(
            Engine
                .Evaluate($"ReadableStream.prototype.hasOwnProperty('{propertyName}')")
                .AsBoolean()
        );
    }

    [Theory]
    [InlineData("ReadableStream.prototype.locked")]
    [InlineData("ReadableStream.prototype.cancel()")]
    [InlineData("ReadableStream.prototype.getReader()")]
    [InlineData("ReadableStream.prototype.pipeThrough()")]
    [InlineData("ReadableStream.prototype.pipeTo()")]
    [InlineData("ReadableStream.prototype.tee()")]
    public void ReadableStreamShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Assert.Equal(
            "[object ReadableStream]",
            Evaluate("Object.prototype.toString.call(new ReadableStream())").AsString()
        );
    }

    [Fact]
    public void ShouldHaveCorrectConstructorName()
    {
        Assert.Equal("ReadableStream", Evaluate("ReadableStream.name").AsString());
    }

    [Fact]
    public void ShouldNotBeCallableAsFunction()
    {
        Assert.Throws<JavaScriptException>(() => Evaluate("ReadableStream()"));
    }

    [Fact]
    public void LockedPropertyShouldBeReadOnly()
    {
        Execute(
            """
            const stream = new ReadableStream();
            const originalLocked = stream.locked;
            stream.locked = true;
            """
        );
        Assert.Equal(Evaluate("originalLocked").AsBoolean(), Evaluate("stream.locked").AsBoolean());
    }
}
