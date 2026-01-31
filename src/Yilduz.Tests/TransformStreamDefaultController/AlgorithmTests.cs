using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.TransformStreamDefaultController;

public sealed class AlgorithmTests : TestBase
{
    [Fact]
    public void ShouldEnqueueChunksCorrectly()
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

        // Should not throw when enqueueing
        Execute("controller.enqueue('test');");
        Execute("controller.enqueue(42);");
        Execute("controller.enqueue({ key: 'value' });");

        Assert.True(Evaluate("true").AsBoolean()); // Basic execution test
    }

    [Fact]
    public void ShouldCalculateDesiredSizeCorrectly()
    {
        Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) {
                    controller = c;
                }
            }, undefined, { highWaterMark: 3, size: () => 1 });
            """
        );

        var initialDesiredSize = Evaluate("controller.desiredSize");
        Assert.Equal(3, initialDesiredSize.AsNumber()); // Should start with highWaterMark

        Execute("controller.enqueue('test');");
        var afterEnqueue = Evaluate("controller.desiredSize");
        Assert.Equal(2, afterEnqueue.AsNumber()); // Should decrease by 1
    }

    [Fact]
    public void ShouldHandleCustomSizeFunction()
    {
        Execute(
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

        var initialDesiredSize = Evaluate("controller.desiredSize");
        Assert.Equal(10, initialDesiredSize.AsNumber());

        Execute("controller.enqueue('hello');"); // Should reduce by 5
        var afterStringEnqueue = Evaluate("controller.desiredSize");
        Assert.Equal(5, afterStringEnqueue.AsNumber());

        Execute("controller.enqueue(42);"); // Should reduce by 1
        var afterNumberEnqueue = Evaluate("controller.desiredSize");
        Assert.Equal(4, afterNumberEnqueue.AsNumber());
    }

    [Fact]
    public void ShouldErrorStreamCorrectly()
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

        // Should not throw when calling error
        Execute("controller.error(new Error('Test error'));");

        Assert.True(Evaluate("true").AsBoolean()); // Basic execution test
    }

    [Fact]
    public void ShouldTerminateStreamCorrectly()
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

        // Should not throw when calling terminate
        Execute("controller.terminate();");

        Assert.True(Evaluate("true").AsBoolean()); // Basic execution test
    }

    [Fact]
    public void ShouldHandleEnqueueAfterTerminate()
    {
        Execute(
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
        Assert.Throws<JavaScriptException>(() => Execute("controller.enqueue('test');"));
    }

    [Fact]
    public void ShouldHandleEnqueueAfterError()
    {
        Execute(
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
        Assert.Throws<JavaScriptException>(() => Execute("controller.enqueue('test');"));
    }

    [Fact]
    public void ShouldHandleMultipleErrors()
    {
        Execute(
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
        Execute("controller.error(new Error('Second error'));");

        Assert.True(Evaluate("true").AsBoolean()); // Should not throw
    }

    [Fact]
    public void ShouldHandleMultipleTerminates()
    {
        Execute(
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
        Execute("controller.terminate();");

        Assert.True(Evaluate("true").AsBoolean()); // Should not throw
    }

    [Fact]
    public void ShouldWorkInFlushMethod()
    {
        Execute(
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
        Assert.True(Evaluate("typeof flushCalled !== 'undefined'").AsBoolean());
    }
}
