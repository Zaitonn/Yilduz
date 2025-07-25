using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;

namespace Yilduz.Utils;

internal static class JsValueExtensions
{
    public static T EnsureThisObject<T>(this JsValue thisObject)
        where T : ObjectInstance
    {
        var obj = thisObject.AsObject();

        return thisObject.As<T>()
            ?? throw new JavaScriptException(obj.Engine.Intrinsics.TypeError, "Illegal invocation");
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
