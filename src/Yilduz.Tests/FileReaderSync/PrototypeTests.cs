using Jint;
using Xunit;

namespace Yilduz.Tests.FileReaderSync;

public sealed class PrototypeTests : TestBase
{
    [Fact]
    public void ShouldHaveToStringTag()
    {
        Assert.Equal(
            "[object FileReaderSync]",
            Evaluate("Object.prototype.toString.call(new FileReaderSync())").AsString()
        );
    }

    [Fact]
    public void ShouldHaveCorrectPrototypeChain()
    {
        Execute(
            """
            const reader = new FileReaderSync();
            const fileReaderSyncPrototype = Object.getPrototypeOf(reader);
            const objectPrototype = Object.getPrototypeOf(fileReaderSyncPrototype);
            """
        );

        Assert.Equal(
            "FileReaderSync",
            Evaluate("fileReaderSyncPrototype.constructor.name").AsString()
        );
        Assert.Equal("Object", Evaluate("objectPrototype.constructor.name").AsString());
    }

    [Fact]
    public void ShouldHaveRequiredMethods()
    {
        Execute(
            """
            const reader = new FileReaderSync();
            """
        );

        Assert.True(Evaluate("typeof reader.readAsArrayBuffer === 'function'").AsBoolean());
        Assert.True(Evaluate("typeof reader.readAsText === 'function'").AsBoolean());
        Assert.True(Evaluate("typeof reader.readAsDataURL === 'function'").AsBoolean());
        Assert.True(Evaluate("typeof reader.readAsBinaryString === 'function'").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectMethodNames()
    {
        Execute(
            """
            const reader = new FileReaderSync();
            """
        );

        Assert.Equal("readAsArrayBuffer", Evaluate("reader.readAsArrayBuffer.name").AsString());
        Assert.Equal("readAsText", Evaluate("reader.readAsText.name").AsString());
        Assert.Equal("readAsDataURL", Evaluate("reader.readAsDataURL.name").AsString());
        Assert.Equal("readAsBinaryString", Evaluate("reader.readAsBinaryString.name").AsString());
    }

    [Fact]
    public void ShouldNotInheritEventTargetMethods()
    {
        Execute(
            """
            const reader = new FileReaderSync();
            """
        );

        Assert.False(Evaluate("reader instanceof EventTarget").AsBoolean());
        Assert.True(Evaluate("typeof reader.addEventListener === 'undefined'").AsBoolean());
        Assert.True(Evaluate("typeof reader.removeEventListener === 'undefined'").AsBoolean());
        Assert.True(Evaluate("typeof reader.dispatchEvent === 'undefined'").AsBoolean());
    }
}
