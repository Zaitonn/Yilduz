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
            Engine.Evaluate("Object.prototype.toString.call(new FileReaderSync())").AsString()
        );
    }

    [Fact]
    public void ShouldHaveCorrectPrototypeChain()
    {
        Engine.Execute(
            """
            const reader = new FileReaderSync();
            const fileReaderSyncPrototype = Object.getPrototypeOf(reader);
            const objectPrototype = Object.getPrototypeOf(fileReaderSyncPrototype);
            """
        );

        Assert.Equal(
            "FileReaderSync",
            Engine.Evaluate("fileReaderSyncPrototype.constructor.name").AsString()
        );
        Assert.Equal("Object", Engine.Evaluate("objectPrototype.constructor.name").AsString());
    }

    [Fact]
    public void ShouldHaveRequiredMethods()
    {
        Engine.Execute(
            """
            const reader = new FileReaderSync();
            """
        );

        Assert.True(Engine.Evaluate("typeof reader.readAsArrayBuffer === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("typeof reader.readAsText === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("typeof reader.readAsDataURL === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("typeof reader.readAsBinaryString === 'function'").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectMethodNames()
    {
        Engine.Execute(
            """
            const reader = new FileReaderSync();
            """
        );

        Assert.Equal(
            "readAsArrayBuffer",
            Engine.Evaluate("reader.readAsArrayBuffer.name").AsString()
        );
        Assert.Equal("readAsText", Engine.Evaluate("reader.readAsText.name").AsString());
        Assert.Equal("readAsDataURL", Engine.Evaluate("reader.readAsDataURL.name").AsString());
        Assert.Equal(
            "readAsBinaryString",
            Engine.Evaluate("reader.readAsBinaryString.name").AsString()
        );
    }

    [Fact]
    public void ShouldNotInheritEventTargetMethods()
    {
        Engine.Execute(
            """
            const reader = new FileReaderSync();
            """
        );

        Assert.False(Engine.Evaluate("reader instanceof EventTarget").AsBoolean());
        Assert.True(Engine.Evaluate("typeof reader.addEventListener === 'undefined'").AsBoolean());
        Assert.True(
            Engine.Evaluate("typeof reader.removeEventListener === 'undefined'").AsBoolean()
        );
        Assert.True(Engine.Evaluate("typeof reader.dispatchEvent === 'undefined'").AsBoolean());
    }
}
