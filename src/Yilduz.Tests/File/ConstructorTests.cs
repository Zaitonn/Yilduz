using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.File;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldThrowWithInvalidArguments()
    {
        Assert.Throws<JavaScriptException>(() => Execute("new File();"));
        Assert.Throws<JavaScriptException>(() => Execute("new File([]);"));
        Assert.Throws<JavaScriptException>(
            () => Execute("new File('not an array', 'filename.txt');")
        );
        Assert.Throws<JavaScriptException>(
            () => Execute("new File([], 'filename.txt', 'not an object');")
        );
    }

    [Fact]
    public void ShouldCreateFileWithDefaults()
    {
        Execute(
            """
            const file = new File([], 'test.txt');
            """
        );

        Assert.True(Evaluate("file instanceof File").AsBoolean());
        Assert.Equal("test.txt", Evaluate("file.name").AsString());
        Assert.Equal("", Evaluate("file.type").AsString());
        Assert.Equal(0, Evaluate("file.size").AsNumber());
        Assert.True(Evaluate("typeof file.lastModified === 'number'").AsBoolean());
    }

    [Fact]
    public void ShouldCreateFileWithContentAndOptions()
    {
        Execute(
            """
            const content = 'Hello, World!';
            const file = new File([content], 'test.txt', { 
                type: 'text/plain',
                lastModified: 12345
            });
            """
        );

        Assert.Equal("test.txt", Evaluate("file.name").AsString());
        Assert.Equal("text/plain", Evaluate("file.type").AsString());
        Assert.Equal(13, Evaluate("file.size").AsNumber());
        Assert.Equal(12345, Evaluate("file.lastModified").AsNumber());
    }

    [Fact]
    public void ShouldCreateFileWithMultipleParts()
    {
        Execute(
            """
            const file = new File(['Hello', ', ', 'World!'], 'multi.txt');
            """
        );

        Assert.Equal(13, Evaluate("file.size").AsNumber());
    }

    [Fact]
    public void ShouldRequireMinimumArguments()
    {
        Assert.Throws<JavaScriptException>(() => Execute("new File();"));
        Assert.Throws<JavaScriptException>(() => Execute("new File([]);"));
    }

    [Fact]
    public void ShouldHandleEmptyFileName()
    {
        Execute(
            """
            const file = new File([], '');
            """
        );

        Assert.Equal("", Evaluate("file.name").AsString());
    }

    [Fact]
    public void ShouldInheritFromBlob()
    {
        Execute(
            """
            const file = new File([], 'test.txt');
            const isBlob = file instanceof Blob;
            const hasSlice = typeof file.slice === 'function';
            """
        );

        Assert.True(Evaluate("isBlob").AsBoolean());
        Assert.True(Evaluate("hasSlice").AsBoolean());
    }
}
