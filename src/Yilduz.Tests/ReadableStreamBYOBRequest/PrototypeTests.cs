using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.ReadableStreamBYOBRequest;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("constructor")]
    [InlineData("view")]
    [InlineData("respond")]
    [InlineData("respondWithNewView")]
    public void ReadableStreamBYOBRequestShouldHaveCorrectPrototype(string propertyName)
    {
        Assert.True(
            Engine
                .Evaluate($"ReadableStreamBYOBRequest.prototype.hasOwnProperty('{propertyName}')")
                .AsBoolean()
        );
    }

    [Theory]
    [InlineData("ReadableStreamBYOBRequest.prototype.view")]
    [InlineData("ReadableStreamBYOBRequest.prototype.respond(0)")]
    [InlineData("ReadableStreamBYOBRequest.prototype.respondWithNewView(new Uint8Array(0))")]
    public void ReadableStreamBYOBRequestShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }

    [Fact]
    public void ShouldHaveCorrectConstructorName()
    {
        Assert.Equal(
            "ReadableStreamBYOBRequest",
            Evaluate("ReadableStreamBYOBRequest.name").AsString()
        );
    }

    [Fact]
    public void ShouldNotBeCallableAsFunction()
    {
        Assert.Throws<JavaScriptException>(() => Evaluate("ReadableStreamBYOBRequest()"));
    }

    [Fact]
    public void ShouldNotBeConstructableDirectly()
    {
        Assert.Throws<JavaScriptException>(() => Evaluate("new ReadableStreamBYOBRequest()"));
    }
}
