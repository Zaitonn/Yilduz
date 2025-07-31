using System;
using System.Collections.Generic;
using System.Linq;

namespace Yilduz.Network;

internal static class HttpHelper
{
    private static readonly HashSet<string> ForbiddenHeaderNames =
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
        "te",
        "trailer",
        "transfer-encoding",
        "upgrade",
        "user-agent",
    ];

    public static bool IsForbiddenHeader(string name, string value)
    {
        if (ForbiddenHeaderNames.Contains(name.ToLowerInvariant()))
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
            name.Equals("X-HTTP-Method", StringComparison.InvariantCultureIgnoreCase)
            || name.Equals("X-HTTP-Method-Override", StringComparison.InvariantCultureIgnoreCase)
            || name.Equals("X-Method-Override", StringComparison.InvariantCultureIgnoreCase)
        )
        {
            return GetDecodeAndSplit(value).Any(IsForbiddenMethod);
        }

        return false;
    }

    public static string[] GetDecodeAndSplit(string value)
    {
        return value.Split(',');
    }

    public static bool IsHeaderValue(string value)
    {
        if (value.StartsWith("\x20") || value.EndsWith("\x20"))
        {
            return false;
        }

        if (value.StartsWith("\x09") || value.EndsWith("\x09"))
        {
            return false;
        }

        return !value.Contains('\0')
            && !value.Contains('\n')
            && !value.Contains('\r')
            && !value.Contains(':');
    }

    public static bool IsForbiddenMethod(string method)
    {
        return method.Equals("CONNECT", StringComparison.InvariantCultureIgnoreCase)
            || method.Equals("TRACE", StringComparison.InvariantCultureIgnoreCase)
            || method.Equals("TRACK", StringComparison.InvariantCultureIgnoreCase);
    }
}
