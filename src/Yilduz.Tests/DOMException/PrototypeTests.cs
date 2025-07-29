using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.DOMException;

public sealed class PrototypeTests : TestBase
{
    [Theory]
    [InlineData("name")]
    [InlineData("message")]
    [InlineData("code")]
    public void ShouldHaveProperty(string propertyName)
    {
        Engine.Execute("const exception = new DOMException();");

        Assert.True(Engine.Evaluate($"'{propertyName}' in exception").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Engine.Execute("const exception = new DOMException();");

        Assert.Equal(
            "[object DOMException]",
            Engine.Evaluate("Object.prototype.toString.call(exception)").AsString()
        );
    }

    [Fact]
    public void ShouldHaveReadOnlyProperties()
    {
        Engine.Execute(
            @"
            const exception = new DOMException('Test message', 'TestError');
            const originalName = exception.name;
            const originalMessage = exception.message;
            const originalCode = exception.code;
            
            try { exception.name = 'changed'; } catch {}
            try { exception.message = 'changed'; } catch {}
            try { exception.code = 999; } catch {}
        "
        );

        Assert.Equal(
            Engine.Evaluate("originalName").AsString(),
            Engine.Evaluate("exception.name").AsString()
        );
        Assert.Equal(
            Engine.Evaluate("originalMessage").AsString(),
            Engine.Evaluate("exception.message").AsString()
        );
        Assert.Equal(
            Engine.Evaluate("originalCode").AsNumber(),
            Engine.Evaluate("exception.code").AsNumber()
        );
    }

    [Fact]
    public void ShouldHaveCorrectConstructorProperty()
    {
        Engine.Execute("const exception = new DOMException();");

        Assert.True(Engine.Evaluate("exception.constructor === DOMException").AsBoolean());
    }

    [Fact]
    public void ShouldInheritFromObjectPrototype()
    {
        Engine.Execute("const exception = new DOMException();");

        Assert.True(Engine.Evaluate("exception instanceof Object").AsBoolean());
        Assert.True(
            Engine
                .Evaluate("Object.getPrototypeOf(exception) === DOMException.prototype")
                .AsBoolean()
        );
    }

    [Fact]
    public void ShouldHaveCorrectPropertyDescriptors()
    {
        Engine.Execute(
            @"
            const exception = new DOMException('Test', 'TestError');
            const nameDesc = Object.getOwnPropertyDescriptor(DOMException.prototype, 'name');
            const messageDesc = Object.getOwnPropertyDescriptor(DOMException.prototype, 'message');
            const codeDesc = Object.getOwnPropertyDescriptor(DOMException.prototype, 'code');
        "
        );

        // Properties should be configured with getters but no setters
        Assert.True(Engine.Evaluate("typeof nameDesc.get === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("nameDesc.set === undefined").AsBoolean());
        Assert.False(Engine.Evaluate("nameDesc.enumerable").AsBoolean());
        Assert.True(Engine.Evaluate("nameDesc.configurable").AsBoolean());

        Assert.True(Engine.Evaluate("typeof messageDesc.get === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("messageDesc.set === undefined").AsBoolean());
        Assert.False(Engine.Evaluate("messageDesc.enumerable").AsBoolean());
        Assert.True(Engine.Evaluate("messageDesc.configurable").AsBoolean());

        Assert.True(Engine.Evaluate("typeof codeDesc.get === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("codeDesc.set === undefined").AsBoolean());
        Assert.False(Engine.Evaluate("codeDesc.enumerable").AsBoolean());
        Assert.True(Engine.Evaluate("codeDesc.configurable").AsBoolean());
    }

    [Fact]
    public void ShouldNotAllowDirectCall()
    {
        const string expression = "DOMException.prototype.name.call({})";

        Assert.Throws<JavaScriptException>(() => Engine.Evaluate(expression));
    }

    [Fact]
    public void ShouldWorkWithPropertyAccess()
    {
        Engine.Execute("const exception = new DOMException('Test message', 'IndexSizeError');");

        // Direct property access
        Assert.Equal("Test message", Engine.Evaluate("exception.message").AsString());
        Assert.Equal("IndexSizeError", Engine.Evaluate("exception.name").AsString());
        Assert.Equal(1, Engine.Evaluate("exception.code").AsNumber());

        // Bracket notation
        Assert.Equal("Test message", Engine.Evaluate("exception['message']").AsString());
        Assert.Equal("IndexSizeError", Engine.Evaluate("exception['name']").AsString());
        Assert.Equal(1, Engine.Evaluate("exception['code']").AsNumber());
    }
}
