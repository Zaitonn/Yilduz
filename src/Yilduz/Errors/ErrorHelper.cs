using Jint;
using Jint.Native;
using Jint.Native.Object;

namespace Yilduz.Errors;

internal static class ErrorHelper
{
    public static ObjectInstance CreateError(this Engine engine, string name, JsValue message)
    {
        var error = engine.Intrinsics.Error.Construct([message], JsValue.Undefined);
        error.Set("name", name);
        return error;
    }
}
