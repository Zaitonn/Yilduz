using Jint;
using Jint.Native;
using Jint.Runtime;
using Xunit;
using Yilduz.Aborting.AbortController;
using Yilduz.Aborting.AbortSignal;

namespace Yilduz.Tests.AbortController;

public sealed class RuntimeTests : TestBase
{
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
        Assert.NotNull(Engine.Evaluate("controller"));

        Assert.True(controller.Signal.Aborted);
        Assert.True(Engine.Evaluate("controller.signal.aborted").AsBoolean());

        var reason = Engine.Evaluate("controller.signal.reason");
        Assert.IsType<JsError>(reason);
        Assert.Equal("AbortError: signal is aborted without reason", reason.ToString());
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

    [Fact]
    public void ShouldCreateWithFreshSignal()
    {
        Engine.Execute(
            """
            const controller1 = new AbortController();
            const controller2 = new AbortController();
            """
        );

        Assert.True(Engine.Evaluate("controller1.signal !== controller2.signal").AsBoolean());
        Assert.True(
            Engine
                .Evaluate("!controller1.signal.aborted && !controller2.signal.aborted")
                .AsBoolean()
        );
    }

    [Fact]
    public void ShouldAbortWithCustomReason()
    {
        Engine.Execute(
            """
            const controller = new AbortController();
            const customReason = new Error('Custom abort reason');
            controller.abort(customReason);
            """
        );

        Assert.True(Engine.Evaluate("controller.signal.aborted").AsBoolean());
        Assert.Equal(
            "Custom abort reason",
            Engine.Evaluate("controller.signal.reason.message").AsString()
        );
    }

    [Fact]
    public void ShouldAbortWithStringReason()
    {
        Engine.Execute(
            """
            const controller = new AbortController();
            controller.abort('String reason');
            """
        );

        Assert.True(Engine.Evaluate("controller.signal.aborted").AsBoolean());
        Assert.Equal("String reason", Engine.Evaluate("controller.signal.reason").AsString());
    }

    [Fact]
    public void ShouldAbortWithNumberReason()
    {
        Engine.Execute(
            """
            const controller = new AbortController();
            controller.abort(42);
            """
        );

        Assert.True(Engine.Evaluate("controller.signal.aborted").AsBoolean());
        Assert.Equal(42, Engine.Evaluate("controller.signal.reason").AsNumber());
    }

    [Fact]
    public void ShouldAbortWithObjectReason()
    {
        Engine.Execute(
            """
            const controller = new AbortController();
            const reason = { code: 'TIMEOUT', message: 'Operation timed out' };
            controller.abort(reason);
            """
        );

        Assert.True(Engine.Evaluate("controller.signal.aborted").AsBoolean());
        Assert.Equal("TIMEOUT", Engine.Evaluate("controller.signal.reason.code").AsString());
        Assert.Equal(
            "Operation timed out",
            Engine.Evaluate("controller.signal.reason.message").AsString()
        );
    }

    [Fact]
    public void ShouldSupportSignalInheritance()
    {
        Engine.Execute("const controller = new AbortController();");

        Assert.True(Engine.Evaluate("controller.signal instanceof EventTarget").AsBoolean());
        Assert.True(Engine.Evaluate("'aborted' in controller.signal").AsBoolean());
        Assert.True(Engine.Evaluate("'reason' in controller.signal").AsBoolean());
    }

    [Fact]
    public void ShouldThrowErrorOnSignalConstruction()
    {
        var exception = Assert.Throws<JavaScriptException>(() =>
        {
            Engine.Execute("new AbortSignal();");
        });

        Assert.Contains("AbortSignal", exception.Message);
    }

    [Fact]
    public void ShouldHaveCorrectToString()
    {
        Engine.Execute("const controller = new AbortController();");
        Assert.Equal(
            "[object AbortController]",
            Engine.Evaluate("controller.toString()").AsString()
        );
        Assert.Equal(
            "[object AbortSignal]",
            Engine.Evaluate("controller.signal.toString()").AsString()
        );
    }

    [Fact]
    public void ShouldHandleCustomErrors()
    {
        Engine.Execute(
            """
            const controller = new AbortController();
            const customError = new Error('Custom error message');
            customError.code = 'CUSTOM_CODE';
            controller.abort(customError);
            const error = controller.signal.reason;
            """
        );

        var errorMessage = Engine.Evaluate("error.message").AsString();
        var errorCode = Engine.Evaluate("error.code").AsString();

        Assert.Equal("Custom error message", errorMessage);
        Assert.Equal("CUSTOM_CODE", errorCode);
    }

    [Fact]
    public void CanSetOnAbort()
    {
        Engine.Execute(
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

        Assert.True(Engine.Evaluate("called").AsBoolean());
    }
}
