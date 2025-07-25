using System;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;

namespace Yilduz.Utils;

#pragma warning disable IDE0046

internal static class Extensions
{
    public static string ToJsStylePropertyName(this string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (name.Length > 1)
        {
            return char.ToLowerInvariant(name[0]) + name[1..];
        }

        return name.ToLowerInvariant();
    }

    public static string ToJsGetterName(this string name)
    {
        return "get " + ToJsStylePropertyName(name);
    }

    public static T EnsureThisObject<T>(this JsValue thisObject)
        where T : ObjectInstance
    {
        return thisObject.As<T>() ?? throw new JavaScriptException("Illegal invocation");
    }

    public static bool ToBoolean(this JsValue value)
    {
        return value switch
        {
            JsBoolean jsBoolean => jsBoolean.AsBoolean(),
            JsNumber jsNumber => jsNumber != 0,
            JsString jsString => !string.IsNullOrEmpty(jsString.ToString()),
            _ => !value.IsNull() && !value.IsUndefined(),
        };
    }
}
