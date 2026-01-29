using System.Threading.Tasks;
using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultController;

public sealed class ErrorMethodTests : TestBase
{
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
    public void ShouldErrorStreamWithCustomReason()
    {
        Engine.Execute(
            """
            let controller = null;
            let errorReason = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            try {
                controller.error('custom error reason');
                stream.getWriter();
            } catch (e) {
                errorReason = e.message || e;
            }
            """
        );

        Assert.NotNull(Engine.Evaluate("errorReason"));
    }

    [Fact]
    public void ShouldRejectWriterPromises()
    {
        Engine.Execute(
            """
            let controller = null;
            let closedRejected = false;

            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                },
                write(chunk, ctrl) {
                    return new Promise((resolve, reject) => {});
                },
            });

            const writer = stream.getWriter();
            writer.closed.catch(() => { closedRejected = true; });

            controller.error(new Error('Stream error'));
            """
        );

        Assert.True(Engine.Evaluate("closedRejected").AsBoolean());
    }

    [Fact]
    public void ShouldBeIdempotent()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            controller.error(new Error('First error'));
            controller.error(new Error('Second error')); // Should not throw
            """
        );

        // Should not throw - multiple calls to error should be safe
        Assert.True(true);
    }

    [Fact]
    public void ShouldPreventFutureWrites()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            });

            controller.error(new Error('Stream error'));
            const writer = stream.getWriter();
            """
        );

        Assert.Throws<PromiseRejectedException>(
            () => Engine.Evaluate("writer.write('test')").UnwrapIfPromise()
        );
    }
}
