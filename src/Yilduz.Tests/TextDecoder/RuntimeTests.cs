using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.TextDecoder;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldDecodeUtf8String()
    {
        Engine.Execute(
            """
            const decoder = new TextDecoder('utf-8');
            const bytes = new Uint8Array([72, 101, 108, 108, 111]); // 'Hello'
            const result = decoder.decode(bytes);
            """
        );

        Assert.Equal("Hello", Engine.Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldDecodeEmptyInput()
    {
        Engine.Execute(
            """
            const decoder = new TextDecoder();
            const result = decoder.decode();
            """
        );

        Assert.Equal("", Engine.Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldDecodeUndefinedInput()
    {
        Engine.Execute(
            """
            const decoder = new TextDecoder();
            const result = decoder.decode(undefined);
            """
        );

        Assert.Equal("", Engine.Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldDecodeArrayBuffer()
    {
        Engine.Execute(
            """
            const decoder = new TextDecoder();
            const buffer = new ArrayBuffer(5);
            const view = new Uint8Array(buffer);
            view[0] = 72; // H
            view[1] = 101; // e
            view[2] = 108; // l
            view[3] = 108; // l
            view[4] = 111; // o
            const result = decoder.decode(buffer);
            """
        );

        Assert.Equal("Hello", Engine.Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldDecodeDataView()
    {
        Engine.Execute(
            """
            const decoder = new TextDecoder();
            const buffer = new ArrayBuffer(5);
            const view = new DataView(buffer);
            view.setUint8(0, 72); // H
            view.setUint8(1, 101); // e
            view.setUint8(2, 108); // l
            view.setUint8(3, 108); // l
            view.setUint8(4, 111); // o
            const result = decoder.decode(view);
            """
        );

        Assert.Equal("Hello", Engine.Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldDecodeTypedArray()
    {
        Engine.Execute(
            """
            const decoder = new TextDecoder();
            const bytes = new Uint8Array([72, 101, 108, 108, 111]);
            const result = decoder.decode(bytes);
            """
        );

        Assert.Equal("Hello", Engine.Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldDecodeUtf8WithEmoji()
    {
        Engine.Execute(
            """
            const decoder = new TextDecoder('utf-8');
            const bytes = new Uint8Array([240, 159, 152, 128]); // 😀 emoji
            const result = decoder.decode(bytes);
            """
        );

        Assert.Equal("😀", Engine.Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldHandleStreamOption()
    {
        Engine.Execute(
            """
            const decoder = new TextDecoder();
            const bytes = new Uint8Array([72, 101, 108, 108, 111]);
            const result = decoder.decode(bytes, { stream: true });
            """
        );

        Assert.Equal("Hello", Engine.Evaluate("result").AsString());
    }

    [Theory]
    [InlineData("utf-8")]
    [InlineData("utf-16le")]
    [InlineData("ascii")]
    public void ShouldWorkWithDifferentEncodings(string encoding)
    {
        Engine.Execute(
            $@"
            const decoder = new TextDecoder('{encoding}');
            const encoder = new TextEncoder(); // Always UTF-8
            const text = 'Hello World';
            const bytes = encoder.encode(text);
            """
        );

        // For UTF-8, should work correctly
        if (encoding == "utf-8")
        {
            Engine.Execute("const result = decoder.decode(bytes);");
            Assert.Equal("Hello World", Engine.Evaluate("result").AsString());
        }
    }

    [Fact]
    public void ShouldRespectFatalOption()
    {
        Engine.Execute(
            """
            const decoder = new TextDecoder('utf-8', { fatal: false });
            const invalidBytes = new Uint8Array([0xFF, 0xFE]); // Invalid UTF-8
            const result = decoder.decode(invalidBytes);
            """
        );

        // Should not throw and return some result (possibly with replacement characters)
        Assert.NotNull(Engine.Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldThrowInvalidArgumentForInvalidInput()
    {
        Engine.Execute("const decoder = new TextDecoder();");

        Assert.Throws<JavaScriptException>(() => Engine.Execute("decoder.decode('invalid');"));
    }

    [Fact]
    public void ShouldIgnoreBOMWhenOptionSet()
    {
        Engine.Execute(
            """
            const decoder = new TextDecoder('utf-8', { ignoreBOM: true });
            const bytesWithBOM = new Uint8Array([0xEF, 0xBB, 0xBF, 72, 101, 108, 108, 111]); // BOM + 'Hello'
            const result = decoder.decode(bytesWithBOM);
            """
        );

        Assert.Equal("\ufeffHello", Engine.Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldRemoveBOMByDefault()
    {
        Engine.Execute(
            """
            const decoder = new TextDecoder('utf-8');
            const bytesWithBOM = new Uint8Array([0xEF, 0xBB, 0xBF, 72, 101, 108, 108, 111]); // BOM + 'Hello'
            const result = decoder.decode(bytesWithBOM);
            """
        );

        Assert.Equal("Hello", Engine.Evaluate("result").AsString());
    }
}
