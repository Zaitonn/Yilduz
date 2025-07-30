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
    public void ShouldNotBeConstructibleWithArguments()
    {
        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("new WritableStreamDefaultController({});")
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
    public void ShouldBePassedToUnderlyingSinkWrite()
    {
        Engine.Execute(
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

        Assert.IsType<WritableStreamDefaultControllerInstance>(Engine.Evaluate("writeController"));
        Assert.True(
            Engine
                .Evaluate("writeController instanceof WritableStreamDefaultController")
                .AsBoolean()
        );
    }

    [Fact]
    public async Task ShouldBePassedToUnderlyingSinkClose()
    {
        Engine.Execute(
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

        if (!Engine.Evaluate("closeController === null").AsBoolean())
        {
            Assert.IsType<WritableStreamDefaultControllerInstance>(
                Engine.Evaluate("closeController")
            );
        }
    }
}
