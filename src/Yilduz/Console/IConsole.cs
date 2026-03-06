using Jint.Native;

namespace Yilduz.Console;

/// <summary>
/// Represents the console object.
/// </summary>
public interface IConsole
{
    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/assert_static
    /// </summary>
    void Assert(bool condition, params JsValue[] data);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/clear_static
    /// </summary>
    void Clear();

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/count_static
    /// </summary>
    void Count(string label);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/countReset_static
    /// </summary>
    void CountReset(string label);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/debug_static
    /// </summary>
    void Debug(params JsValue[] data);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/dir_static
    /// </summary>
    void Dir(JsValue item, JsValue options);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/dirxml_static
    /// </summary>
    void Dirxml(params JsValue[] data);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/error_static
    /// </summary>
    void Error(params JsValue[] data);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/group_static
    /// </summary>
    void Group(params JsValue[] data);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/groupCollapsed_static
    /// </summary>
    void GroupCollapsed(params JsValue[] data);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/groupEnd_static
    /// </summary>
    void GroupEnd();

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/info_static
    /// </summary>
    void Info(params JsValue[] data);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/log_static
    /// </summary>
    void Log(params JsValue[] data);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/table_static
    /// </summary>
    void Table(JsValue tabularData, string[]? properties = null);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/time_static
    /// </summary>
    void Time(string label);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/timeEnd_static
    /// </summary>
    void TimeEnd(string label);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/timeLog_static
    /// </summary>
    void TimeLog(string label, params JsValue[] data);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/timeStamp_static
    /// </summary>
    void TimeStamp(string label);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/trace_static
    /// </summary>
    void Trace(params JsValue[] data);

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/console/warn_static
    /// </summary>
    void Warn(params JsValue[] data);
}
