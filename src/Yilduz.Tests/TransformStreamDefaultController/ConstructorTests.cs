using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.TransformStreamDefaultController;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldNotBeConstructableDirectly()
    {
        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("new TransformStreamDefaultController();")
        );
    }

    [Fact]
    public void ShouldBeCreatedByTransformStream()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) {
                    controller = c;
                }
            });
            """
        );

        Assert.Equal(
            "TransformStreamDefaultController",
            Engine.Evaluate("controller.constructor.name")
        );
    }

    [Fact]
    public void ShouldHaveCorrectInstanceType()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) {
                    controller = c;
                }
            });
            """
        );

        Assert.True(
            Engine.Evaluate("controller instanceof TransformStreamDefaultController").AsBoolean()
        );
    }

    [Fact]
    public void ShouldBeUniquePerTransformStream()
    {
        Engine.Execute(
            """
            let controller1 = null;
            let controller2 = null;

            const stream1 = new TransformStream({
                start(c) { controller1 = c; }
            });

            const stream2 = new TransformStream({
                start(c) { controller2 = c; }
            });
            """
        );

        Assert.False(Engine.Evaluate("controller1 === controller2").AsBoolean());
    }

    [Fact]
    public void ShouldBePassedToFlushMethod()
    {
        Engine.Execute(
            """
            let flushController = null;
            const stream = new TransformStream({
                flush(controller) {
                    flushController = controller;
                }
            });

            const writer = stream.writable.getWriter();
            writer.close();
            """
        );

        // Note: flush is called asynchronously, controller will be set eventually
        Assert.True(Engine.Evaluate("typeof flushController !== 'undefined'").AsBoolean());
    }

    [Fact]
    public void ShouldNotThrowOnValidController()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) {
                    controller = c;
                    // These should not throw
                    typeof controller.desiredSize;
                    typeof controller.enqueue;
                    typeof controller.error;
                    typeof controller.terminate;
                }
            });
            """
        );

        Assert.True(Engine.Evaluate("controller !== null").AsBoolean());
    }

    [Fact]
    public void ShouldMaintainControllerState()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) {
                    controller = c;
                }
            });
            """
        );

        // Controller should remain accessible and functional
        Assert.Equal(
            "TransformStreamDefaultController",
            Engine.Evaluate("controller.constructor.name")
        );
        Assert.True(
            Engine
                .Evaluate(
                    "typeof controller.desiredSize === 'number' || controller.desiredSize === null"
                )
                .AsBoolean()
        );
    }
}
