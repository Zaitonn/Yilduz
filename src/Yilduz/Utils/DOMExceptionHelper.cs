using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Runtime;
using Yilduz.DOM.DOMException;

namespace Yilduz.Utils;

internal static class DOMExceptionHelper
{
    [DoesNotReturn]
    public static void Throw(this DOMExceptionInstance domException)
    {
        throw new JavaScriptException(domException);
    }

    public static DOMExceptionInstance Create(Engine engine, string name, string message = "")
    {
        var domExceptionConstructor = engine.GetWebApiIntrinsics().DOMException;
        var domException = domExceptionConstructor.CreateInstance(name, message);
        return domException;
    }

    public static DOMExceptionInstance CreateIndexSizeError(Engine engine, string message = "")
    {
        return Create(engine, "IndexSizeError", message);
    }

    public static DOMExceptionInstance CreateHierarchyRequestError(
        Engine engine,
        string message = ""
    )
    {
        return Create(engine, "HierarchyRequestError", message);
    }

    public static DOMExceptionInstance CreateWrongDocumentError(Engine engine, string message = "")
    {
        return Create(engine, "WrongDocumentError", message);
    }

    public static DOMExceptionInstance CreateInvalidCharacterError(
        Engine engine,
        string message = ""
    )
    {
        return Create(engine, "InvalidCharacterError", message);
    }

    public static DOMExceptionInstance CreateNoModificationAllowedError(
        Engine engine,
        string message = ""
    )
    {
        return Create(engine, "NoModificationAllowedError", message);
    }

    public static DOMExceptionInstance CreateNotFoundError(Engine engine, string message = "")
    {
        return Create(engine, "NotFoundError", message);
    }

    public static DOMExceptionInstance CreateNotSupportedError(Engine engine, string message = "")
    {
        return Create(engine, "NotSupportedError", message);
    }

    public static DOMExceptionInstance CreateInUseAttributeError(Engine engine, string message = "")
    {
        return Create(engine, "InUseAttributeError", message);
    }

    public static DOMExceptionInstance CreateInvalidStateError(Engine engine, string message = "")
    {
        return Create(engine, "InvalidStateError", message);
    }

    public static DOMExceptionInstance CreateSyntaxError(Engine engine, string message = "")
    {
        return Create(engine, "SyntaxError", message);
    }

    public static DOMExceptionInstance CreateInvalidModificationError(
        Engine engine,
        string message = ""
    )
    {
        return Create(engine, "InvalidModificationError", message);
    }

    public static DOMExceptionInstance CreateNamespaceError(Engine engine, string message = "")
    {
        return Create(engine, "NamespaceError", message);
    }

    public static DOMExceptionInstance CreateInvalidAccessError(Engine engine, string message = "")
    {
        return Create(engine, "InvalidAccessError", message);
    }

    public static DOMExceptionInstance CreateSecurityError(Engine engine, string message = "")
    {
        return Create(engine, "SecurityError", message);
    }

    public static DOMExceptionInstance CreateNetworkError(Engine engine, string message = "")
    {
        return Create(engine, "NetworkError", message);
    }

    public static DOMExceptionInstance CreateAbortError(Engine engine, string message = "")
    {
        return Create(engine, "AbortError", message);
    }

    public static DOMExceptionInstance CreateURLMismatchError(Engine engine, string message = "")
    {
        return Create(engine, "URLMismatchError", message);
    }

    public static DOMExceptionInstance CreateQuotaExceededError(Engine engine, string message = "")
    {
        return Create(engine, "QuotaExceededError", message);
    }

    public static DOMExceptionInstance CreateTimeoutError(Engine engine, string message = "")
    {
        return Create(engine, "TimeoutError", message);
    }

    public static DOMExceptionInstance CreateInvalidNodeTypeError(
        Engine engine,
        string message = ""
    )
    {
        return Create(engine, "InvalidNodeTypeError", message);
    }

    public static DOMExceptionInstance CreateDataCloneError(Engine engine, string message = "")
    {
        return Create(engine, "DataCloneError", message);
    }

    public static DOMExceptionInstance CreateEncodingError(Engine engine, string message = "")
    {
        return Create(engine, "EncodingError", message);
    }

    public static DOMExceptionInstance CreateNotReadableError(Engine engine, string message = "")
    {
        return Create(engine, "NotReadableError", message);
    }

    public static DOMExceptionInstance CreateUnknownError(Engine engine, string message = "")
    {
        return Create(engine, "UnknownError", message);
    }

    public static DOMExceptionInstance CreateConstraintError(Engine engine, string message = "")
    {
        return Create(engine, "ConstraintError", message);
    }

    public static DOMExceptionInstance CreateDataError(Engine engine, string message = "")
    {
        return Create(engine, "DataError", message);
    }

    public static DOMExceptionInstance CreateTransactionInactiveError(
        Engine engine,
        string message = ""
    )
    {
        return Create(engine, "TransactionInactiveError", message);
    }

    public static DOMExceptionInstance CreateReadOnlyError(Engine engine, string message = "")
    {
        return Create(engine, "ReadOnlyError", message);
    }

    public static DOMExceptionInstance CreateVersionError(Engine engine, string message = "")
    {
        return Create(engine, "VersionError", message);
    }

    public static DOMExceptionInstance CreateOperationError(Engine engine, string message = "")
    {
        return Create(engine, "OperationError", message);
    }

    public static DOMExceptionInstance CreateNotAllowedError(Engine engine, string message = "")
    {
        return Create(engine, "NotAllowedError", message);
    }
}
