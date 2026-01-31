using Jint;
using Jint.Runtime;
using Xunit;
using Yilduz.Extensions;

namespace Yilduz.Tests.TextDecoder;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("encoding")]
    [InlineData("fatal")]
    [InlineData("ignoreBOM")]
    [InlineData("decode")]
    public void ShouldHaveProperty(string propertyName)
    {
        Execute("const decoder = new TextDecoder();");

        Assert.True(Evaluate($"'{propertyName}' in decoder").AsBoolean());
    }

    [Fact]
    public void ShouldHaveDecodeMethod()
    {
        Execute("const decoder = new TextDecoder();");

        Assert.True(Evaluate("typeof decoder.decode === 'function'").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectMethodNames()
    {
        Execute("const decoder = new TextDecoder();");
        var methodName = nameof(Encoding.TextDecoder.TextDecoderInstance.Decode).ToJsStyleName();

        Assert.Equal("decode", Evaluate("decoder.decode.name").AsString());
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Execute("const decoder = new TextDecoder();");

        Assert.Equal(
            "[object TextDecoder]",
            Evaluate("Object.prototype.toString.call(decoder)").AsString()
        );
    }

    [Fact]
    public void ShouldNotAllowDirectCall()
    {
        const string expression = "TextDecoder.prototype.decode.call({})";

        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }

    [Fact]
    public void ShouldHaveReadOnlyProperties()
    {
        Execute(
            """
            const decoder = new TextDecoder();
            const originalEncoding = decoder.encoding;
            const originalFatal = decoder.fatal;
            const originalIgnoreBOM = decoder.ignoreBOM;

            try { decoder.encoding = 'changed'; } catch {}
            try { decoder.fatal = !decoder.fatal; } catch {}
            try { decoder.ignoreBOM = !decoder.ignoreBOM; } catch {}
            """
        );

        Assert.Equal(
            Evaluate("originalEncoding").AsString(),
            Evaluate("decoder.encoding").AsString()
        );
        Assert.Equal(Evaluate("originalFatal").AsBoolean(), Evaluate("decoder.fatal").AsBoolean());
        Assert.Equal(
            Evaluate("originalIgnoreBOM").AsBoolean(),
            Evaluate("decoder.ignoreBOM").AsBoolean()
        );
    }
}
