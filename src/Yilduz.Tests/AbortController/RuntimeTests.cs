using Jint;
using Jint.Native;
using Jint.Runtime;
using Xunit;
using Yilduz.Aborting.AbortController;
using Yilduz.Aborting.AbortSignal;
using Yilduz.DOM.DOMException;

namespace Yilduz.Tests.AbortController;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void CanCreateNewAbortController()
    {
        var result = Evaluate("new AbortController()");
        Assert.NotNull(result);
        Assert.True(result is AbortControllerInstance);
    }

    [Fact]
    public void CanAccessSignalProperty()
    {
        Execute("var controller = new AbortController();");
        var signal = Evaluate("controller.signal");

        Assert.NotNull(signal);
        Assert.True(signal is AbortSignalInstance);

        Assert.Equal(JsValue.Undefined, Evaluate("controller.signal.reason"));
        Assert.Equal(false, Evaluate("controller.signal.aborted"));
    }

    [Fact]
    public void CanAbort()
    {
        Execute("var controller = new AbortController();");
        Execute("controller.abort('Test reason');");

        var controller = Evaluate("controller") as AbortControllerInstance;

        Assert.NotNull(controller);
        Assert.True(controller.Signal.Aborted);
        Assert.Equal("Test reason", controller.Signal.Reason);
    }

    [Fact]
    public void CanAbortWithoutReason()
    {
        Execute("var controller = new AbortController();");
        Execute("controller.abort();");

        var controller = Evaluate("controller") as AbortControllerInstance;

        Assert.NotNull(controller);
        Assert.NotNull(Evaluate("controller"));

        Assert.True(controller.Signal.Aborted);
        Assert.True(Evaluate("controller.signal.aborted").AsBoolean());

        var reason = Evaluate("controller.signal.reason");
        Assert.IsType<DOMExceptionInstance>(reason);
        Assert.Equal("AbortError: signal is aborted without reason", reason.ToString());
    }

    [Fact]
    public void CanAbortMultipleTimes()
    {
        Execute("var controller = new AbortController();");
        Execute("controller.abort('First reason');");
        Execute("controller.abort('Second reason');");

        var controller = Evaluate("controller") as AbortControllerInstance;

        Assert.NotNull(controller);
        Assert.True(controller.Signal.Aborted);
        Assert.Equal("First reason", controller.Signal.Reason);

        Assert.NotNull(Evaluate("controller"));
        Assert.True(Evaluate("controller.signal.aborted").AsBoolean());
        Assert.Equal("First reason", Evaluate("controller.signal.reason").AsString());
    }

    [Fact]
    public void ShouldCreateWithFreshSignal()
    {
        Execute(
            """
            const controller1 = new AbortController();
            const controller2 = new AbortController();
            """
        );

        Assert.True(Evaluate("controller1.signal !== controller2.signal").AsBoolean());
        Assert.True(
            Engine
                .Evaluate("!controller1.signal.aborted && !controller2.signal.aborted")
                .AsBoolean()
        );
    }

    [Fact]
    public void ShouldAbortWithCustomReason()
    {
        Execute(
            """
            const controller = new AbortController();
            const customReason = new Error('Custom abort reason');
            controller.abort(customReason);
            """
        );

        Assert.True(Evaluate("controller.signal.aborted").AsBoolean());
        Assert.Equal(
            "Custom abort reason",
            Evaluate("controller.signal.reason.message").AsString()
        );
    }

    [Fact]
    public void ShouldAbortWithStringReason()
    {
        Execute(
            """
            const controller = new AbortController();
            controller.abort('String reason');
            """
        );

        Assert.True(Evaluate("controller.signal.aborted").AsBoolean());
        Assert.Equal("String reason", Evaluate("controller.signal.reason").AsString());
    }

    [Fact]
    public void ShouldAbortWithNumberReason()
    {
        Execute(
            """
            const controller = new AbortController();
            controller.abort(42);
            """
        );

        Assert.True(Evaluate("controller.signal.aborted").AsBoolean());
        Assert.Equal(42, Evaluate("controller.signal.reason").AsNumber());
    }

    [Fact]
    public void ShouldAbortWithObjectReason()
    {
        Execute(
            """
            const controller = new AbortController();
            const reason = { code: 'TIMEOUT', message: 'Operation timed out' };
            controller.abort(reason);
            """
        );

        Assert.True(Evaluate("controller.signal.aborted").AsBoolean());
        Assert.Equal("TIMEOUT", Evaluate("controller.signal.reason.code").AsString());
        Assert.Equal(
            "Operation timed out",
            Evaluate("controller.signal.reason.message").AsString()
        );
    }

    [Fact]
    public void ShouldSupportSignalInheritance()
    {
        Execute("const controller = new AbortController();");

        Assert.True(Evaluate("controller.signal instanceof EventTarget").AsBoolean());
        Assert.True(Evaluate("'aborted' in controller.signal").AsBoolean());
        Assert.True(Evaluate("'reason' in controller.signal").AsBoolean());
    }

    [Fact]
    public void ShouldThrowErrorOnSignalConstruction()
    {
        var exception = Assert.Throws<JavaScriptException>(() =>
        {
            Execute("new AbortSignal();");
        });

        Assert.Contains("AbortSignal", exception.Message);
    }

    [Fact]
    public void ShouldHaveCorrectToString()
    {
        Execute("const controller = new AbortController();");
        Assert.Equal("[object AbortController]", Evaluate("controller.toString()").AsString());
        Assert.Equal("[object AbortSignal]", Evaluate("controller.signal.toString()").AsString());
    }

    [Fact]
    public void ShouldHandleCustomErrors()
    {
        Execute(
            """
            const controller = new AbortController();
            const customError = new Error('Custom error message');
            customError.code = 'CUSTOM_CODE';
            controller.abort(customError);
            const error = controller.signal.reason;
            """
        );

        var errorMessage = Evaluate("error.message").AsString();
        var errorCode = Evaluate("error.code").AsString();

        Assert.Equal("Custom error message", errorMessage);
        Assert.Equal("CUSTOM_CODE", errorCode);
    }

    [Fact]
    public void CanSetOnAbort()
    {
        Execute(
            """
            const controller = new AbortController();
            const signal = controller.signal;
            let called = false;

            signal.onabort = () => {
                called = true;
            };
            controller.abort();
            """
        );

        Assert.True(Evaluate("called").AsBoolean());
    }
}
