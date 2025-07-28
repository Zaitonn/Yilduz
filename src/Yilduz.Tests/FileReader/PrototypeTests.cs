using Jint;
using Xunit;

namespace Yilduz.Tests.FileReader;

public sealed class PrototypeTests : TestBase
{
    [Fact]
    public void ShouldHaveToStringTag()
    {
        Assert.Equal(
            "[object FileReader]",
            Engine.Evaluate("Object.prototype.toString.call(new FileReader())").AsString()
        );
    }

    [Fact]
    public void ShouldHaveCorrectPrototypeChain()
    {
        Engine.Execute(
            """
            const reader = new FileReader();
            const fileReaderPrototype = Object.getPrototypeOf(reader);
            const eventTargetPrototype = Object.getPrototypeOf(fileReaderPrototype);
            const objectPrototype = Object.getPrototypeOf(eventTargetPrototype);
            """
        );

        Assert.Equal(
            "FileReader",
            Engine.Evaluate("fileReaderPrototype.constructor.name").AsString()
        );
        Assert.Equal(
            "EventTarget",
            Engine.Evaluate("eventTargetPrototype.constructor.name").AsString()
        );
        Assert.Equal("Object", Engine.Evaluate("objectPrototype.constructor.name").AsString());
    }

    [Fact]
    public void ShouldHaveReadOnlyProperties()
    {
        Engine.Execute(
            """
            const reader = new FileReader();
            const originalReadyState = reader.readyState;

            reader.readyState = 999;
            """
        );

        Assert.Equal(0, Engine.Evaluate("reader.readyState").AsNumber());
    }

    [Fact]
    public void ShouldHaveRequiredMethods()
    {
        Engine.Execute(
            """
            const reader = new FileReader();
            const hasReadAsArrayBuffer = typeof reader.readAsArrayBuffer === 'function';
            const hasReadAsText = typeof reader.readAsText === 'function';
            const hasReadAsDataURL = typeof reader.readAsDataURL === 'function';
            const hasAbort = typeof reader.abort === 'function';
            """
        );

        Assert.True(Engine.Evaluate("hasReadAsArrayBuffer").AsBoolean());
        Assert.True(Engine.Evaluate("hasReadAsText").AsBoolean());
        Assert.True(Engine.Evaluate("hasReadAsDataURL").AsBoolean());
        Assert.True(Engine.Evaluate("hasAbort").AsBoolean());
    }

    [Fact]
    public void ShouldHaveEventHandlerProperties()
    {
        Engine.Execute(
            """
            const reader = new FileReader();

            // Set event handlers
            let loadCalled = false;
            reader.onload = () => { loadCalled = true; };

            // Verify they're set
            const hasOnLoadHandler = typeof reader.onload === 'function';
            """
        );

        Assert.True(Engine.Evaluate("hasOnLoadHandler").AsBoolean());
    }

    [Theory]
    [InlineData("onloadstart")]
    [InlineData("onprogress")]
    [InlineData("onload")]
    [InlineData("onloadend")]
    [InlineData("onerror")]
    [InlineData("onabort")]
    [InlineData("readAsArrayBuffer")]
    [InlineData("readAsText")]
    [InlineData("readAsDataURL")]
    [InlineData("abort")]
    [InlineData("error")]
    [InlineData("result")]
    [InlineData("readyState")]
    public void ShouldHaveCorrectPrototype(string propertyName)
    {
        Assert.True(
            Engine.Evaluate($"FileReader.prototype.hasOwnProperty('{propertyName}')").AsBoolean()
        );
    }
}
