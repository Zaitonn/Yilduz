using Jint;
using Xunit;

namespace Yilduz.Tests.TextDecoderStream;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateTextDecoderStream()
    {
        Engine.Execute("const stream = new TextDecoderStream();");
        Assert.Equal("TextDecoderStream", Engine.Evaluate("stream.constructor.name").AsString());
    }

    [Fact]
    public void ShouldHaveDefaultProperties()
    {
        Engine.Execute("const stream = new TextDecoderStream();");
        Assert.Equal("utf-8", Engine.Evaluate("stream.encoding").AsString());
        Assert.False(Engine.Evaluate("stream.fatal").AsBoolean());
        Assert.False(Engine.Evaluate("stream.ignoreBOM").AsBoolean());
    }

    [Fact]
    public void ShouldAcceptEncodingAndOptions()
    {
        Engine.Execute(
            "const stream = new TextDecoderStream('utf-8', { fatal: true, ignoreBOM: true });"
        );

        Assert.Equal("utf-8", Engine.Evaluate("stream.encoding").AsString());
        Assert.True(Engine.Evaluate("stream.fatal").AsBoolean());
        Assert.True(Engine.Evaluate("stream.ignoreBOM").AsBoolean());
    }

    [Fact]
    public void ShouldHaveReadableAndWritable()
    {
        Engine.Execute("const stream = new TextDecoderStream();");
        Assert.Equal("ReadableStream", Engine.Evaluate("stream.readable.constructor.name"));
        Assert.Equal("WritableStream", Engine.Evaluate("stream.writable.constructor.name"));
    }
}
