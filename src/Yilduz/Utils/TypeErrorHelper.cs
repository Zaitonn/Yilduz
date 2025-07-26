using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Runtime;

namespace Yilduz.Utils;

internal static class TypeErrorHelper
{
    [DoesNotReturn]
    public static void Throw(Engine engine, string message)
    {
        throw new JavaScriptException(engine.Intrinsics.TypeError, message);
    }

    [DoesNotReturn]
    public static void Throw(
        Engine engine,
        string message,
        string functionName,
        string? objectName = null
    )
    {
        if (string.IsNullOrEmpty(objectName))
        {
            throw new JavaScriptException(
                engine.Intrinsics.TypeError,
                $"Failed to execute '{functionName}': {message}"
            );
        }
        else
        {
            throw new JavaScriptException(
                engine.Intrinsics.TypeError,
                $"Failed to execute '{functionName}' on '{objectName}': {message}"
            );
        }
    }
}
