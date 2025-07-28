using Jint;
using Xunit;

namespace Yilduz.Tests.TextEncoder;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateTextEncoderWithoutArguments()
    {
        Engine.Execute("const encoder = new TextEncoder();");
        Assert.Equal("TextEncoder", Engine.Evaluate("encoder.constructor.name").AsString());
    }

    [Fact]
    public void ShouldHaveCorrectEncoding()
    {
        Engine.Execute("const encoder = new TextEncoder();");
        Assert.Equal("utf-8", Engine.Evaluate("encoder.encoding").AsString());
    }

    [Fact]
    public void ShouldEncodeString()
    {
        Engine.Execute(
            """
            const encoder = new TextEncoder();
            const result = encoder.encode('Hello');
            """
        );

        // 'Hello' in UTF-8: [72, 101, 108, 108, 111]
        var result = Engine.Evaluate("Array.from(result)");
        var array = result.AsArray();

        Assert.Equal<uint>(5, array.Length);
        Assert.Equal(72, array[0].AsNumber()); // H
        Assert.Equal(101, array[1].AsNumber()); // e
        Assert.Equal(108, array[2].AsNumber()); // l
        Assert.Equal(108, array[3].AsNumber()); // l
        Assert.Equal(111, array[4].AsNumber()); // o
    }

    [Fact]
    public void ShouldEncodeEmptyString()
    {
        Engine.Execute(
            """
            const encoder = new TextEncoder();
            const result = encoder.encode('');
            """
        );

        var result = Engine.Evaluate("result.length");
        Assert.Equal(0, result.AsNumber());
    }

    [Fact]
    public void ShouldEncodeUnicodeString()
    {
        Engine.Execute(
            """
            const encoder = new TextEncoder();
            const result = encoder.encode('€');
            """
        );

        // '€' in UTF-8: [226, 130, 172]
        var result = Engine.Evaluate("Array.from(result)");
        var array = result.AsArray();

        Assert.Equal<uint>(3, array.Length);
        Assert.Equal(226, array[0].AsNumber());
        Assert.Equal(130, array[1].AsNumber());
        Assert.Equal(172, array[2].AsNumber());
    }
}
