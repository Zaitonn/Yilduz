using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Blob;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("type")]
    [InlineData("size")]
    [InlineData("text")]
    [InlineData("stream")]
    [InlineData("arrayBuffer")]
    [InlineData("slice")]
    public void ShouldHaveCorrectPrototype(string property)
    {
        Assert.True(Engine.Evaluate($"Blob.prototype.hasOwnProperty('{property}')").AsBoolean());
    }

    [Theory]
    [InlineData("Blob.prototype.type")]
    [InlineData("Blob.prototype.size")]
    [InlineData("Blob.prototype.text()")]
    [InlineData("Blob.prototype.stream()")]
    [InlineData("Blob.prototype.arrayBuffer()")]
    [InlineData("Blob.prototype.slice()")]
    public void ShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate(expression));
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Engine.Execute("const blob = new Blob();");

        Assert.Equal(
            "Blob",
            Engine
                .Evaluate("Object.prototype.toString.call(blob)")
                .AsString()
                .Replace("[object ", "")
                .Replace("]", "")
        );
    }

    [Fact]
    public void ShouldInheritFromCorrectPrototype()
    {
        Engine.Execute("const blob = new Blob();");
        Assert.True(Engine.Evaluate("blob instanceof Blob").AsBoolean());
        Assert.True(Engine.Evaluate("Object.getPrototypeOf(blob) === Blob.prototype").AsBoolean());
    }
}
