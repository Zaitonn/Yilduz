using Jint;
using Jint.Native;
using Xunit;
using Yilduz.Aborting.AbortController;
using Yilduz.Aborting.AbortSignal;

namespace Yilduz.Tests.AbortController;

public sealed class RuntimeTests : TestBase
{
    public RuntimeTests()
    {
        Engine.AddAbortingApi();
    }

    [Fact]
    public void CanCreateNewAbortController()
    {
        var result = Engine.Evaluate("new AbortController()");
        Assert.NotNull(result);
        Assert.True(result is AbortControllerInstance);
    }

    [Fact]
    public void CanAccessSignalProperty()
    {
        Engine.Execute("var controller = new AbortController();");
        var signal = Engine.Evaluate("controller.signal");

        Assert.NotNull(signal);
        Assert.True(signal is AbortSignalInstance);

        Assert.Equal(JsValue.Undefined, Engine.Evaluate("controller.signal.reason"));
        Assert.Equal(false, Engine.Evaluate("controller.signal.aborted"));
    }

    [Fact]
    public void CanAbort()
    {
        Engine.Execute("var controller = new AbortController();");
        Engine.Execute("controller.abort('Test reason');");

        var controller = Engine.Evaluate("controller") as AbortControllerInstance;

        Assert.NotNull(controller);
        Assert.True(controller.Signal.Aborted);
        Assert.Equal("Test reason", controller.Signal.Reason);
    }

    [Fact]
    public void CanAbortWithoutReason()
    {
        Engine.Execute("var controller = new AbortController();");
        Engine.Execute("controller.abort();");

        var controller = Engine.Evaluate("controller") as AbortControllerInstance;

        Assert.NotNull(controller);
        Assert.True(controller.Signal.Aborted);
        Assert.Equal(JsValue.Undefined, controller.Signal.Reason);

        Assert.NotNull(Engine.Evaluate("controller"));
        Assert.True(Engine.Evaluate("controller.signal.aborted").AsBoolean());
        Assert.Equal(JsValue.Undefined, Engine.Evaluate("controller.signal.reason"));
    }

    [Fact]
    public void CanAbortMultipleTimes()
    {
        Engine.Execute("var controller = new AbortController();");
        Engine.Execute("controller.abort('First reason');");
        Engine.Execute("controller.abort('Second reason');");

        var controller = Engine.Evaluate("controller") as AbortControllerInstance;

        Assert.NotNull(controller);
        Assert.True(controller.Signal.Aborted);
        Assert.Equal("First reason", controller.Signal.Reason);

        Assert.NotNull(Engine.Evaluate("controller"));
        Assert.True(Engine.Evaluate("controller.signal.aborted").AsBoolean());
        Assert.Equal("First reason", Engine.Evaluate("controller.signal.reason").AsString());
    }
}
