using Jint.Native;

namespace Yilduz.Console;

public sealed partial class ConsoleInstance
{
    /// <inheritdoc/>
    public void Assert(bool condition, params JsValue[] data)
    {
        _console.Assert(condition, data);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _console.Clear();
    }

    /// <inheritdoc/>
    public void Count(string label)
    {
        _console.Count(label);
    }

    /// <inheritdoc/>
    public void CountReset(string label)
    {
        _console.CountReset(label);
    }

    /// <inheritdoc/>
    public void Debug(params JsValue[] data)
    {
        _console.Debug(data);
    }

    /// <inheritdoc/>
    public void Dir(JsValue item, JsValue options)
    {
        _console.Dir(item, options);
    }

    /// <inheritdoc/>
    public void Dirxml(params JsValue[] data)
    {
        _console.Dirxml(data);
    }

    /// <inheritdoc/>
    public void Error(params JsValue[] data)
    {
        _console.Error(data);
    }

    /// <inheritdoc/>
    public void Group(params JsValue[] data)
    {
        _console.Group(data);
    }

    /// <inheritdoc/>
    public void GroupCollapsed(params JsValue[] data)
    {
        _console.GroupCollapsed(data);
    }

    /// <inheritdoc/>
    public void GroupEnd()
    {
        _console.GroupEnd();
    }

    /// <inheritdoc/>
    public void Info(params JsValue[] data)
    {
        _console.Info(data);
    }

    /// <inheritdoc/>
    public void Log(params JsValue[] data)
    {
        _console.Log(data);
    }

    /// <inheritdoc/>
    public void Table(JsValue tabularData, string[]? properties = null)
    {
        _console.Table(tabularData, properties);
    }

    /// <inheritdoc/>
    public void Time(string label)
    {
        _console.Time(label);
    }

    /// <inheritdoc/>
    public void TimeEnd(string label)
    {
        _console.TimeEnd(label);
    }

    /// <inheritdoc/>
    public void TimeLog(string label, params JsValue[] data)
    {
        _console.TimeLog(label, data);
    }

    /// <inheritdoc/>
    public void TimeStamp(string label)
    {
        _console.TimeStamp(label);
    }

    /// <inheritdoc/>
    public void Trace(params JsValue[] data)
    {
        _console.Trace(data);
    }

    /// <inheritdoc/>
    public void Warn(params JsValue[] data)
    {
        _console.Warn(data);
    }
}
