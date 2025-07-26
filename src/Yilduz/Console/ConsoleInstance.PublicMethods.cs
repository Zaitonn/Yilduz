namespace Yilduz.Console;

public sealed partial class ConsoleInstance
{
    /// <inheritdoc/>
    public void Assert(bool condition, params object[] data)
    {
        _console.Assert(condition, data);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _console.Clear();
    }

    /// <inheritdoc/>
    public void Count(string? label = null)
    {
        _console.Count(label);
    }

    /// <inheritdoc/>
    public void CountReset(string? label = null)
    {
        _console.CountReset(label);
    }

    /// <inheritdoc/>
    public void Debug(params object[] data)
    {
        _console.Debug(data);
    }

    /// <inheritdoc/>
    public void Dir(object? item = null, object? options = null)
    {
        _console.Dir(item, options);
    }

    /// <inheritdoc/>
    public void Dirxml(params object[] data)
    {
        _console.Dirxml(data);
    }

    /// <inheritdoc/>
    public void Error(params object[] data)
    {
        _console.Error(data);
    }

    /// <inheritdoc/>
    public void Group(params object[] data)
    {
        _console.Group(data);
    }

    /// <inheritdoc/>
    public void GroupCollapsed(params object[] data)
    {
        _console.GroupCollapsed(data);
    }

    /// <inheritdoc/>
    public void GroupEnd()
    {
        _console.GroupEnd();
    }

    /// <inheritdoc/>
    public void Info(params object[] data)
    {
        _console.Info(data);
    }

    /// <inheritdoc/>
    public void Log(params object[] data)
    {
        _console.Log(data);
    }

    /// <inheritdoc/>
    public void Table(object? tabularData = null, string[]? properties = null)
    {
        _console.Table(tabularData, properties);
    }

    /// <inheritdoc/>
    public void Time(string? label = null)
    {
        _console.Time(label);
    }

    /// <inheritdoc/>
    public void TimeEnd(string? label = null)
    {
        _console.TimeEnd(label);
    }

    /// <inheritdoc/>
    public void TimeLog(string? label = null, params object[] data)
    {
        _console.TimeLog(label, data);
    }

    /// <inheritdoc/>
    public void TimeStamp(string? label = null)
    {
        _console.TimeStamp(label);
    }

    /// <inheritdoc/>
    public void Trace(params object[] data)
    {
        _console.Trace(data);
    }

    /// <inheritdoc/>
    public void Warn(params object[] data)
    {
        _console.Warn(data);
    }
}
