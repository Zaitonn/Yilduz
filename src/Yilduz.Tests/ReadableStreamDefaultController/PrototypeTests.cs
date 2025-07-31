using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.ReadableStreamDefaultController;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("constructor")]
    [InlineData("desiredSize")]
    [InlineData("close")]
    [InlineData("enqueue")]
    [InlineData("error")]
    public void ReadableStreamDefaultControllerShouldHaveCorrectPrototype(string propertyName)
    {
        Assert.True(
            Engine
                .Evaluate(
                    $"ReadableStreamDefaultController.prototype.hasOwnProperty('{propertyName}')"
                )
                .AsBoolean()
        );
    }

    [Theory]
    [InlineData("ReadableStreamDefaultController.prototype.desiredSize")]
    [InlineData("ReadableStreamDefaultController.prototype.close()")]
    [InlineData("ReadableStreamDefaultController.prototype.enqueue()")]
    [InlineData("ReadableStreamDefaultController.prototype.error()")]
    public void ReadableStreamDefaultControllerShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate(expression));
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Engine.Execute(
            """
            let controllerToStringTag;
            const stream = new ReadableStream({
                start(controller) {
                    controllerToStringTag = controller.toString();
                }
            });
            """
        );
        Assert.Equal(
            "[object ReadableStreamDefaultController]",
            Engine.Evaluate("controllerToStringTag").AsString()
        );
    }

    [Fact]
    public void ShouldHaveCorrectConstructorName()
    {
        Assert.Equal(
            "ReadableStreamDefaultController",
            Engine.Evaluate("ReadableStreamDefaultController.name").AsString()
        );
    }

    [Fact]
    public void ShouldNotBeCallableAsFunction()
    {
        Assert.Throws<JavaScriptException>(
            () => Engine.Evaluate("ReadableStreamDefaultController()")
        );
    }

    [Fact]
    public void ShouldNotBeConstructableDirectly()
    {
        Assert.Throws<JavaScriptException>(
            () => Engine.Evaluate("new ReadableStreamDefaultController()")
        );
    }
}
