using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yilduz.Network.Body;
using Yilduz.Network.Headers;
using Yilduz.Network.Request;
using Yilduz.Network.Response;

namespace Yilduz.Network;

internal static class MIMETypeHelper
{
    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-body-mime-type
    /// </summary>
    public static MIMEType? Get(BodyInstance body)
    {
        var headers = body switch
        {
            RequestInstance request => request.Headers,
            ResponseInstance response => response.Headers,
            _ => throw new InvalidOperationException(
                "BodyInstance must be either RequestInstance or ResponseInstance"
            ),
        };

        try
        {
            var mimeType = Extract(headers);
            return mimeType;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-header-extract-mime-type
    /// </summary>
    private static MIMEType Extract(HeadersInstance headers)
    {
        var values =
            HttpHelper.GetDecodeAndSplit(headers.Get("Content-Type"))
            ?? throw new InvalidOperationException("Content-Type header is not set");
        string? charset = null;
        var essence = string.Empty;
        MIMEType? mimeType = null;

        foreach (var value in values)
        {
            try
            {
                var temporaryMimeType = Parse(value);
                if (temporaryMimeType.Essence == "*/*")
                {
                    continue;
                }

                mimeType = temporaryMimeType;
                if (mimeType.Essence != essence)
                {
                    charset = null;

                    if (mimeType.Parameters.TryGetValue("charset", out var charsetValue))
                    {
                        charset = charsetValue;
                    }

                    essence = mimeType.Essence;
                }
                else if (mimeType.Parameters.ContainsKey("charset") && charset is not null)
                {
                    mimeType.Parameters["charset"] = charset;
                }
            }
            catch { }
        }

        if (mimeType == null)
        {
            throw new InvalidOperationException("No valid MIME type found in Content-Type header");
        }

        return mimeType;
    }

    /// <summary>
    /// https://mimesniff.spec.whatwg.org/#parse-a-mime-type
    /// </summary>
    public static MIMEType Parse(string value)
    {
        // 1. Remove any leading and trailing HTTP whitespace from input.
        var input = value.Trim();

        // 2. Let position be a position variable for input, initially pointing at the start of input.
        var position = 0;

        // 3. Let type be the result of collecting a sequence of code points that are not U+002F (/) from input, given position.
        var type = CollectSequenceOfCodePoints(input, ref position, c => c != '/');

        // 4. If type is the empty string or does not solely contain HTTP token code points, then return failure.
        if (
            string.IsNullOrEmpty(type)
            || type.Any(c => !char.IsLetterOrDigit(c) && !TokenCodePoints.Contains(c))
        )
        {
            throw new InvalidOperationException("Invalid MIME type");
        }

        // 5. If position is past the end of input, then return failure.
        if (position >= input.Length)
        {
            throw new InvalidOperationException("Invalid MIME type");
        }

        // 6. Advance position by 1. (This skips past U+002F (/).)
        position++;

        // 7. Let subtype be the result of collecting a sequence of code points that are not U+003B (;) from input, given position.
        var subtype = CollectSequenceOfCodePoints(input, ref position, c => c != ';');

        // 8. Remove any trailing HTTP whitespace from subtype.
        subtype = subtype.TrimEnd();

        // 9. If subtype is the empty string or does not solely contain HTTP token code points, then return failure.
        if (
            string.IsNullOrEmpty(subtype)
            || subtype.Any(c => !char.IsLetterOrDigit(c) && !TokenCodePoints.Contains(c))
        )
        {
            throw new InvalidOperationException("Invalid MIME type");
        }

        // 10. Let mimeType be a new MIME type record whose type is type, in ASCII lowercase, and subtype is subtype, in ASCII lowercase.
        var mimeType = new MIMEType(type.ToLowerInvariant(), subtype.ToLowerInvariant());

        // 11. While position is not past the end of input:
        while (position < input.Length)
        {
            // 11.1. Advance position by 1. (This skips past U+003B (;).)
            position++;

            // 11.2. Collect a sequence of code points that are HTTP whitespace from input given position.
            CollectSequenceOfCodePoints(input, ref position, IsHTTPWhitespace);

            // 11.3. Let parameterName be the result of collecting a sequence of code points that are not U+003B (;) or U+003D (=) from input, given position.
            var parameterName = CollectSequenceOfCodePoints(
                input,
                ref position,
                c => c != ';' && c != '='
            );

            // 11.4. Set parameterName to parameterName, in ASCII lowercase.
            parameterName = parameterName.ToLowerInvariant();

            // 11.5. If position is not past the end of input, then:
            if (position < input.Length)
            {
                // 11.5.1. If the code point at position within input is U+003B (;), then continue.
                if (input[position] == ';')
                {
                    continue;
                }

                // 11.5.2. Advance position by 1. (This skips past U+003D (=).)
                position++;
            }

            // 11.6. If position is past the end of input, then break.
            if (position >= input.Length)
            {
                break;
            }

            // 11.7. Let parameterValue be null.
            string? parameterValue;

            // 11.8. If the code point at position within input is U+0022 ("), then:
            if (input[position] == '"')
            {
                // 11.8.1. Set parameterValue to the result of collecting an HTTP quoted string from input, given position and true.
                parameterValue = HttpHelper.CollectHTTPQuotedString(input, ref position, true);

                // 11.8.2. Collect a sequence of code points that are not U+003B (;) from input, given position.
                CollectSequenceOfCodePoints(input, ref position, c => c != ';');
            }
            else
            {
                // 11.9.1. Set parameterValue to the result of collecting a sequence of code points that are not U+003B (;) from input, given position.
                parameterValue = CollectSequenceOfCodePoints(input, ref position, c => c != ';');

                // 11.9.2. Remove any trailing HTTP whitespace from parameterValue.
                parameterValue = parameterValue.TrimEnd();

                // 11.9.3. If parameterValue is the empty string, then continue.
                if (string.IsNullOrEmpty(parameterValue))
                {
                    continue;
                }
            }

            // 11.10. If all of the following are true, then set mimeType’s parameters[parameterName] to parameterValue.
            if (
                !string.IsNullOrEmpty(parameterName)
                && parameterName.All(c => char.IsLetterOrDigit(c) || TokenCodePoints.Contains(c))
                && parameterValue.All(IsHTTPQuotedStringTokenCodePoint)
#if NETCOREAPP
            )
            {
                mimeType.Parameters.TryAdd(parameterName, parameterValue);
            }
#else
                && !mimeType.Parameters.ContainsKey(parameterName)
            )
            {
                mimeType.Parameters[parameterName] = parameterValue;
            }
#endif
        }

        // 13. Return mimeType.
        return mimeType;
    }

    private static bool IsHTTPWhitespace(char c)
    {
        // Helper: HTTP whitespace is HTAB, SP, CR, or LF.
        return c is '\t' or '\n' or '\r' or ' ';
    }

    private static bool IsHTTPQuotedStringTokenCodePoint(char c)
    {
        // Helper: HTTP quoted-string token code point per mimesniff spec.
        return c == '\t' || (c >= 0x20 && c <= 0x7E && c != '"' && c != '\\');
    }

    private static readonly HashSet<char> TokenCodePoints =
    [
        '!',
        '#',
        '$',
        '%',
        '&',
        '\'',
        '*',
        '+',
        '-',
        '.',
        '^',
        '_',
        '`',
        '|',
        '~',
    ];

    /// <summary>
    /// https://infra.spec.whatwg.org/#collect-a-sequence-of-code-points
    /// </summary>
    private static string CollectSequenceOfCodePoints(
        string value,
        ref int position,
        Func<char, bool> predicate
    )
    {
        var result = string.Empty;

        while (position < value.Length)
        {
            var c = value[position];

            if (!predicate(c))
            {
                break;
            }

            result += c;
            position++;
        }

        return result;
    }

    /// <summary>
    /// https://mimesniff.spec.whatwg.org/#serialize-a-mime-type
    /// </summary>
    public static string Serialize(MIMEType mimeType)
    {
        // Let serialization be the concatenation of mimeType’s type, U+002F (/), and mimeType’s subtype.
        var serialization = new StringBuilder($"{mimeType.Type}/{mimeType.Subtype}");

        // For each name → value of mimeType’s parameters:
#if NETCOREAPP
        foreach (var (name, value) in mimeType.Parameters)
        {
#else
        foreach (var kvp in mimeType.Parameters)
        {
            var name = kvp.Key;
            var value = kvp.Value;
#endif
            // Append U+003B (;) to serialization.
            serialization.Append(';');

            // Append name to serialization.
            serialization.Append(name);

            // Append U+003D (=) to serialization.
            serialization.Append('=');

            // If value does not solely contain HTTP token code points or value is the empty string, then:
            if (string.IsNullOrEmpty(value) || value.Any(c => !TokenCodePoints.Contains(c)))
            {
                // Precede each occurrence of U+0022 (") or U+005C (\) in value with U+005C (\).
                var v = value.Replace("\\", @"\\").Replace("\"", "\\\"");

                // Prepend U+0022 (") to value
                // Append U+0022 (") to value
                v = $"\"{v}\"";

                // Append value to serialization
                serialization.Append(v);
            }
            else
            {
                // Append value to serialization
                serialization.Append(value);
            }
        }

        // Return serialization.
        return serialization.ToString();
    }
}
