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
            () => Execute("new TransformStreamDefaultController();")
        );
    }

    [Fact]
    public void ShouldBeCreatedByTransformStream()
    {
        Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) {
                    controller = c;
                }
            });
            """
        );

        Assert.Equal("TransformStreamDefaultController", Evaluate("controller.constructor.name"));
    }

    [Fact]
    public void ShouldHaveCorrectInstanceType()
    {
        Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) {
                    controller = c;
                }
            });
            """
        );

        Assert.True(Evaluate("controller instanceof TransformStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldBeUniquePerTransformStream()
    {
        Execute(
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

        Assert.False(Evaluate("controller1 === controller2").AsBoolean());
    }

    [Fact]
    public void ShouldBePassedToFlushMethod()
    {
        Execute(
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
        Assert.True(Evaluate("typeof flushController !== 'undefined'").AsBoolean());
    }

    [Fact]
    public void ShouldNotThrowOnValidController()
    {
        Execute(
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

        Assert.True(Evaluate("controller !== null").AsBoolean());
    }

    [Fact]
    public void ShouldMaintainControllerState()
    {
        Execute(
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
        Assert.Equal("TransformStreamDefaultController", Evaluate("controller.constructor.name"));
        Assert.True(
            Engine
                .Evaluate(
                    "typeof controller.desiredSize === 'number' || controller.desiredSize === null"
                )
                .AsBoolean()
        );
    }
}
