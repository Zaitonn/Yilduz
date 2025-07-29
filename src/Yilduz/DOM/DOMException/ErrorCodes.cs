using System.Collections.Generic;
#if NET8_0
using System.Collections.Frozen;
#endif

namespace Yilduz.DOM.DOMException;

/// <summary>
/// DOMException error name to legacy code mapping
/// https://webidl.spec.whatwg.org/#dfn-error-names-table
/// </summary>
public static class ErrorCodes
{
    static ErrorCodes()
    {
        var codes = new Dictionary<string, int>
        {
            { "IndexSizeError", 1 },
            { "HierarchyRequestError", 3 },
            { "WrongDocumentError", 4 },
            { "InvalidCharacterError", 5 },
            { "NoModificationAllowedError", 7 },
            { "NotFoundError", 8 },
            { "NotSupportedError", 9 },
            { "InUseAttributeError", 10 },
            { "InvalidStateError", 11 },
            { "SyntaxError", 12 },
            { "InvalidModificationError", 13 },
            { "NamespaceError", 14 },
            { "InvalidAccessError", 15 },
            { "SecurityError", 18 },
            { "NetworkError", 19 },
            { "AbortError", 20 },
            { "URLMismatchError", 21 },
            { "QuotaExceededError", 22 },
            { "TimeoutError", 23 },
            { "InvalidNodeTypeError", 24 },
            { "DataCloneError", 25 },
        };

        var names = new Dictionary<int, string>
        {
            { 1, "INDEX_SIZE_ERR" },
            { 3, "HIERARCHY_REQUEST_ERR" },
            { 4, "WRONG_DOCUMENT_ERR" },
            { 5, "INVALID_CHARACTER_ERR" },
            { 7, "NO_MODIFICATION_ALLOWED_ERR" },
            { 8, "NOT_FOUND_ERR" },
            { 9, "NOT_SUPPORTED_ERR" },
            { 10, "INUSE_ATTRIBUTE_ERR" },
            { 11, "INVALID_STATE_ERR" },
            { 12, "SYNTAX_ERR" },
            { 13, "INVALID_MODIFICATION_ERR" },
            { 14, "NAMESPACE_ERR" },
            { 15, "INVALID_ACCESS_ERR" },
            { 18, "SECURITY_ERR" },
            { 19, "NETWORK_ERR" },
            { 20, "ABORT_ERR" },
            { 21, "URL_MISMATCH_ERR" },
            { 22, "QUOTA_EXCEEDED_ERR" },
            { 23, "TIMEOUT_ERR" },
            { 24, "INVALID_NODE_TYPE_ERR" },
            { 25, "DATA_CLONE_ERR" },
        };

#if NET8_0
        Codes = codes.ToFrozenDictionary();
        CodeConstantNames = names.ToFrozenDictionary();
#else
        Codes = codes;
        CodeConstantNames = names;
#endif
    }

    public static readonly IReadOnlyDictionary<string, int> Codes;

    public static readonly IReadOnlyDictionary<int, string> CodeConstantNames;
}
