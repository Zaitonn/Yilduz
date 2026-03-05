using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Compression;

public sealed class ConstructorTests : TestBase
{
    [Theory]
    [InlineData("CompressionStream", "gzip")]
    [InlineData("CompressionStream", "deflate")]
    [InlineData("DecompressionStream", "gzip")]
    [InlineData("DecompressionStream", "deflate")]
    public void ShouldConstructWithSupportedFormats(string ctor, string format)
    {
        Execute($"const stream = new {ctor}('{format}');");
        Assert.Equal(ctor, Evaluate("stream.constructor.name"));
        Assert.Equal("ReadableStream", Evaluate("stream.readable.constructor.name"));
        Assert.Equal("WritableStream", Evaluate("stream.writable.constructor.name"));
    }

    [Theory]
    [InlineData("CompressionStream")]
    [InlineData("DecompressionStream")]
    public void ShouldThrowOnUnsupportedFormat(string ctor)
    {
        Assert.Throws<JavaScriptException>(() => Execute($"new {ctor}('unknown');"));
    }
}
