using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.DOMException;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldBeThrowable()
    {
        var exception = Assert.Throws<JavaScriptException>(
            () => Execute("throw new DOMException('Test error', 'TestError');")
        );

        Assert.NotNull(exception.Error);
    }

    [Fact]
    public void ShouldBeCatchable()
    {
        Execute(
            @"
            let caught = false;
            let caughtException = null;
            
            try {
                throw new DOMException('Test error', 'TestError');
            } catch (e) {
                caught = true;
                caughtException = e;
            }
        "
        );

        Assert.True(Evaluate("caught").AsBoolean());
        Assert.True(Evaluate("caughtException instanceof DOMException").AsBoolean());
        Assert.Equal("Test error", Evaluate("caughtException.message").AsString());
        Assert.Equal("TestError", Evaluate("caughtException.name").AsString());
    }

    [Fact]
    public void ShouldWorkInTryCatchFinally()
    {
        Execute(
            @"
            let executed = [];
            
            try {
                executed.push('try');
                throw new DOMException('Test', 'IndexSizeError');
                executed.push('after-throw'); // Should not execute
            } catch (e) {
                executed.push('catch');
                executed.push(e.name);
            } finally {
                executed.push('finally');
            }
        "
        );

        var result = Evaluate("executed").AsArray();
        Assert.Equal<uint>(4, result.Length);
        Assert.Equal("try", result[0].AsString());
        Assert.Equal("catch", result[1].AsString());
        Assert.Equal("IndexSizeError", result[2].AsString());
        Assert.Equal("finally", result[3].AsString());
    }

    [Fact]
    public void ShouldWorkWithPromiseRejection()
    {
        Execute(
            @"
            let rejectionCaught = false;
            let rejectionError = null;
            
            const promise = Promise.reject(new DOMException('Async error', 'NetworkError'));
            
            promise.catch(e => {
                rejectionCaught = true;
                rejectionError = e;
            });
        "
        );

        Assert.True(Evaluate("rejectionCaught").AsBoolean());
        Assert.True(Evaluate("rejectionError instanceof DOMException").AsBoolean());
        Assert.Equal("Async error", Evaluate("rejectionError.message").AsString());
        Assert.Equal("NetworkError", Evaluate("rejectionError.name").AsString());
    }

    [Fact]
    public void ShouldWorkWithToString()
    {
        Execute("const exception = new DOMException('Test message', 'TestError');");

        Assert.Equal("TestError: Test message", Evaluate("exception.toString()").AsString());
    }

    [Fact]
    public void ShouldWorkWithStringCoercion()
    {
        Execute("const exception = new DOMException('Test message', 'TestError');");

        Assert.Equal("TestError: Test message", Evaluate("String(exception)").AsString());
        Assert.Equal("TestError: Test message", Evaluate("'' + exception").AsString());
    }

    [Theory]
    [InlineData("IndexSizeError", "Index out of bounds")]
    [InlineData("InvalidCharacterError", "Invalid character in string")]
    [InlineData("NotFoundError", "Element not found")]
    [InlineData("SecurityError", "Security violation")]
    [InlineData("NetworkError", "Network request failed")]
    [InlineData("AbortError", "Operation was aborted")]
    public void ShouldWorkWithCommonErrorTypes(string errorName, string message)
    {
        Execute($"const exception = new DOMException('{message}', '{errorName}');");

        Assert.Equal(message, Evaluate("exception.message").AsString());
        Assert.Equal(errorName, Evaluate("exception.name").AsString());
        Assert.True(Evaluate("exception.code > 0").AsBoolean());
    }

    [Fact]
    public void ShouldWorkWithInstanceof()
    {
        Execute("const exception = new DOMException();");

        Assert.True(Evaluate("exception instanceof DOMException").AsBoolean());
        Assert.True(Evaluate("exception instanceof Object").AsBoolean());
        Assert.False(Evaluate("exception instanceof Error").AsBoolean());
        Assert.False(Evaluate("exception instanceof Array").AsBoolean());
    }

    [Fact]
    public void ShouldWorkWithObjectMethods()
    {
        Execute("const exception = new DOMException('Test', 'TestError');");

        // hasOwnProperty should work for inherited properties
        Assert.False(Evaluate("exception.hasOwnProperty('name')").AsBoolean());
        Assert.False(Evaluate("exception.hasOwnProperty('message')").AsBoolean());
        Assert.False(Evaluate("exception.hasOwnProperty('code')").AsBoolean());

        // But 'in' operator should work
        Assert.True(Evaluate("'name' in exception").AsBoolean());
        Assert.True(Evaluate("'message' in exception").AsBoolean());
        Assert.True(Evaluate("'code' in exception").AsBoolean());
    }

    [Fact]
    public void ShouldWorkWithJSONStringify()
    {
        Execute("const exception = new DOMException('Test message', 'TestError');");

        // DOMException should stringify to an empty object by default (like Error objects)
        Assert.Equal("{}", Evaluate("JSON.stringify(exception)").AsString());
    }

    [Fact]
    public void ShouldWorkInArrays()
    {
        Execute(
            @"
            const exceptions = [
                new DOMException('Error 1', 'IndexSizeError'),
                new DOMException('Error 2', 'NetworkError'),
                new DOMException('Error 3', 'SecurityError')
            ];
        "
        );

        Assert.Equal(3, Evaluate("exceptions.length").AsNumber());
        Assert.True(Evaluate("exceptions[0] instanceof DOMException").AsBoolean());
        Assert.Equal("Error 1", Evaluate("exceptions[0].message").AsString());
        Assert.Equal("IndexSizeError", Evaluate("exceptions[0].name").AsString());
    }

    [Fact]
    public void ShouldWorkWithComparison()
    {
        Execute(
            @"
            const exception1 = new DOMException('Test', 'TestError');
            const exception2 = new DOMException('Test', 'TestError');
            const exception3 = exception1;
        "
        );

        // Different instances should not be equal even with same content
        Assert.False(Evaluate("exception1 === exception2").AsBoolean());
        Assert.False(Evaluate("exception1 == exception2").AsBoolean());

        // Same reference should be equal
        Assert.True(Evaluate("exception1 === exception3").AsBoolean());
    }
}
