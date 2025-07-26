using Jint;
using Jint.Native;
using Jint.Native.Error;
using Jint.Native.Object;

namespace Yilduz.Errors;

internal static class ErrorHelper
{
    public static ErrorInstance? GetErrorPrototype(this Engine engine)
    {
        return engine.Global.Get("Error").Get("prototype").As<ErrorInstance>();
    }

    public static ObjectInstance CreateAbortError(this Engine engine, JsValue message)
    {
        var error = engine.Intrinsics.Error.Construct([message], JsValue.Undefined);
        error.Set("name", "AbortError");
        return error;
    }
}
