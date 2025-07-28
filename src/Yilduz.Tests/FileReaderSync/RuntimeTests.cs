using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.FileReaderSync;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldBeGlobalConstructor()
    {
        Assert.True(Engine.Evaluate("typeof FileReaderSync === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("FileReaderSync.prototype").IsObject());
    }

    [Fact]
    public void ShouldCreateFileReaderSyncInstance()
    {
        Engine.Execute(
            """
            const reader = new FileReaderSync();
            """
        );

        Assert.True(Engine.Evaluate("reader instanceof FileReaderSync").AsBoolean());
    }

    [Fact]
    public void ShouldReadTextFromBlob()
    {
        Engine.Execute(
            """
            const blob = new Blob(['Hello, World!'], { type: 'text/plain' });
            const reader = new FileReaderSync();
            const result = reader.readAsText(blob);
            """
        );

        Assert.Equal("Hello, World!", Engine.Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldReadArrayBufferFromBlob()
    {
        Engine.Execute(
            """
            const blob = new Blob(['Hello']);
            const reader = new FileReaderSync();
            const result = reader.readAsArrayBuffer(blob);
            """
        );

        Assert.True(Engine.Evaluate("result instanceof ArrayBuffer").AsBoolean());
        Assert.Equal(5, Engine.Evaluate("result.byteLength").AsNumber());
    }

    [Fact]
    public void ShouldReadDataURLFromBlob()
    {
        Engine.Execute(
            """
            const blob = new Blob(['Hello'], { type: 'text/plain' });
            const reader = new FileReaderSync();
            const result = reader.readAsDataURL(blob);
            """
        );

        var result = Engine.Evaluate("result").AsString();
        Assert.StartsWith("data:text/plain;base64,", result);
    }

    [Fact]
    public void ShouldHandleEmptyBlob()
    {
        Engine.Execute(
            """
            const blob = new Blob([]);
            const reader = new FileReaderSync();
            const result = reader.readAsText(blob);
            """
        );

        Assert.Equal("", Engine.Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldThrowErrorForInvalidBlobParameter()
    {
        Engine.Execute(
            """
            const reader = new FileReaderSync();
            """
        );

        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("reader.readAsText('not a blob');")
        );
    }

    [Fact]
    public void ShouldHandleCustomEncoding()
    {
        Engine.Execute(
            """
            const blob = new Blob(['Hello, 世界!'], { type: 'text/plain' });
            const reader = new FileReaderSync();
            const result = reader.readAsText(blob, 'UTF-8');
            """
        );

        Assert.Equal("Hello, 世界!", Engine.Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldReadBinaryStringFromBlob()
    {
        Engine.Execute(
            """
            const blob = new Blob([new Uint8Array([72, 101, 108, 108, 111])], { type: 'application/octet-stream' });
            const reader = new FileReaderSync();
            const result = reader.readAsBinaryString(blob);
            """
        );

        Assert.Equal("Hello", Engine.Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldHandleBinaryDataCorrectly()
    {
        Engine.Execute(
            """
            // Create a blob with binary data including null bytes
            const blob = new Blob([new Uint8Array([0, 255, 128, 65, 0])]);
            const reader = new FileReaderSync();
            const result = reader.readAsBinaryString(blob);
            """
        );

        var result = Engine.Evaluate("result").AsString();
        Assert.Equal(5, result.Length);
        Assert.Equal('\0', result[0]); // null byte
        Assert.Equal('\u00FF', result[1]); // 255
        Assert.Equal('\u0080', result[2]); // 128
        Assert.Equal('A', result[3]); // 65 = 'A'
        Assert.Equal('\0', result[4]); // null byte
    }
}
