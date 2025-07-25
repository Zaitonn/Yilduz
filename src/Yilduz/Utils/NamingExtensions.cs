using System;

namespace Yilduz.Utils;

#pragma warning disable IDE0046

internal static class NamingExtensions
{
    public static string ToJsStyleName(this string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));
        }

        if (name.Length > 1)
        {
            return char.ToLowerInvariant(name[0]) + name[1..];
        }

        return name.ToLowerInvariant();
    }

    public static string ToJsGetterName(this string name)
    {
        return "get " + ToJsStyleName(name);
    }

    public static string ToJsSetterName(this string name)
    {
        return "set " + ToJsStyleName(name);
    }
}
