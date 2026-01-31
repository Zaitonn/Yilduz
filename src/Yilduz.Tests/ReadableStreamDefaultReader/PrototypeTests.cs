using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.ReadableStreamDefaultReader;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("constructor")]
    [InlineData("closed")]
    [InlineData("cancel")]
    [InlineData("read")]
    [InlineData("releaseLock")]
    public void ReadableStreamDefaultReaderShouldHaveCorrectPrototype(string propertyName)
    {
        Assert.True(
            Engine
                .Evaluate($"ReadableStreamDefaultReader.prototype.hasOwnProperty('{propertyName}')")
                .AsBoolean()
        );
    }

    [Theory]
    [InlineData("ReadableStreamDefaultReader.prototype.closed")]
    [InlineData("ReadableStreamDefaultReader.prototype.cancel()")]
    [InlineData("ReadableStreamDefaultReader.prototype.read()")]
    [InlineData("ReadableStreamDefaultReader.prototype.releaseLock()")]
    public void ReadableStreamDefaultReaderShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Execute(
            """
            const stream = new ReadableStream();
            const reader = stream.getReader();
            const readerToStringTag = Object.prototype.toString.call(reader);
            """
        );
        Assert.Equal(
            "[object ReadableStreamDefaultReader]",
            Evaluate("readerToStringTag").AsString()
        );
    }

    [Fact]
    public void ShouldHaveCorrectConstructorName()
    {
        Assert.Equal(
            "ReadableStreamDefaultReader",
            Evaluate("ReadableStreamDefaultReader.name").AsString()
        );
    }

    [Fact]
    public void ShouldNotBeCallableAsFunction()
    {
        Assert.Throws<JavaScriptException>(() => Evaluate("ReadableStreamDefaultReader()"));
    }

    [Fact]
    public void ShouldNotBeConstructableDirectly()
    {
        Assert.Throws<JavaScriptException>(() => Evaluate("new ReadableStreamDefaultReader()"));
    }
}
