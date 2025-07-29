using Jint;
using Jint.Runtime;
using Xunit;
using Yilduz.Utils;

namespace Yilduz.Tests.DOMException;

public sealed class HelperTests : TestBase
{
    [Fact]
    public void ShouldThrowIndexSizeError()
    {
        var exception = Assert.Throws<JavaScriptException>(
            () => DOMExceptionHelper.CreateIndexSizeError(Engine, "Index out of bounds").Throw()
        );

        var domException = exception.Error.AsObject();
        Assert.Equal("IndexSizeError", domException.Get("name").AsString());
        Assert.Equal("Index out of bounds", domException.Get("message").AsString());
        Assert.Equal(1, domException.Get("code").AsNumber());
    }

    [Fact]
    public void ShouldThrowHierarchyRequestError()
    {
        var exception = Assert.Throws<JavaScriptException>(
            () =>
                DOMExceptionHelper.CreateHierarchyRequestError(Engine, "Invalid hierarchy").Throw()
        );

        var domException = exception.Error.AsObject();
        Assert.Equal("HierarchyRequestError", domException.Get("name").AsString());
        Assert.Equal("Invalid hierarchy", domException.Get("message").AsString());
        Assert.Equal(3, domException.Get("code").AsNumber());
    }

    [Fact]
    public void ShouldThrowInvalidCharacterError()
    {
        var exception = Assert.Throws<JavaScriptException>(
            () =>
                DOMExceptionHelper.CreateInvalidCharacterError(Engine, "Invalid character").Throw()
        );

        var domException = exception.Error.AsObject();
        Assert.Equal("InvalidCharacterError", domException.Get("name").AsString());
        Assert.Equal("Invalid character", domException.Get("message").AsString());
        Assert.Equal(5, domException.Get("code").AsNumber());
    }

    [Fact]
    public void ShouldThrowNotFoundError()
    {
        var exception = Assert.Throws<JavaScriptException>(
            () => DOMExceptionHelper.CreateNotFoundError(Engine, "Element not found").Throw()
        );

        var domException = exception.Error.AsObject();
        Assert.Equal("NotFoundError", domException.Get("name").AsString());
        Assert.Equal("Element not found", domException.Get("message").AsString());
        Assert.Equal(8, domException.Get("code").AsNumber());
    }

    [Fact]
    public void ShouldThrowSecurityError()
    {
        var exception = Assert.Throws<JavaScriptException>(
            () => DOMExceptionHelper.CreateSecurityError(Engine, "Security violation").Throw()
        );

        var domException = exception.Error.AsObject();
        Assert.Equal("SecurityError", domException.Get("name").AsString());
        Assert.Equal("Security violation", domException.Get("message").AsString());
        Assert.Equal(18, domException.Get("code").AsNumber());
    }

    [Fact]
    public void ShouldThrowNetworkError()
    {
        var exception = Assert.Throws<JavaScriptException>(
            () => DOMExceptionHelper.CreateNetworkError(Engine, "Network failure").Throw()
        );

        var domException = exception.Error.AsObject();
        Assert.Equal("NetworkError", domException.Get("name").AsString());
        Assert.Equal("Network failure", domException.Get("message").AsString());
        Assert.Equal(19, domException.Get("code").AsNumber());
    }

    [Fact]
    public void ShouldThrowAbortError()
    {
        var exception = Assert.Throws<JavaScriptException>(
            () => DOMExceptionHelper.CreateAbortError(Engine, "Operation aborted").Throw()
        );

        var domException = exception.Error.AsObject();
        Assert.Equal("AbortError", domException.Get("name").AsString());
        Assert.Equal("Operation aborted", domException.Get("message").AsString());
        Assert.Equal(20, domException.Get("code").AsNumber());
    }

    [Fact]
    public void ShouldThrowTimeoutError()
    {
        var exception = Assert.Throws<JavaScriptException>(
            () => DOMExceptionHelper.CreateTimeoutError(Engine, "Operation timed out").Throw()
        );

        var domException = exception.Error.AsObject();
        Assert.Equal("TimeoutError", domException.Get("name").AsString());
        Assert.Equal("Operation timed out", domException.Get("message").AsString());
        Assert.Equal(23, domException.Get("code").AsNumber());
    }

    [Fact]
    public void ShouldThrowDataCloneError()
    {
        var exception = Assert.Throws<JavaScriptException>(
            () => DOMExceptionHelper.CreateDataCloneError(Engine, "Cannot clone data").Throw()
        );

        var domException = exception.Error.AsObject();
        Assert.Equal("DataCloneError", domException.Get("name").AsString());
        Assert.Equal("Cannot clone data", domException.Get("message").AsString());
        Assert.Equal(25, domException.Get("code").AsNumber());
    }

    [Fact]
    public void ShouldThrowWithEmptyMessage()
    {
        var exception = Assert.Throws<JavaScriptException>(
            () => DOMExceptionHelper.CreateNotSupportedError(Engine).Throw()
        );

        var domException = exception.Error.AsObject();
        Assert.Equal("NotSupportedError", domException.Get("name").AsString());
        Assert.Equal("", domException.Get("message").AsString());
        Assert.Equal(9, domException.Get("code").AsNumber());
    }

    [Fact]
    public void ShouldThrowGenericDOMException()
    {
        var exception = Assert.Throws<JavaScriptException>(
            () => DOMExceptionHelper.Create(Engine, "CustomError", "Custom message").Throw()
        );

        var domException = exception.Error.AsObject();
        Assert.Equal("CustomError", domException.Get("name").AsString());
        Assert.Equal("Custom message", domException.Get("message").AsString());
        Assert.Equal(0, domException.Get("code").AsNumber()); // Unknown error should have code 0
    }
}
