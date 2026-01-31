using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Xunit;
using Yilduz.DOM.DOMException;

namespace Yilduz.Tests.AbortSignal;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void CanNotCreateNewAbortSignalDirectly()
    {
        Assert.Throws<JavaScriptException>(() => Evaluate("new AbortSignal()"));
        Assert.Throws<JavaScriptException>(() => Evaluate("new AbortSignal({})"));
        Assert.Throws<JavaScriptException>(() => Evaluate("new AbortSignal(12345)"));
    }

    [Fact]
    public void CanThrowIfAborted()
    {
        Execute(
            """
            var controller = new AbortController()
            var signal = controller.signal;
            signal.throwIfAborted();
            """
        );

        Execute("controller.abort();");

        Assert.Throws<JavaScriptException>(() => Execute("signal.throwIfAborted();"));
    }

    [Fact]
    public void CanCreateAbortedSignal()
    {
        Execute("var signal = AbortSignal.abort('test');");

        Assert.True(Evaluate("signal.aborted").AsBoolean());
        Assert.Throws<JavaScriptException>(() => Execute("signal.throwIfAborted();"));
    }

    [Fact]
    public async Task CanCreateAbortedSignalWithTimeout()
    {
        Execute("var signal = AbortSignal.timeout(1000);");
        Assert.False(Evaluate("signal.aborted").AsBoolean());

        await Task.Delay(1500);
        Assert.True(Evaluate("signal.aborted").AsBoolean());
        Assert.Throws<JavaScriptException>(() => Execute("signal.throwIfAborted();"));
    }

    [Fact]
    public void CanCreateAbortedSignalLinkedWithAnother()
    {
        Assert.False(Evaluate("AbortSignal.any([]).aborted").AsBoolean());
        Assert.True(Evaluate("AbortSignal.any([AbortSignal.abort()]).aborted").AsBoolean());

        Execute(
            """
            var controller1 = new AbortController();
            var controller2 = new AbortController();
            var signal = AbortSignal.any([controller1.signal, controller2.signal]);
            """
        );
        Assert.False(Evaluate("signal.aborted").AsBoolean());

        Execute("controller1.abort();");
        Assert.True(Evaluate("signal.aborted").AsBoolean());
        Assert.Throws<JavaScriptException>(() => Execute("signal.throwIfAborted();"));
    }

    [Fact]
    public void ShouldHaveCorrectReason()
    {
        var reason = Evaluate("AbortSignal.abort().reason").AsObject();
        Assert.True(reason is DOMExceptionInstance);
        Assert.Equal("AbortError", reason["name"]);
        Assert.Equal("signal is aborted without reason", reason["message"]);

        Assert.Equal(1, Evaluate("AbortSignal.abort(1).reason"));
        Assert.Equal(JsValue.Null, Evaluate("AbortSignal.abort(null).reason"));
    }

    [Fact]
    public void ShouldSupportStaticAbortMethod()
    {
        Execute("const signal = AbortSignal.abort('Static abort reason');");

        Assert.True(Evaluate("signal.aborted").AsBoolean());
        Assert.Equal("Static abort reason", Evaluate("signal.reason").AsString());
    }
}
