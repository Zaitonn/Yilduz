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
        Execute("const exception = new DOMException();");

        Assert.True(Evaluate($"'{propertyName}' in exception").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Execute("const exception = new DOMException();");

        Assert.Equal(
            "[object DOMException]",
            Evaluate("Object.prototype.toString.call(exception)").AsString()
        );
    }

    [Fact]
    public void ShouldHaveReadOnlyProperties()
    {
        Execute(
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

        Assert.Equal(Evaluate("originalName").AsString(), Evaluate("exception.name").AsString());
        Assert.Equal(
            Evaluate("originalMessage").AsString(),
            Evaluate("exception.message").AsString()
        );
        Assert.Equal(Evaluate("originalCode").AsNumber(), Evaluate("exception.code").AsNumber());
    }

    [Fact]
    public void ShouldHaveCorrectConstructorProperty()
    {
        Execute("const exception = new DOMException();");

        Assert.True(Evaluate("exception.constructor === DOMException").AsBoolean());
    }

    [Fact]
    public void ShouldInheritFromObjectPrototype()
    {
        Execute("const exception = new DOMException();");

        Assert.True(Evaluate("exception instanceof Object").AsBoolean());
        Assert.True(
            Engine
                .Evaluate("Object.getPrototypeOf(exception) === DOMException.prototype")
                .AsBoolean()
        );
    }

    [Fact]
    public void ShouldHaveCorrectPropertyDescriptors()
    {
        Execute(
            @"
            const exception = new DOMException('Test', 'TestError');
            const nameDesc = Object.getOwnPropertyDescriptor(DOMException.prototype, 'name');
            const messageDesc = Object.getOwnPropertyDescriptor(DOMException.prototype, 'message');
            const codeDesc = Object.getOwnPropertyDescriptor(DOMException.prototype, 'code');
        "
        );

        // Properties should be configured with getters but no setters
        Assert.True(Evaluate("typeof nameDesc.get === 'function'").AsBoolean());
        Assert.True(Evaluate("nameDesc.set === undefined").AsBoolean());
        Assert.False(Evaluate("nameDesc.enumerable").AsBoolean());
        Assert.True(Evaluate("nameDesc.configurable").AsBoolean());

        Assert.True(Evaluate("typeof messageDesc.get === 'function'").AsBoolean());
        Assert.True(Evaluate("messageDesc.set === undefined").AsBoolean());
        Assert.False(Evaluate("messageDesc.enumerable").AsBoolean());
        Assert.True(Evaluate("messageDesc.configurable").AsBoolean());

        Assert.True(Evaluate("typeof codeDesc.get === 'function'").AsBoolean());
        Assert.True(Evaluate("codeDesc.set === undefined").AsBoolean());
        Assert.False(Evaluate("codeDesc.enumerable").AsBoolean());
        Assert.True(Evaluate("codeDesc.configurable").AsBoolean());
    }

    [Fact]
    public void ShouldNotAllowDirectCall()
    {
        const string expression = "DOMException.prototype.name.call({})";

        Assert.Throws<JavaScriptException>(() => Evaluate(expression));
    }

    [Fact]
    public void ShouldWorkWithPropertyAccess()
    {
        Execute("const exception = new DOMException('Test message', 'IndexSizeError');");

        // Direct property access
        Assert.Equal("Test message", Evaluate("exception.message").AsString());
        Assert.Equal("IndexSizeError", Evaluate("exception.name").AsString());
        Assert.Equal(1, Evaluate("exception.code").AsNumber());

        // Bracket notation
        Assert.Equal("Test message", Evaluate("exception['message']").AsString());
        Assert.Equal("IndexSizeError", Evaluate("exception['name']").AsString());
        Assert.Equal(1, Evaluate("exception['code']").AsNumber());
    }
}
