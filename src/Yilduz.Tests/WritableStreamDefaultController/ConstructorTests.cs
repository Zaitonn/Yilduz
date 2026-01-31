using System.Threading.Tasks;
using Jint;
using Jint.Runtime;
using Xunit;
using Yilduz.Aborting.AbortController;
using Yilduz.Streams.WritableStreamDefaultController;

namespace Yilduz.Tests.WritableStreamDefaultController;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldBeGlobalConstructor()
    {
        Assert.True(Evaluate("typeof WritableStreamDefaultController === 'function'").AsBoolean());
        Assert.True(Evaluate("WritableStreamDefaultController.prototype").IsObject());
    }

    [Fact]
    public void ShouldNotBeConstructible()
    {
        Assert.Throws<JavaScriptException>(() => Execute("new WritableStreamDefaultController();"));
    }

    [Fact]
    public void ShouldNotBeConstructibleWithArguments()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("new WritableStreamDefaultController({});")
        );
    }

    [Fact]
    public void ShouldBePassedToUnderlyingSinkStart()
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

        Assert.IsType<WritableStreamDefaultControllerInstance>(Evaluate("controller"));
        Assert.True(Evaluate("controller instanceof WritableStreamDefaultController").AsBoolean());
    }

    [Fact]
    public void ShouldBePassedToUnderlyingSinkWrite()
    {
        Execute(
            """
            let writeController = null;
            const stream = new WritableStream({
                write(chunk, ctrl) {
                    writeController = ctrl;
                }
            });
            const writer = stream.getWriter();
            writer.write('test');
            """
        );

        Assert.IsType<WritableStreamDefaultControllerInstance>(Evaluate("writeController"));
        Assert.True(
            Engine
                .Evaluate("writeController instanceof WritableStreamDefaultController")
                .AsBoolean()
        );
    }

    [Fact]
    public async Task ShouldBePassedToUnderlyingSinkClose()
    {
        Execute(
            """
            let closeController = null;
            const stream = new WritableStream({
                close(ctrl) {
                    closeController = ctrl;
                }
            });
            const writer = stream.getWriter();
            writer.close();
            """
        );

        await Task.Delay(100);

        if (!Evaluate("closeController === null").AsBoolean())
        {
            Assert.IsType<WritableStreamDefaultControllerInstance>(Evaluate("closeController"));
        }
    }
}
