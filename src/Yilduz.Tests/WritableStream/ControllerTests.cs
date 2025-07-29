using Jint;
using Jint.Runtime;
using Xunit;
using Yilduz.Streams.WritableStreamDefaultController;

namespace Yilduz.Tests.WritableStream;

public sealed class ControllerTests : TestBase
{
    [Fact]
    public void ShouldBeGlobalConstructor()
    {
        Assert.True(
            Engine.Evaluate("typeof WritableStreamDefaultController === 'function'").AsBoolean()
        );
        Assert.True(Engine.Evaluate("WritableStreamDefaultController.prototype").IsObject());
    }

    [Fact]
    public void ShouldNotBeConstructible()
    {
        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("new WritableStreamDefaultController();")
        );
    }

    [Fact]
    public void ShouldBePassedToUnderlyingSinkStart()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });
            """
        );

        Assert.IsType<WritableStreamDefaultControllerInstance>(Engine.Evaluate("controller"));
        Assert.True(
            Engine.Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldHaveErrorMethod()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });
            """
        );

        Assert.True(Engine.Evaluate("typeof controller.error === 'function'").AsBoolean());
    }

    [Fact]
    public void ShouldCallErrorMethod()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });
            controller.error('Test error');
            """
        );

        // The error method should execute without throwing
        Assert.True(true);
    }

    [Fact]
    public void ShouldHaveToStringTag()
    {
        Engine.Execute(
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
}
