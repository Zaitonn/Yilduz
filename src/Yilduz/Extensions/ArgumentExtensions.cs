using Jint;
using Jint.Native;
using Yilduz.Utils;

namespace Yilduz.Extensions;

internal static class ArgumentExtensions
{
    private static string GetMessage(int count, int present)
    {
        return count == 1
            ? $"{count} argument required, but only {present} present."
            : $"{count} arguments required, but only {present} present.";
    }

    public static void EnsureCount(
        this JsValue[] arguments,
        Engine engine,
        int count,
        string message
    )
    {
        if (arguments.Length < count)
        {
            TypeErrorHelper.Throw(engine, $"{message}: " + GetMessage(count, arguments.Length));
        }
    }

    public static void EnsureCount(
        this JsValue[] arguments,
        Engine engine,
        int count,
        string functionName,
        string? objectName
    )
    {
        if (arguments.Length < count)
        {
            TypeErrorHelper.Throw(
                engine,
                GetMessage(count, arguments.Length),
                functionName,
                objectName
            );
        }
    }

    public static string ToArgumentString(this JsValue jsValue)
    {
        return jsValue is JsArray jsArray ? string.Join(",", jsArray) : jsValue.ToString();
    }
}
