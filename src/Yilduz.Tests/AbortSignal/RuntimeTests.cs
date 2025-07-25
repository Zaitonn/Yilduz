using System.Threading.Tasks;
using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.AbortSignal;

public sealed class RuntimeTests : TestBase
{
    public RuntimeTests()
    {
        Engine.AddAbortingApi();
    }

    [Fact]
    public void CanNotCreateNewAbortSignalDirectly()
    {
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate("new AbortSignal()"));
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate("new AbortSignal({})"));
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate("new AbortSignal(12345)"));
    }

    [Fact]
    public void CanThrowIfAborted()
    {
        Engine.Execute(
            """
            var controller = new AbortController()
            var signal = controller.signal;
            signal.throwIfAborted();
            """
        );

        Engine.Execute("controller.abort();");

        Assert.Throws<JavaScriptException>(() => Engine.Execute("signal.throwIfAborted();"));
    }

    [Fact]
    public void CanCreateAbortedSignal()
    {
        Engine.Execute("var signal = AbortSignal.abort('test');");

        Assert.True(Engine.Evaluate("signal.aborted").AsBoolean());
        Assert.Throws<JavaScriptException>(() => Engine.Execute("signal.throwIfAborted();"));
    }

    [Fact]
    public async Task CanCreateAbortedSignalWithTimeout()
    {
        Engine.Execute("var signal = AbortSignal.timeout(1000);");
        Assert.False(Engine.Evaluate("signal.aborted").AsBoolean());

        await Task.Delay(1500);
        Assert.True(Engine.Evaluate("signal.aborted").AsBoolean());
        Assert.Throws<JavaScriptException>(() => Engine.Execute("signal.throwIfAborted();"));
    }

    [Fact]
    public void CanCreateAbortedSignalLinkedWithAnother()
    {
        Assert.False(Engine.Evaluate("AbortSignal.any([]).aborted").AsBoolean());
        Assert.True(Engine.Evaluate("AbortSignal.any([AbortSignal.abort()]).aborted").AsBoolean());

        Engine.Execute(
            """
            var controller1 = new AbortController();
            var controller2 = new AbortController();
            var signal = AbortSignal.any([controller1.signal, controller2.signal]);
            """
        );
        Assert.False(Engine.Evaluate("signal.aborted").AsBoolean());

        Engine.Execute("controller1.abort();");
        Assert.True(Engine.Evaluate("signal.aborted").AsBoolean());
        Assert.Throws<JavaScriptException>(() => Engine.Execute("signal.throwIfAborted();"));
    }
}
