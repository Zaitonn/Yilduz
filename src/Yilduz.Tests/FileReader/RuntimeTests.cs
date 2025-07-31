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
        Assert.True(Engine.Evaluate("typeof FileReader === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("FileReader.prototype").IsObject());
    }

    [Fact]
    public void ShouldCreateFileReaderInstance()
    {
        Engine.Execute(
            """
            const reader = new FileReader();
            """
        );

        Assert.True(Engine.Evaluate("reader instanceof FileReader").AsBoolean());
        Assert.True(Engine.Evaluate("reader instanceof EventTarget").AsBoolean());
    }

    [Fact]
    public void ShouldHaveReadyStateConstants()
    {
        Assert.Equal(0, Engine.Evaluate("FileReader.EMPTY").AsNumber());
        Assert.Equal(1, Engine.Evaluate("FileReader.LOADING").AsNumber());
        Assert.Equal(2, Engine.Evaluate("FileReader.DONE").AsNumber());
    }

    [Fact]
    public void ShouldHaveEmptyStateAfterCreation()
    {
        Engine.Execute(
            """
            const reader = new FileReader();
            """
        );

        Assert.Equal(0, Engine.Evaluate("reader.readyState").AsNumber());
        Assert.True(Engine.Evaluate("reader.result === null").AsBoolean());
        Assert.True(Engine.Evaluate("reader.error === null").AsBoolean());
    }

    [Fact]
    public async Task ShouldReadAsText()
    {
        Engine.Execute(
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

        Assert.True(Engine.Evaluate("reader.readyState === FileReader.DONE").AsBoolean());
        Assert.True(Engine.Evaluate("loadFired").AsBoolean());
    }

    [Fact]
    public async Task ShouldReadAsDataURL()
    {
        Engine.Execute(
            """
            const blob = new Blob(['Hello, World!'], { type: 'text/plain' });
            const reader = new FileReader();
            reader.readAsDataURL(blob);
            """
        );

        await Task.Delay(100);

        Assert.True(Engine.Evaluate("reader.readyState === FileReader.DONE").AsBoolean());
    }

    [Fact]
    public async Task ShouldReadAsArrayBuffer()
    {
        Engine.Execute(
            """
            const blob = new Blob(['Hello, World!'], { type: 'text/plain' });
            const reader = new FileReader();
            reader.readAsArrayBuffer(blob);
            """
        );

        await Task.Delay(100);

        Assert.True(Engine.Evaluate("reader.readyState === FileReader.DONE").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWithInvalidArguments()
    {
        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("const reader = new FileReader(); reader.readAsText();")
        );
        Assert.Throws<JavaScriptException>(
            () =>
                Engine.Execute("const reader = new FileReader(); reader.readAsText('not a blob');")
        );
    }

    [Fact]
    public void ShouldHaveCorrectInheritanceChain()
    {
        Engine.Execute(
            """
            const reader = new FileReader();
            """
        );

        Assert.True(Engine.Evaluate("reader instanceof FileReader").AsBoolean());
        Assert.True(Engine.Evaluate("reader instanceof EventTarget").AsBoolean());
        Assert.True(Engine.Evaluate("reader instanceof Object").AsBoolean());
    }

    [Fact]
    public async Task ShouldReadMultipleEncodings()
    {
        Engine.Execute(
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
        Assert.Equal("Hello UTF-8", Engine.Evaluate("reader1.result").AsString());
        Assert.Equal("Hello ASCII", Engine.Evaluate("reader2.result").AsString());
    }

    [Fact]
    public async Task ShouldReadAsDataURLWithMimeType()
    {
        Engine.Execute(
            """
            const blob = new Blob(['Hello DataURL'], { type: 'text/plain' });
            const reader = new FileReader();
            reader.readAsDataURL(blob);
            """
        );

        await Task.Delay(100);
        var result = Engine.Evaluate("reader.result").AsString();
        Assert.StartsWith("data:text/plain;base64,", result);
    }

    [Fact]
    public async Task ShouldReadAsArrayBufferAndReturnCorrectType()
    {
        Engine.Execute(
            """
            const blob = new Blob(['Hello ArrayBuffer']);
            const reader = new FileReader();
            reader.readAsArrayBuffer(blob);
            """
        );

        await Task.Delay(100);
        Assert.True(Engine.Evaluate("reader.result instanceof ArrayBuffer").AsBoolean());
        Assert.True(Engine.Evaluate("reader.result.byteLength > 0").AsBoolean());
    }

    [Fact]
    public void ShouldHandleEmptyBlob()
    {
        Engine.Execute(
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
        Engine.Execute(
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

        Assert.Equal(0, Engine.Evaluate("reader.readyState").AsNumber());
        Assert.True(Engine.Evaluate("reader.result === null").AsBoolean());
        Assert.True(Engine.Evaluate("reader.error === null").AsBoolean());
    }
}
