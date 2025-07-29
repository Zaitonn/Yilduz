using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.DOMException;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateInstanceWithDefaultValues()
    {
        Engine.Execute("const exception = new DOMException();");

        Assert.True(Engine.Evaluate("exception instanceof DOMException").AsBoolean());
        Assert.Equal("", Engine.Evaluate("exception.message").AsString());
        Assert.Equal("Error", Engine.Evaluate("exception.name").AsString());
        Assert.Equal(0, Engine.Evaluate("exception.code").AsNumber());
    }

    [Fact]
    public void ShouldCreateInstanceWithMessage()
    {
        Engine.Execute("const exception = new DOMException('Something went wrong');");

        Assert.Equal("Something went wrong", Engine.Evaluate("exception.message").AsString());
        Assert.Equal("Error", Engine.Evaluate("exception.name").AsString());
        Assert.Equal(0, Engine.Evaluate("exception.code").AsNumber());
    }

    [Fact]
    public void ShouldCreateInstanceWithMessageAndName()
    {
        Engine.Execute("const exception = new DOMException('Invalid index', 'IndexSizeError');");

        Assert.Equal("Invalid index", Engine.Evaluate("exception.message").AsString());
        Assert.Equal("IndexSizeError", Engine.Evaluate("exception.name").AsString());
        Assert.Equal(1, Engine.Evaluate("exception.code").AsNumber());
    }

    [Theory]
    [InlineData("IndexSizeError", 1)]
    [InlineData("HierarchyRequestError", 3)]
    [InlineData("WrongDocumentError", 4)]
    [InlineData("InvalidCharacterError", 5)]
    [InlineData("NoModificationAllowedError", 7)]
    [InlineData("NotFoundError", 8)]
    [InlineData("NotSupportedError", 9)]
    [InlineData("InUseAttributeError", 10)]
    [InlineData("InvalidStateError", 11)]
    [InlineData("SyntaxError", 12)]
    [InlineData("InvalidModificationError", 13)]
    [InlineData("NamespaceError", 14)]
    [InlineData("InvalidAccessError", 15)]
    [InlineData("SecurityError", 18)]
    [InlineData("NetworkError", 19)]
    [InlineData("AbortError", 20)]
    [InlineData("URLMismatchError", 21)]
    [InlineData("QuotaExceededError", 22)]
    [InlineData("TimeoutError", 23)]
    [InlineData("InvalidNodeTypeError", 24)]
    [InlineData("DataCloneError", 25)]
    public void ShouldMapErrorNameToCorrectCode(string errorName, int expectedCode)
    {
        Engine.Execute($"const exception = new DOMException('Test message', '{errorName}');");

        Assert.Equal(errorName, Engine.Evaluate("exception.name").AsString());
        Assert.Equal(expectedCode, Engine.Evaluate("exception.code").AsNumber());
    }

    [Fact]
    public void ShouldReturnZeroCodeForUnknownErrorName()
    {
        Engine.Execute("const exception = new DOMException('Test', 'UnknownError');");

        Assert.Equal("UnknownError", Engine.Evaluate("exception.name").AsString());
        Assert.Equal(0, Engine.Evaluate("exception.code").AsNumber());
    }

    [Fact]
    public void ShouldHandleEmptyStringArguments()
    {
        Engine.Execute("const exception = new DOMException('', '');");

        Assert.Equal("", Engine.Evaluate("exception.message").AsString());
        Assert.Equal("", Engine.Evaluate("exception.name").AsString());
        Assert.Equal(0, Engine.Evaluate("exception.code").AsNumber());
    }

    [Fact]
    public void ShouldConvertArgumentsToStrings()
    {
        Engine.Execute("const exception = new DOMException(123, 456);");

        Assert.Equal("123", Engine.Evaluate("exception.message").AsString());
        Assert.Equal("456", Engine.Evaluate("exception.name").AsString());
    }

    [Fact]
    public void ShouldWorkWithNullAndUndefined()
    {
        Engine.Execute("const exception1 = new DOMException(null, undefined);");
        Engine.Execute("const exception2 = new DOMException(undefined, null);");

        Assert.Equal("null", Engine.Evaluate("exception1.message").AsString());
        Assert.Equal("undefined", Engine.Evaluate("exception1.name").AsString());

        Assert.Equal("undefined", Engine.Evaluate("exception2.message").AsString());
        Assert.Equal("null", Engine.Evaluate("exception2.name").AsString());
    }
}
