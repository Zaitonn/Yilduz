using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.TransformStreamDefaultController;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("constructor")]
    [InlineData("desiredSize")]
    [InlineData("enqueue")]
    [InlineData("error")]
    [InlineData("terminate")]
    public void TransformStreamDefaultControllerShouldHaveCorrectPrototype(string propertyName)
    {
        Assert.True(
            Engine
                .Evaluate(
                    $"TransformStreamDefaultController.prototype.hasOwnProperty('{propertyName}')"
                )
                .AsBoolean()
        );
    }

    [Theory]
    [InlineData("TransformStreamDefaultController.prototype.desiredSize")]
    [InlineData("TransformStreamDefaultController.prototype.enqueue()")]
    [InlineData("TransformStreamDefaultController.prototype.error()")]
    [InlineData("TransformStreamDefaultController.prototype.terminate()")]
    public void TransformStreamDefaultControllerShouldThrowOnInvalidInvocation(string expression)
    {
        Assert.Throws<JavaScriptException>(() => Engine.Evaluate(expression));
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) { controller = c; }
            });
            """
        );

        Assert.Equal(
            "[object TransformStreamDefaultController]",
            Engine.Evaluate("Object.prototype.toString.call(controller)").AsString()
        );
    }

    [Fact]
    public void ShouldHaveCorrectConstructorName()
    {
        Assert.Equal(
            "TransformStreamDefaultController",
            Engine.Evaluate("TransformStreamDefaultController.name").AsString()
        );
    }

    [Fact]
    public void ShouldHaveDesiredSizePropertyDescriptor()
    {
        var descriptor = Engine.Evaluate(
            "Object.getOwnPropertyDescriptor(TransformStreamDefaultController.prototype, 'desiredSize')"
        );

        Assert.NotNull(descriptor.Get("get"));
        Assert.True(descriptor.Get("enumerable").AsBoolean());
        Assert.True(descriptor.Get("configurable").AsBoolean());
        Assert.Equal(JsValue.Undefined, descriptor.Get("set"));
    }

    [Fact]
    public void ShouldHaveEnqueueMethodDescriptor()
    {
        var descriptor = Engine.Evaluate(
            "Object.getOwnPropertyDescriptor(TransformStreamDefaultController.prototype, 'enqueue')"
        );

        Assert.True(descriptor.Get("value") is Function);
        Assert.False(descriptor.Get("enumerable").AsBoolean());
        Assert.False(descriptor.Get("writable").AsBoolean());
        Assert.True(descriptor.Get("configurable").AsBoolean());
    }

    [Fact]
    public void ShouldHaveErrorMethodDescriptor()
    {
        var descriptor = Engine.Evaluate(
            "Object.getOwnPropertyDescriptor(TransformStreamDefaultController.prototype, 'error')"
        );

        Assert.True(descriptor.Get("value") is Function);
        Assert.False(descriptor.Get("enumerable").AsBoolean());
        Assert.False(descriptor.Get("writable").AsBoolean());
        Assert.True(descriptor.Get("configurable").AsBoolean());
    }

    [Fact]
    public void ShouldHaveTerminateMethodDescriptor()
    {
        var descriptor = Engine.Evaluate(
            "Object.getOwnPropertyDescriptor(TransformStreamDefaultController.prototype, 'terminate')"
        );

        Assert.True(descriptor.Get("value") is Function);
        Assert.False(descriptor.Get("enumerable").AsBoolean());
        Assert.False(descriptor.Get("writable").AsBoolean());
        Assert.True(descriptor.Get("configurable").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectPrototypeChain()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) { controller = c; }
            });
            """
        );

        Assert.True(
            Engine
                .Evaluate(
                    "Object.getPrototypeOf(controller) === TransformStreamDefaultController.prototype"
                )
                .AsBoolean()
        );

        Assert.True(
            Engine
                .Evaluate(
                    "Object.getPrototypeOf(TransformStreamDefaultController.prototype) === Object.prototype"
                )
                .AsBoolean()
        );
    }

    [Fact]
    public void ShouldNotBeEnumerable()
    {
        Engine.Execute(
            """
            let controller = null;
            const stream = new TransformStream({
                start(c) { controller = c; }
            });
            """
        );

        var keys = Engine.Evaluate("Object.keys(controller)");
        Assert.Empty(keys.AsArray());
    }

    [Fact]
    public void ShouldNotBeConstructableDirectly()
    {
        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("new TransformStreamDefaultController();")
        );
    }
}
