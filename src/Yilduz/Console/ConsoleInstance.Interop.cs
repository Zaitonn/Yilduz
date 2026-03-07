using System.Linq;
using Jint;
using Jint.Native;
using Yilduz.Extensions;

namespace Yilduz.Console;

public sealed partial class ConsoleInstance
{
    private JsValue Assert(JsValue thisObject, JsValue[] arguments)
    {
        var condition = arguments.Length > 0 && arguments[0].ConvertToBoolean();
        Assert(condition, arguments.Length > 1 ? [.. arguments.Skip(1)] : []);
        return Undefined;
    }

    private JsValue Clear(JsValue thisObject, JsValue[] arguments)
    {
        Clear();
        return Undefined;
    }

    private JsValue Count(JsValue thisObject, JsValue[] arguments)
    {
        var label = arguments.Length > 0 ? arguments[0].ToString() : "default";
        Count(label);
        return Undefined;
    }

    private JsValue CountReset(JsValue thisObject, JsValue[] arguments)
    {
        var label = arguments.Length > 0 ? arguments[0].ToString() : "default";
        CountReset(label);
        return Undefined;
    }

    private JsValue Debug(JsValue thisObject, JsValue[] arguments)
    {
        Debug(arguments);
        return Undefined;
    }

    private JsValue Dir(JsValue thisObject, JsValue[] arguments)
    {
        var item = arguments.Length > 0 ? arguments[0] : JsValue.Undefined;
        var options = arguments.Length > 1 ? arguments[1] : JsValue.Undefined;
        Dir(item, options);
        return Undefined;
    }

    private JsValue Dirxml(JsValue thisObject, JsValue[] arguments)
    {
        Dirxml(arguments);
        return Undefined;
    }

    private JsValue Error(JsValue thisObject, JsValue[] arguments)
    {
        Error(arguments);
        return Undefined;
    }

    private JsValue Group(JsValue thisObject, JsValue[] arguments)
    {
        Group(arguments);
        return Undefined;
    }

    private JsValue GroupCollapsed(JsValue thisObject, JsValue[] arguments)
    {
        GroupCollapsed(arguments);
        return Undefined;
    }

    private JsValue GroupEnd(JsValue thisObject, JsValue[] arguments)
    {
        GroupEnd();
        return Undefined;
    }

    private JsValue Info(JsValue thisObject, JsValue[] arguments)
    {
        Info(arguments);
        return Undefined;
    }

    private JsValue Log(JsValue thisObject, JsValue[] arguments)
    {
        Log(arguments);
        return Undefined;
    }

    private JsValue Table(JsValue thisObject, JsValue[] arguments)
    {
        var tabularData = arguments.Length > 0 ? arguments[0] : JsValue.Undefined;
        var properties =
            arguments.Length > 1 && arguments[1].IsArray()
                ? arguments[1].AsArray().Select(v => v.ToString()).ToArray()
                : null;
        Table(tabularData, properties);
        return Undefined;
    }

    private JsValue Time(JsValue thisObject, JsValue[] arguments)
    {
        var label = arguments.Length > 0 ? arguments[0].ToString() : "default";
        Time(label);
        return Undefined;
    }

    private JsValue TimeEnd(JsValue thisObject, JsValue[] arguments)
    {
        var label = arguments.Length > 0 ? arguments[0].ToString() : "default";
        TimeEnd(label);
        return Undefined;
    }

    private JsValue TimeLog(JsValue thisObject, JsValue[] arguments)
    {
        var label = arguments.Length > 0 ? arguments[0].ToString() : "default";
        TimeLog(label, arguments.Length > 1 ? [.. arguments.Skip(1)] : []);
        return Undefined;
    }

    private JsValue TimeStamp(JsValue thisObject, JsValue[] arguments)
    {
        var label = arguments.Length > 0 ? arguments[0].ToString() : "default";
        TimeStamp(label);
        return Undefined;
    }

    private JsValue Trace(JsValue thisObject, JsValue[] arguments)
    {
        Trace(arguments);
        return Undefined;
    }

    private JsValue Warn(JsValue thisObject, JsValue[] arguments)
    {
        Warn(arguments);
        return Undefined;
    }
}
