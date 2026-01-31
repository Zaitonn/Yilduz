using Jint;
using Xunit;

namespace Yilduz.Tests.TextDecoderStream;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateTextDecoderStream()
    {
        Execute("const stream = new TextDecoderStream();");
        Assert.Equal("TextDecoderStream", Evaluate("stream.constructor.name").AsString());
    }

    [Fact]
    public void ShouldHaveDefaultProperties()
    {
        Execute("const stream = new TextDecoderStream();");
        Assert.Equal("utf-8", Evaluate("stream.encoding").AsString());
        Assert.False(Evaluate("stream.fatal").AsBoolean());
        Assert.False(Evaluate("stream.ignoreBOM").AsBoolean());
    }

    [Fact]
    public void ShouldAcceptEncodingAndOptions()
    {
        Execute("const stream = new TextDecoderStream('utf-8', { fatal: true, ignoreBOM: true });");

        Assert.Equal("utf-8", Evaluate("stream.encoding").AsString());
        Assert.True(Evaluate("stream.fatal").AsBoolean());
        Assert.True(Evaluate("stream.ignoreBOM").AsBoolean());
    }

    [Fact]
    public void ShouldHaveReadableAndWritable()
    {
        Execute("const stream = new TextDecoderStream();");
        Assert.Equal("ReadableStream", Evaluate("stream.readable.constructor.name"));
        Assert.Equal("WritableStream", Evaluate("stream.writable.constructor.name"));
    }
}
