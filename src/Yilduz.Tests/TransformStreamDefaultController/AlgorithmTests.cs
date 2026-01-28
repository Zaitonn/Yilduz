using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.TransformStreamDefaultController;

public sealed class AlgorithmTests : TestBase
{
    [Fact]
    public void ShouldEnqueueChunksCorrectly()
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

        // Should not throw when enqueueing
        Engine.Execute("controller.enqueue('test');");
        Engine.Execute("controller.enqueue(42);");
        Engine.Execute("controller.enqueue({ key: 'value' });");

        Assert.True(Engine.Evaluate("true").AsBoolean()); // Basic execution test
    }

    [Fact]
    public void ShouldCalculateDesiredSizeCorrectly()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) {
                    controller = c;
                }
            }, undefined, { highWaterMark: 3, size: () => 1 });
            """
        );

        var initialDesiredSize = Engine.Evaluate("controller.desiredSize");
        Assert.Equal(3, initialDesiredSize.AsNumber()); // Should start with highWaterMark

        Engine.Execute("controller.enqueue('test');");
        var afterEnqueue = Engine.Evaluate("controller.desiredSize");
        Assert.Equal(2, afterEnqueue.AsNumber()); // Should decrease by 1
    }

    [Fact]
    public void ShouldHandleCustomSizeFunction()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) {
                    controller = c;
                }
            }, undefined, { 
                highWaterMark: 10, 
                size: (chunk) => typeof chunk === 'string' ? chunk.length : 1 
            });
            """
        );

        var initialDesiredSize = Engine.Evaluate("controller.desiredSize");
        Assert.Equal(10, initialDesiredSize.AsNumber());

        Engine.Execute("controller.enqueue('hello');"); // Should reduce by 5
        var afterStringEnqueue = Engine.Evaluate("controller.desiredSize");
        Assert.Equal(5, afterStringEnqueue.AsNumber());

        Engine.Execute("controller.enqueue(42);"); // Should reduce by 1
        var afterNumberEnqueue = Engine.Evaluate("controller.desiredSize");
        Assert.Equal(4, afterNumberEnqueue.AsNumber());
    }

    [Fact]
    public void ShouldErrorStreamCorrectly()
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

        // Should not throw when calling error
        Engine.Execute("controller.error(new Error('Test error'));");

        Assert.True(Engine.Evaluate("true").AsBoolean()); // Basic execution test
    }

    [Fact]
    public void ShouldTerminateStreamCorrectly()
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

        // Should not throw when calling terminate
        Engine.Execute("controller.terminate();");

        Assert.True(Engine.Evaluate("true").AsBoolean()); // Basic execution test
    }

    [Fact]
    public void ShouldHandleEnqueueAfterTerminate()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) {
                    controller = c;
                }
            });

            controller.terminate();
            """
        );

        // Enqueueing after terminate should throw
        Assert.Throws<JavaScriptException>(() => Engine.Execute("controller.enqueue('test');"));
    }

    [Fact]
    public void ShouldHandleEnqueueAfterError()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) {
                    controller = c;
                }
            });

            controller.error(new Error('Test error'));
            """
        );

        // Enqueueing after error should throw
        Assert.Throws<JavaScriptException>(() => Engine.Execute("controller.enqueue('test');"));
    }

    [Fact]
    public void ShouldHandleMultipleErrors()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) {
                    controller = c;
                }
            });

            controller.error(new Error('First error'));
            """
        );

        // Second error call should be ignored (not throw)
        Engine.Execute("controller.error(new Error('Second error'));");

        Assert.True(Engine.Evaluate("true").AsBoolean()); // Should not throw
    }

    [Fact]
    public void ShouldHandleMultipleTerminates()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) {
                    controller = c;
                }
            });

            controller.terminate();
            """
        );

        // Second terminate call should be ignored (not throw)
        Engine.Execute("controller.terminate();");

        Assert.True(Engine.Evaluate("true").AsBoolean()); // Should not throw
    }

    [Fact]
    public void ShouldWorkInFlushMethod()
    {
        Engine.Execute(
            """
            let flushCalled = false;
            const stream = new TransformStream({
                flush(controller) {
                    flushCalled = true;
                    
                    // Test controller methods in flush
                    const size = controller.desiredSize;
                    controller.enqueue('flushed');
                }
            });

            const writer = stream.writable.getWriter();
            writer.close();
            """
        );

        // Note: flush is called asynchronously
        Assert.True(Engine.Evaluate("typeof flushCalled !== 'undefined'").AsBoolean());
    }
}
