using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Yilduz.Network;

internal static class HttpHelper
{
    private static readonly HashSet<string> ForbiddenRequestHeaderNames =
    [
        "accept-charset",
        "accept-encoding",
        "access-control-request-headers",
        "access-control-request-method",
        "connection",
        "content-length",
        "cookie",
        "cookie2",
        "date",
        "dnt",
        "expect",
        "host",
        "keep-alive",
        "origin",
        "referer",
        "set-cookie",
        "set-cookie2",
        "te",
        "trailer",
        "transfer-encoding",
        "upgrade",
        "via",
    ];

    public static bool IsForbiddenRequestHeader(string name, string value)
    {
        if (ForbiddenRequestHeaderNames.Contains(name.ToLowerInvariant()))
        {
            return true;
        }

        if (
            name.StartsWith("sec-", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("proxy-", StringComparison.OrdinalIgnoreCase)
        )
        {
            return true;
        }

        if (
            name.Equals("X-HTTP-Method", StringComparison.OrdinalIgnoreCase)
            || name.Equals("X-HTTP-Method-Override", StringComparison.OrdinalIgnoreCase)
            || name.Equals("X-Method-Override", StringComparison.OrdinalIgnoreCase)
        )
        {
            return GetDecodeAndSplit(value).Any(IsForbiddenMethod);
        }

        return false;
    }

    [return: NotNullIfNotNull(nameof(value))]
    public static string[]? GetDecodeAndSplit(string? value)
    {
        return value?.Split(',');
    }

    public static bool IsHeaderName([NotNullWhen(true)] string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        foreach (var c in name)
        {
            if (c < 0x21 || c > 0x7E)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#header-value
    /// </summary>
    public static bool IsHeaderValue([NotNullWhen(true)] string? value)
    {
#if NETSTANDARD
        var space = "\x20";
        var tab = "\x09";
#else
        var space = '\x20';
        var tab = '\x09';
#endif

        // Has no leading or trailing HTTP tab or space bytes.
        if (
            value is null
            || value.StartsWith(space)
            || value.EndsWith(space)
            || value.StartsWith(tab)
            || value.EndsWith(tab)
        )
        {
            return false;
        }

        // Contains no 0x00 (NUL) or HTTP newline bytes.
        return !value.Contains('\0') && !value.Contains('\n') && !value.Contains('\r');
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#forbidden-request-header
    /// </summary>
    public static bool IsForbiddenRequestHeader(string name)
    {
        // If name is a byte-case-insensitive match for one of:
        // then return true.
        if (ForbiddenRequestHeaderNames.Contains(name, StringComparer.OrdinalIgnoreCase))
        {
            return true;
        }

        // If name when byte-lowercased starts with `proxy-` or `sec-`, then return true.
        if (
            name.StartsWith("sec-", StringComparison.OrdinalIgnoreCase)
            || name.StartsWith("proxy-", StringComparison.OrdinalIgnoreCase)
        )
        {
            return true;
        }

        // If name is a byte-case-insensitive match for one of:
        if (
            name.Equals("X-HTTP-Method", StringComparison.OrdinalIgnoreCase)
            || name.Equals("X-HTTP-Method-Override", StringComparison.OrdinalIgnoreCase)
            || name.Equals("X-Method-Override", StringComparison.OrdinalIgnoreCase)
        )
        // then:
        {
            // Let parsedValues be the result of getting, decoding, and splitting value.
            // For each method of parsedValues: if the isomorphic encoding of method is a forbidden method, then return true.

            var parsedValues = GetDecodeAndSplit(name);
            return parsedValues.Any(IsForbiddenMethod);
        }

        return false;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#forbidden-response-header-name
    /// </summary>
    public static bool IsForbiddenResponseHeader(string name)
    {
        return name.Equals("set-cookie", StringComparison.OrdinalIgnoreCase)
            || name.Equals("set-cookie2", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsForbiddenMethod(string method)
    {
        return method.Equals("CONNECT", StringComparison.OrdinalIgnoreCase)
            || method.Equals("TRACE", StringComparison.OrdinalIgnoreCase)
            || method.Equals("TRACK", StringComparison.OrdinalIgnoreCase);
    }

    private static readonly HashSet<string> NoCORSUnsafeRequestHeaderNames =
    [
        "accept",
        "accept-language",
        "content-language",
        "content-type",
    ];

    /// <summary>
    /// https://fetch.spec.whatwg.org/#no-cors-safelisted-request-header-name
    /// </summary>
    public static bool IsNoCORSUnsafeRequestHeaderName(string name)
    {
        return NoCORSUnsafeRequestHeaderNames.Contains(name, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#no-cors-safelisted-request-header
    /// </summary>
    public static bool IsNoCORSUnsafeRequestHeader(string name, string value)
    {
        if (IsNoCORSUnsafeRequestHeaderName(name))
        {
            return false;
        }

        return IsCorsSafelistedRequestHeader(name, value);
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#cors-safelisted-request-header
    /// </summary>
    public static bool IsCorsSafelistedRequestHeader(string name, string value)
    {
        // If value’s length is greater than 128, then return false.
        if (value.Length > 128)
        {
            return false;
        }

        // Byte-lowercase name and switch on the result:
        switch (name.ToLowerInvariant())
        {
            case "accept":
                // If value contains a CORS-unsafe request-header byte, then return false.
                if (value.Any(IsCORSUnsafeRequestHeaderByte))
                {
                    return false;
                }
                break;

            case "accept-language":
            case "content-language":
                // If value contains a byte that is not in the range 0x30 (0) to 0x39 (9), inclusive, is not in the range 0x41 (A) to 0x5A (Z), inclusive, is not in the range 0x61 (a) to 0x7A (z), inclusive, and is not 0x20 (SP), 0x2A (*), 0x2C (,), 0x2D (-), 0x2E (.), 0x3B (;), or 0x3D (=), then return false.

                foreach (var c in value)
                {
                    if (
                        (c >= '0' && c <= '9')
                        || (c >= 'A' && c <= 'Z')
                        || (c >= 'a' && c <= 'z')
                        || c == '\x20'
                        || c == '*'
                        || c == ','
                        || c == '-'
                        || c == '.'
                        || c == ';'
                        || c == '='
                    )
                    {
                        continue;
                    }

                    return false;
                }
                break;

            case "content-type":
                // If value contains a CORS-unsafe request-header byte, then return false.
                if (value.Any(IsCORSUnsafeRequestHeaderByte))
                {
                    return false;
                }

                // Let mimeType be the result of parsing the result of isomorphic decoding value.
                // If mimeType is failure, then return false.
                // If mimeType’s essence is not "application/x-www-form-urlencoded", "multipart/form-data", or "text/plain", then return false.
                throw new NotImplementedException();

            case "range":
                try
                {
                    // Let rangeValue be the result of parsing a single range header value given value and false.
                    var (start, _) = ParseSingleRangeHeaderValue(value, false);

                    // If rangeValue[0] is null, then return false.
                    if (start == null)
                    {
                        return false;
                    }
                }
                catch
                {
                    // If rangeValue is failure, then return false.

                    return false;
                }
                break;

            // Otherwise, return false.
            default:
                return false;
        }

        return true;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#simple-range-header-value
    /// </summary>
    private static (int? Start, int? End) ParseSingleRangeHeaderValue(
        string value,
        bool allowWhitespace
    )
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#cors-unsafe-request-header-byte
    /// </summary>
    public static bool IsCORSUnsafeRequestHeaderByte(char c)
    {
        // a byte byte for which one of the following is true
        //  byte is less than 0x20 and is not 0x09 HT
        if (c < 0x20 && c != 0x09)
        {
            return true;
        }

        //  byte is 0x22 ("), 0x28 (left parenthesis), 0x29 (right parenthesis), 0x3A (:), 0x3C (<), 0x3E (>), 0x3F (?), 0x40 (@), 0x5B ([), 0x5C (\), 0x5D (]), 0x7B ({), 0x7D (}), or 0x7F DEL.
        return c == 0x22
            || c == 0x28
            || c == 0x29
            || c == 0x3A
            || c == 0x3C
            || c == 0x3E
            || c == 0x3F
            || c == 0x40
            || c == 0x5B
            || c == 0x5C
            || c == 0x5D
            || c == 0x7B
            || c == 0x7D
            || c == 0x7F;
    }

    public static bool IsPrivilegedNoCORSRequestHeaderName(string name)
    {
        return name.Equals("range", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#collect-an-http-quoted-string
    /// </summary>
    public static string CollectHTTPQuotedString(
        string input,
        ref int position,
        bool extractValue = false
    )
    {
        // 1. Let positionStart be position.
        var positionStart = position;

        // 2. Let value be the empty string.
        var value = string.Empty;

        // 3. Assert: the code point at position within input is U+0022 (").
        if (position >= input.Length || input[position] != '"')
        {
            throw new InvalidOperationException("Expected opening quote for HTTP quoted string");
        }

        // 4. Advance position by 1.
        position++;

        // 5. While true:
        while (true)
        {
            // 5.1. Append the result of collecting a sequence of code points that are not U+0022 (") or U+005C (\\) from input, given position, to value.
            value += CollectSequenceOfCodePoints(input, ref position, c => c != '"' && c != '\\');

            // 5.2. If position is past the end of input, then break.
            if (position >= input.Length)
            {
                break;
            }

            // 5.3. Let quoteOrBackslash be the code point at position within input.
            var quoteOrBackslash = input[position];

            // 5.4. Advance position by 1.
            position++;

            // 5.5. If quoteOrBackslash is U+005C (\\), then:
            if (quoteOrBackslash == '\\')
            {
                // 5.5.1. If position is past the end of input, then append U+005C (\\) to value and break.
                if (position >= input.Length)
                {
                    value += '\\';
                    break;
                }

                // 5.5.2. Append the code point at position within input to value.
                value += input[position];

                // 5.5.3. Advance position by 1.
                position++;
            }
            else
            {
                // 5.6.1. Assert: quoteOrBackslash is U+0022 (").
                if (quoteOrBackslash != '"')
                {
                    throw new InvalidOperationException("Invalid HTTP quoted string terminator");
                }

                // 5.6.2. Break.
                break;
            }
        }

        // 7. If extract-value is true, then return value.
        if (extractValue)
        {
            return value;
        }

        // 8. Return the code points from positionStart to position, inclusive, within input.
        return input[positionStart..position];
    }

    private static string CollectSequenceOfCodePoints(
        string input,
        ref int position,
        Func<char, bool> predicate
    )
    {
        var result = string.Empty;

        while (position < input.Length)
        {
            var c = input[position];

            if (!predicate(c))
            {
                break;
            }

            result += c;
            position++;
        }

        return result;
    }
}
