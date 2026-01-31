using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultController;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("constructor")]
    [InlineData("error")]
    public void ShouldHaveCorrectPrototypeProperties(string propertyName)
    {
        Assert.True(
            Engine
                .Evaluate(
                    $"WritableStreamDefaultController.prototype.hasOwnProperty('{propertyName}')"
                )
                .AsBoolean()
        );
    }

    [Theory]
    [InlineData("WritableStreamDefaultController.prototype.error()")]
    public void ShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }

    [Fact]
    public void ShouldHaveCorrectConstructorName()
    {
        Assert.Equal(
            "WritableStreamDefaultController",
            Evaluate("WritableStreamDefaultController.name").AsString()
        );
    }

    [Fact]
    public void ShouldHaveCorrectPrototypeChain()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });
            """
        );

        Assert.True(
            Engine
                .Evaluate(
                    "Object.getPrototypeOf(controller) === WritableStreamDefaultController.prototype"
                )
                .AsBoolean()
        );
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });
            """
        );

        Assert.Equal(
            "WritableStreamDefaultController",
            Engine
                .Evaluate("Object.prototype.toString.call(controller)")
                .AsString()
                .Replace("[object ", "")
                .Replace("]", "")
        );
    }

    [Fact]
    public void ShouldNotBeEnumerable()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });
            const keys = Object.keys(controller);
            """
        );

        Assert.Equal(0, Evaluate("keys.length").AsNumber());
    }

    [Fact]
    public void ShouldHaveNonEnumerableProperties()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });
            """
        );

        // Check that signal property is non-enumerable
        Assert.False(
            Evaluate("Object.propertyIsEnumerable.call(controller, 'signal')").AsBoolean()
        );
    }

    [Fact]
    public void ShouldHaveCorrectMethodNames()
    {
        Assert.Equal(
            "error",
            Evaluate("WritableStreamDefaultController.prototype.error.name").AsString()
        );
    }

    [Fact]
    public void ShouldNotAllowDirectConstruction()
    {
        Assert.Throws<JavaScriptException>(() => Execute("new WritableStreamDefaultController();"));
        Assert.Throws<JavaScriptException>(
            () => Execute("new WritableStreamDefaultController({});")
        );
    }

    [Fact]
    public void ShouldHaveConsistentInterface()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });
            """
        );

        // Controller should have expected interface
        Assert.True(Evaluate("typeof controller.error === 'function'").AsBoolean());
        Assert.True(Evaluate("'signal' in controller").AsBoolean());
        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }
}
