using Jint.Native;

namespace Yilduz.Console;

public sealed partial class ConsoleInstance
{
    /// <inheritdoc/>
    public void Assert(bool condition, params JsValue[] data)
    {
        _console.Value.Assert(condition, data);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _console.Value.Clear();
    }

    /// <inheritdoc/>
    public void Count(string label)
    {
        _console.Value.Count(label);
    }

    /// <inheritdoc/>
    public void CountReset(string label)
    {
        _console.Value.CountReset(label);
    }

    /// <inheritdoc/>
    public void Debug(params JsValue[] data)
    {
        _console.Value.Debug(data);
    }

    /// <inheritdoc/>
    public void Dir(JsValue item, JsValue options)
    {
        _console.Value.Dir(item, options);
    }

    /// <inheritdoc/>
    public void Dirxml(params JsValue[] data)
    {
        _console.Value.Dirxml(data);
    }

    /// <inheritdoc/>
    public void Error(params JsValue[] data)
    {
        _console.Value.Error(data);
    }

    /// <inheritdoc/>
    public void Group(params JsValue[] data)
    {
        _console.Value.Group(data);
    }

    /// <inheritdoc/>
    public void GroupCollapsed(params JsValue[] data)
    {
        _console.Value.GroupCollapsed(data);
    }

    /// <inheritdoc/>
    public void GroupEnd()
    {
        _console.Value.GroupEnd();
    }

    /// <inheritdoc/>
    public void Info(params JsValue[] data)
    {
        _console.Value.Info(data);
    }

    /// <inheritdoc/>
    public void Log(params JsValue[] data)
    {
        _console.Value.Log(data);
    }

    /// <inheritdoc/>
    public void Table(JsValue tabularData, string[]? properties = null)
    {
        _console.Value.Table(tabularData, properties);
    }

    /// <inheritdoc/>
    public void Time(string label)
    {
        _console.Value.Time(label);
    }

    /// <inheritdoc/>
    public void TimeEnd(string label)
    {
        _console.Value.TimeEnd(label);
    }

    /// <inheritdoc/>
    public void TimeLog(string label, params JsValue[] data)
    {
        _console.Value.TimeLog(label, data);
    }

    /// <inheritdoc/>
    public void TimeStamp(string label)
    {
        _console.Value.TimeStamp(label);
    }

    /// <inheritdoc/>
    public void Trace(params JsValue[] data)
    {
        _console.Value.Trace(data);
    }

    /// <inheritdoc/>
    public void Warn(params JsValue[] data)
    {
        _console.Value.Warn(data);
    }
}
