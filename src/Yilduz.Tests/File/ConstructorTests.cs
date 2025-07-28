using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.File;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldThrowWithInvalidArguments()
    {
        Assert.Throws<JavaScriptException>(() => Engine.Execute("new File();"));
        Assert.Throws<JavaScriptException>(() => Engine.Execute("new File([]);"));
        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("new File('not an array', 'filename.txt');")
        );
        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("new File([], 'filename.txt', 'not an object');")
        );
    }

    [Fact]
    public void ShouldCreateFileWithDefaults()
    {
        Engine.Execute(
            """
            const file = new File([], 'test.txt');
            """
        );

        Assert.True(Engine.Evaluate("file instanceof File").AsBoolean());
        Assert.Equal("test.txt", Engine.Evaluate("file.name").AsString());
        Assert.Equal("", Engine.Evaluate("file.type").AsString());
        Assert.Equal(0, Engine.Evaluate("file.size").AsNumber());
        Assert.True(Engine.Evaluate("typeof file.lastModified === 'number'").AsBoolean());
    }

    [Fact]
    public void ShouldCreateFileWithContentAndOptions()
    {
        Engine.Execute(
            """
            const content = 'Hello, World!';
            const file = new File([content], 'test.txt', { 
                type: 'text/plain',
                lastModified: 12345
            });
            """
        );

        Assert.Equal("test.txt", Engine.Evaluate("file.name").AsString());
        Assert.Equal("text/plain", Engine.Evaluate("file.type").AsString());
        Assert.Equal(13, Engine.Evaluate("file.size").AsNumber());
        Assert.Equal(12345, Engine.Evaluate("file.lastModified").AsNumber());
    }

    [Fact]
    public void ShouldCreateFileWithMultipleParts()
    {
        Engine.Execute(
            """
            const file = new File(['Hello', ', ', 'World!'], 'multi.txt');
            """
        );

        Assert.Equal(13, Engine.Evaluate("file.size").AsNumber());
    }

    [Fact]
    public void ShouldRequireMinimumArguments()
    {
        Assert.Throws<JavaScriptException>(() => Engine.Execute("new File();"));
        Assert.Throws<JavaScriptException>(() => Engine.Execute("new File([]);"));
    }

    [Fact]
    public void ShouldHandleEmptyFileName()
    {
        Engine.Execute(
            """
            const file = new File([], '');
            """
        );

        Assert.Equal("", Engine.Evaluate("file.name").AsString());
    }

    [Fact]
    public void ShouldInheritFromBlob()
    {
        Engine.Execute(
            """
            const file = new File([], 'test.txt');
            const isBlob = file instanceof Blob;
            const hasSlice = typeof file.slice === 'function';
            """
        );

        Assert.True(Engine.Evaluate("isBlob").AsBoolean());
        Assert.True(Engine.Evaluate("hasSlice").AsBoolean());
    }
}
