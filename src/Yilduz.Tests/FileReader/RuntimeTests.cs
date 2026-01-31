using System.Threading.Tasks;
using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.FileReader;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldBeGlobalConstructor()
    {
        Assert.True(Evaluate("typeof FileReader === 'function'").AsBoolean());
        Assert.True(Evaluate("FileReader.prototype").IsObject());
    }

    [Fact]
    public void ShouldCreateFileReaderInstance()
    {
        Execute(
            """
            const reader = new FileReader();
            """
        );

        Assert.True(Evaluate("reader instanceof FileReader").AsBoolean());
        Assert.True(Evaluate("reader instanceof EventTarget").AsBoolean());
    }

    [Fact]
    public void ShouldHaveReadyStateConstants()
    {
        Assert.Equal(0, Evaluate("FileReader.EMPTY").AsNumber());
        Assert.Equal(1, Evaluate("FileReader.LOADING").AsNumber());
        Assert.Equal(2, Evaluate("FileReader.DONE").AsNumber());
    }

    [Fact]
    public void ShouldHaveEmptyStateAfterCreation()
    {
        Execute(
            """
            const reader = new FileReader();
            """
        );

        Assert.Equal(0, Evaluate("reader.readyState").AsNumber());
        Assert.True(Evaluate("reader.result === null").AsBoolean());
        Assert.True(Evaluate("reader.error === null").AsBoolean());
    }

    [Fact]
    public async Task ShouldReadAsText()
    {
        Execute(
            """
            const blob = new Blob(['Hello, World!'], { type: 'text/plain' });
            const reader = new FileReader();

            // Setup event handlers
            let loadFired = false;
            reader.addEventListener('load', () => {
                loadFired = true;
            });

            reader.readAsText(blob);
            """
        );

        await Task.Delay(100);

        Assert.True(Evaluate("reader.readyState === FileReader.DONE").AsBoolean());
        Assert.True(Evaluate("loadFired").AsBoolean());
    }

    [Fact]
    public async Task ShouldReadAsDataURL()
    {
        Execute(
            """
            const blob = new Blob(['Hello, World!'], { type: 'text/plain' });
            const reader = new FileReader();
            reader.readAsDataURL(blob);
            """
        );

        await Task.Delay(100);

        Assert.True(Evaluate("reader.readyState === FileReader.DONE").AsBoolean());
    }

    [Fact]
    public async Task ShouldReadAsArrayBuffer()
    {
        Execute(
            """
            const blob = new Blob(['Hello, World!'], { type: 'text/plain' });
            const reader = new FileReader();
            reader.readAsArrayBuffer(blob);
            """
        );

        await Task.Delay(100);

        Assert.True(Evaluate("reader.readyState === FileReader.DONE").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWithInvalidArguments()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("const reader = new FileReader(); reader.readAsText();")
        );
        Assert.Throws<JavaScriptException>(
            () => Execute("const reader = new FileReader(); reader.readAsText('not a blob');")
        );
    }

    [Fact]
    public void ShouldHaveCorrectInheritanceChain()
    {
        Execute(
            """
            const reader = new FileReader();
            """
        );

        Assert.True(Evaluate("reader instanceof FileReader").AsBoolean());
        Assert.True(Evaluate("reader instanceof EventTarget").AsBoolean());
        Assert.True(Evaluate("reader instanceof Object").AsBoolean());
    }

    [Fact]
    public async Task ShouldReadMultipleEncodings()
    {
        Execute(
            """
            const utf8Blob = new Blob(['Hello UTF-8'], { type: 'text/plain;charset=utf-8' });
            const reader1 = new FileReader();
            reader1.readAsText(utf8Blob, 'utf-8');

            const asciiBlob = new Blob(['Hello ASCII'], { type: 'text/plain;charset=ascii' });
            const reader2 = new FileReader();
            reader2.readAsText(asciiBlob, 'ascii');
            """
        );

        await Task.Delay(100);
        Assert.Equal("Hello UTF-8", Evaluate("reader1.result").AsString());
        Assert.Equal("Hello ASCII", Evaluate("reader2.result").AsString());
    }

    [Fact]
    public async Task ShouldReadAsDataURLWithMimeType()
    {
        Execute(
            """
            const blob = new Blob(['Hello DataURL'], { type: 'text/plain' });
            const reader = new FileReader();
            reader.readAsDataURL(blob);
            """
        );

        await Task.Delay(100);
        var result = Evaluate("reader.result").AsString();
        Assert.StartsWith("data:text/plain;base64,", result);
    }

    [Fact]
    public async Task ShouldReadAsArrayBufferAndReturnCorrectType()
    {
        Execute(
            """
            const blob = new Blob(['Hello ArrayBuffer']);
            const reader = new FileReader();
            reader.readAsArrayBuffer(blob);
            """
        );

        await Task.Delay(100);
        Assert.True(Evaluate("reader.result instanceof ArrayBuffer").AsBoolean());
        Assert.True(Evaluate("reader.result.byteLength > 0").AsBoolean());
    }

    [Fact]
    public void ShouldHandleEmptyBlob()
    {
        Execute(
            """
            const emptyBlob = new Blob([]);
            const reader = new FileReader();
            reader.readAsText(emptyBlob);
            """
        );

        Assert.True(
            Engine
                .Evaluate(
                    "reader.readyState === FileReader.LOADING || reader.readyState === FileReader.DONE"
                )
                .AsBoolean()
        );
    }

    [Fact]
    public void ShouldHaveReadOnlyProperties()
    {
        Execute(
            """
            const reader = new FileReader();
            const originalReadyState = reader.readyState;
            const originalResult = reader.result;
            const originalError = reader.error;

            reader.readyState = 999;
            reader.result = 'modified';
            reader.error = 'modified';
            """
        );

        Assert.Equal(0, Evaluate("reader.readyState").AsNumber());
        Assert.True(Evaluate("reader.result === null").AsBoolean());
        Assert.True(Evaluate("reader.error === null").AsBoolean());
    }
}
