using Jint.Native;

namespace Yilduz.Console;

public interface IConsole
{
    /// <summary>
    /// Writes an error message if the specified condition is false.
    /// </summary>
    void Assert(bool condition, params JsValue[] data);

    /// <summary>
    /// Clears the console.
    /// </summary>
    void Clear();

    /// <summary>
    /// Logs the number of times this count has been called with the given label.
    /// </summary>
    void Count(string label);

    /// <summary>
    /// Resets the count for the given label.
    /// </summary>
    void CountReset(string label);

    /// <summary>
    /// Outputs a debug message.
    /// </summary>
    void Debug(params JsValue[] data);

    /// <summary>
    /// Displays an interactive list of the properties of the specified object.
    /// </summary>
    void Dir(JsValue item, JsValue options);

    /// <summary>
    /// Displays an XML/HTML element representation.
    /// </summary>
    void Dirxml(params JsValue[] data);

    /// <summary>
    /// Outputs an error message.
    /// </summary>
    void Error(params JsValue[] data);

    /// <summary>
    /// Creates a new inline group.
    /// </summary>
    void Group(params JsValue[] data);

    /// <summary>
    /// Creates a new inline group that is initially collapsed.
    /// </summary>
    void GroupCollapsed(params JsValue[] data);

    /// <summary>
    /// Ends the current inline group.
    /// </summary>
    void GroupEnd();

    /// <summary>
    /// Outputs an informational message.
    /// </summary>
    void Info(params JsValue[] data);

    /// <summary>
    /// Outputs a message to the console.
    /// </summary>
    void Log(params JsValue[] data);

    /// <summary>
    /// Displays tabular data as a table.
    /// </summary>
    void Table(JsValue tabularData, string[]? properties = null);

    /// <summary>
    /// Starts a timer with a label.
    /// </summary>
    void Time(string label);

    /// <summary>
    /// Stops a timer with a label.
    /// </summary>
    void TimeEnd(string label);

    /// <summary>
    /// Logs the current value of a timer.
    /// </summary>
    void TimeLog(string label, params JsValue[] data);

    /// <summary>
    /// Adds a timestamp with an optional label.
    /// </summary>
    void TimeStamp(string label);

    /// <summary>
    /// Outputs a stack trace.
    /// </summary>
    void Trace(params JsValue[] data);

    /// <summary>
    /// Outputs a warning message.
    /// </summary>
    void Warn(params JsValue[] data);
}
