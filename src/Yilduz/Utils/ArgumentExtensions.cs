using Jint;
using Jint.Native;

namespace Yilduz.Utils;

internal static class ArgumentExtensions
{
    private static string GetMessage(int count, int present)
    {
        return count == 1
            ? $"{count} argument required, but only {present} present."
            : $"{count} arguments required, but only {present} present.";
    }

    public static void EnsureCount(this JsValue[] arguments, int count, Engine engine)
    {
        if (arguments.Length < count)
        {
            TypeErrorHelper.Throw(GetMessage(count, arguments.Length), engine);
        }
    }

    public static void EnsureCount(
        this JsValue[] arguments,
        int count,
        Engine engine,
        string message
    )
    {
        if (arguments.Length < count)
        {
            TypeErrorHelper.Throw($"{message}: " + GetMessage(count, arguments.Length), engine);
        }
    }

    public static void EnsureCount(
        this JsValue[] arguments,
        int count,
        Engine engine,
        string functionName,
        string? objectName
    )
    {
        if (arguments.Length < count)
        {
            TypeErrorHelper.Throw(
                GetMessage(count, arguments.Length),
                engine,
                functionName,
                objectName
            );
        }
    }
}
