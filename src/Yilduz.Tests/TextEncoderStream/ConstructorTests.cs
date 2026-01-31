using Jint;
using Xunit;

namespace Yilduz.Tests.TextEncoderStream;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateTextEncoderStream()
    {
        Execute("const stream = new TextEncoderStream();");
        Assert.Equal("TextEncoderStream", Evaluate("stream.constructor.name").AsString());
    }

    [Fact]
    public void ShouldHaveCorrectEncoding()
    {
        Execute("const stream = new TextEncoderStream();");
        Assert.Equal("utf-8", Evaluate("stream.encoding").AsString());
    }

    [Fact]
    public void ShouldHaveReadableAndWritable()
    {
        Execute("const stream = new TextEncoderStream();");
        Assert.Equal("ReadableStream", Evaluate("stream.readable.constructor.name"));
        Assert.Equal("WritableStream", Evaluate("stream.writable.constructor.name"));
    }
}
