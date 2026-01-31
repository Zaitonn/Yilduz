using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.TransformStream;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("constructor")]
    [InlineData("readable")]
    [InlineData("writable")]
    public void TransformStreamShouldHaveCorrectPrototype(string propertyName)
    {
        Assert.True(
            Engine
                .Evaluate($"TransformStream.prototype.hasOwnProperty('{propertyName}')")
                .AsBoolean()
        );
    }

    [Theory]
    [InlineData("TransformStream.prototype.readable")]
    [InlineData("TransformStream.prototype.writable")]
    public void TransformStreamShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Assert.Equal(
            "[object TransformStream]",
            Evaluate("Object.prototype.toString.call(new TransformStream())").AsString()
        );
    }

    [Fact]
    public void ShouldHaveCorrectConstructorName()
    {
        Assert.Equal("TransformStream", Evaluate("TransformStream.name").AsString());
    }

    [Fact]
    public void ReadablePropertyShouldReturnSameInstance()
    {
        Execute(
            """
            const stream = new TransformStream();
            const readable1 = stream.readable;
            const readable2 = stream.readable;
            """
        );
        Assert.True(Evaluate("readable1 === readable2").AsBoolean());
    }

    [Fact]
    public void WritablePropertyShouldReturnSameInstance()
    {
        Execute(
            """
            const stream = new TransformStream();
            const writable1 = stream.writable;
            const writable2 = stream.writable;
            """
        );
        Assert.True(Evaluate("writable1 === writable2").AsBoolean());
    }

    [Fact]
    public void ShouldNotBeEnumerable()
    {
        Execute("const stream = new TransformStream();");
        var keys = Evaluate("Object.keys(stream)");
        Assert.Empty(keys.AsArray());
    }

    [Fact]
    public void ShouldHaveCorrectPrototypeChain()
    {
        Assert.True(
            Engine
                .Evaluate(
                    "Object.getPrototypeOf(new TransformStream()) === TransformStream.prototype"
                )
                .AsBoolean()
        );

        Assert.True(
            Engine
                .Evaluate("Object.getPrototypeOf(TransformStream.prototype) === Object.prototype")
                .AsBoolean()
        );
    }
}
