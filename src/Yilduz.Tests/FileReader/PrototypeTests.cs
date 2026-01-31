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
            Evaluate("Object.prototype.toString.call(new FileReader())").AsString()
        );
    }

    [Fact]
    public void ShouldHaveCorrectPrototypeChain()
    {
        Execute(
            """
            const reader = new FileReader();
            const fileReaderPrototype = Object.getPrototypeOf(reader);
            const eventTargetPrototype = Object.getPrototypeOf(fileReaderPrototype);
            const objectPrototype = Object.getPrototypeOf(eventTargetPrototype);
            """
        );

        Assert.Equal("FileReader", Evaluate("fileReaderPrototype.constructor.name").AsString());
        Assert.Equal("EventTarget", Evaluate("eventTargetPrototype.constructor.name").AsString());
        Assert.Equal("Object", Evaluate("objectPrototype.constructor.name").AsString());
    }

    [Fact]
    public void ShouldHaveReadOnlyProperties()
    {
        Execute(
            """
            const reader = new FileReader();
            const originalReadyState = reader.readyState;

            reader.readyState = 999;
            """
        );

        Assert.Equal(0, Evaluate("reader.readyState").AsNumber());
    }

    [Fact]
    public void ShouldHaveRequiredMethods()
    {
        Execute(
            """
            const reader = new FileReader();
            const hasReadAsArrayBuffer = typeof reader.readAsArrayBuffer === 'function';
            const hasReadAsText = typeof reader.readAsText === 'function';
            const hasReadAsDataURL = typeof reader.readAsDataURL === 'function';
            const hasAbort = typeof reader.abort === 'function';
            """
        );

        Assert.True(Evaluate("hasReadAsArrayBuffer").AsBoolean());
        Assert.True(Evaluate("hasReadAsText").AsBoolean());
        Assert.True(Evaluate("hasReadAsDataURL").AsBoolean());
        Assert.True(Evaluate("hasAbort").AsBoolean());
    }

    [Fact]
    public void ShouldHaveEventHandlerProperties()
    {
        Execute(
            """
            const reader = new FileReader();

            // Set event handlers
            let loadCalled = false;
            reader.onload = () => { loadCalled = true; };

            // Verify they're set
            const hasOnLoadHandler = typeof reader.onload === 'function';
            """
        );

        Assert.True(Evaluate("hasOnLoadHandler").AsBoolean());
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
        Assert.True(Evaluate($"FileReader.prototype.hasOwnProperty('{propertyName}')").AsBoolean());
    }
}
