namespace Yilduz.Extensions;

#pragma warning disable IDE0046

internal static class NamingExtensions
{
    public static string ToJsStyleName(this string name)
    {
        if (name.Length > 1)
        {
            return char.ToLowerInvariant(name[0]) + name[1..];
        }

        return name.ToLowerInvariant();
    }

    public static string ToJsGetterName(this string name)
    {
        return "get " + name.ToJsStyleName();
    }

    public static string ToJsSetterName(this string name)
    {
        return "set " + name.ToJsStyleName();
    }
}
