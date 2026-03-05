using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Compression;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("CompressionStream", "readable")]
    [InlineData("CompressionStream", "writable")]
    [InlineData("DecompressionStream", "readable")]
    [InlineData("DecompressionStream", "writable")]
    public void ShouldExposePrototypeProperty(string ctor, string property)
    {
        Assert.True(Evaluate($"{ctor}.prototype.hasOwnProperty('{property}')").AsBoolean());
    }

    [Theory]
    [InlineData("CompressionStream.prototype.readable")]
    [InlineData("CompressionStream.prototype.writable")]
    [InlineData("DecompressionStream.prototype.readable")]
    [InlineData("DecompressionStream.prototype.writable")]
    public void ShouldThrowOnIllegalInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }

    [Theory]
    [InlineData("CompressionStream")]
    [InlineData("DecompressionStream")]
    public void ShouldReturnCorrectToStringTag(string ctor)
    {
        Execute($"const obj = new {ctor}('gzip');");
        Assert.Equal($"[object {ctor}]", Evaluate("obj.toString()"));
    }
}
