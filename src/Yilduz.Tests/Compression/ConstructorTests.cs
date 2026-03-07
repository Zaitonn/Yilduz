using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Compression;

public sealed class ConstructorTests : TestBase
{
    [Theory]
    [InlineData("CompressionStream", "gzip")]
    [InlineData("DecompressionStream", "gzip")]
    [InlineData("CompressionStream", "deflate-raw")]
    [InlineData("DecompressionStream", "deflate-raw")]
#if NET6_0_OR_GREATER
    [InlineData("CompressionStream", "deflate")]
    [InlineData("DecompressionStream", "deflate")]
#endif
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
