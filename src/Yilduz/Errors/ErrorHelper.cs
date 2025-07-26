using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Errors;

internal static class ErrorHelper
{
    private static ObjectInstance CreateError(this Engine engine, JsValue message, string name)
    {
        var error = engine.Intrinsics.Error.Construct([message], JsValue.Undefined);
        error.Set("name", name);
        return error;
    }

    public static ObjectInstance CreateAbortErrorInstance(this Engine engine, JsValue message)
    {
        return engine.CreateError(message, "AbortError");
    }

    public static ObjectInstance CreateTimeoutErrorInstance(this Engine engine, JsValue message)
    {
        return engine.CreateError(message, "TimeoutError");
    }
}
