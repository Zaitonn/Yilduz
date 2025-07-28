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
        Engine.Execute("const decoder = new TextDecoder();");

        Assert.True(Engine.Evaluate($"'{propertyName}' in decoder").AsBoolean());
    }

    [Fact]
    public void ShouldHaveDecodeMethod()
    {
        Engine.Execute("const decoder = new TextDecoder();");

        Assert.True(Engine.Evaluate("typeof decoder.decode === 'function'").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectMethodNames()
    {
        Engine.Execute("const decoder = new TextDecoder();");
        var methodName = nameof(Encoding.TextDecoder.TextDecoderInstance.Decode).ToJsStyleName();

        Assert.Equal("decode", Engine.Evaluate("decoder.decode.name").AsString());
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Engine.Execute("const decoder = new TextDecoder();");

        Assert.Equal(
            "[object TextDecoder]",
            Engine.Evaluate("Object.prototype.toString.call(decoder)").AsString()
        );
    }

    [Fact]
    public void ShouldNotAllowDirectCall()
    {
        const string expression = "TextDecoder.prototype.decode.call({})";

        Assert.Throws<JavaScriptException>(() => Engine.Evaluate(expression));
    }

    [Fact]
    public void ShouldHaveReadOnlyProperties()
    {
        Engine.Execute(
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
            Engine.Evaluate("originalEncoding").AsString(),
            Engine.Evaluate("decoder.encoding").AsString()
        );
        Assert.Equal(
            Engine.Evaluate("originalFatal").AsBoolean(),
            Engine.Evaluate("decoder.fatal").AsBoolean()
        );
        Assert.Equal(
            Engine.Evaluate("originalIgnoreBOM").AsBoolean(),
            Engine.Evaluate("decoder.ignoreBOM").AsBoolean()
        );
    }
}
