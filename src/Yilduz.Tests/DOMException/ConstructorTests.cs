using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.DOMException;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateInstanceWithDefaultValues()
    {
        Execute("const exception = new DOMException();");

        Assert.True(Evaluate("exception instanceof DOMException").AsBoolean());
        Assert.Equal("", Evaluate("exception.message").AsString());
        Assert.Equal("Error", Evaluate("exception.name").AsString());
        Assert.Equal(0, Evaluate("exception.code").AsNumber());
    }

    [Fact]
    public void ShouldCreateInstanceWithMessage()
    {
        Execute("const exception = new DOMException('Something went wrong');");

        Assert.Equal("Something went wrong", Evaluate("exception.message").AsString());
        Assert.Equal("Error", Evaluate("exception.name").AsString());
        Assert.Equal(0, Evaluate("exception.code").AsNumber());
    }

    [Fact]
    public void ShouldCreateInstanceWithMessageAndName()
    {
        Execute("const exception = new DOMException('Invalid index', 'IndexSizeError');");

        Assert.Equal("Invalid index", Evaluate("exception.message").AsString());
        Assert.Equal("IndexSizeError", Evaluate("exception.name").AsString());
        Assert.Equal(1, Evaluate("exception.code").AsNumber());
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
        Execute($"const exception = new DOMException('Test message', '{errorName}');");

        Assert.Equal(errorName, Evaluate("exception.name").AsString());
        Assert.Equal(expectedCode, Evaluate("exception.code").AsNumber());
    }

    [Fact]
    public void ShouldReturnZeroCodeForUnknownErrorName()
    {
        Execute("const exception = new DOMException('Test', 'UnknownError');");

        Assert.Equal("UnknownError", Evaluate("exception.name").AsString());
        Assert.Equal(0, Evaluate("exception.code").AsNumber());
    }

    [Fact]
    public void ShouldHandleEmptyStringArguments()
    {
        Execute("const exception = new DOMException('', '');");

        Assert.Equal("", Evaluate("exception.message").AsString());
        Assert.Equal("", Evaluate("exception.name").AsString());
        Assert.Equal(0, Evaluate("exception.code").AsNumber());
    }

    [Fact]
    public void ShouldConvertArgumentsToStrings()
    {
        Execute("const exception = new DOMException(123, 456);");

        Assert.Equal("123", Evaluate("exception.message").AsString());
        Assert.Equal("456", Evaluate("exception.name").AsString());
    }

    [Fact]
    public void ShouldWorkWithNullAndUndefined()
    {
        Execute("const exception1 = new DOMException(null, undefined);");
        Execute("const exception2 = new DOMException(undefined, null);");

        Assert.Equal("null", Evaluate("exception1.message").AsString());
        Assert.Equal("undefined", Evaluate("exception1.name").AsString());

        Assert.Equal("undefined", Evaluate("exception2.message").AsString());
        Assert.Equal("null", Evaluate("exception2.name").AsString());
    }
}
