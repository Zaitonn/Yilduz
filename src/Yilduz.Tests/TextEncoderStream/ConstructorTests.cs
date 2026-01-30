using Jint;
using Xunit;

namespace Yilduz.Tests.TextEncoderStream;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateTextEncoderStream()
    {
        Engine.Execute("const stream = new TextEncoderStream();");
        Assert.Equal("TextEncoderStream", Engine.Evaluate("stream.constructor.name").AsString());
    }

    [Fact]
    public void ShouldHaveCorrectEncoding()
    {
        Engine.Execute("const stream = new TextEncoderStream();");
        Assert.Equal("utf-8", Engine.Evaluate("stream.encoding").AsString());
    }

    [Fact]
    public void ShouldHaveReadableAndWritable()
    {
        Engine.Execute("const stream = new TextEncoderStream();");
        Assert.Equal("ReadableStream", Engine.Evaluate("stream.readable.constructor.name"));
        Assert.Equal("WritableStream", Engine.Evaluate("stream.writable.constructor.name"));
    }
}
