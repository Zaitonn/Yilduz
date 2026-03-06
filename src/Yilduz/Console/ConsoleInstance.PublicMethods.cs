using Jint.Native;

namespace Yilduz.Console;

public sealed partial class ConsoleInstance
{
    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void Assert(bool condition, params JsValue[] data)
    {
        _console.Value.Assert(condition, data);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void Clear()
    {
        _console.Value.Clear();
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void Count(string label)
    {
        _console.Value.Count(label);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void CountReset(string label)
    {
        _console.Value.CountReset(label);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void Debug(params JsValue[] data)
    {
        _console.Value.Debug(data);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void Dir(JsValue item, JsValue options)
    {
        _console.Value.Dir(item, options);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void Dirxml(params JsValue[] data)
    {
        _console.Value.Dirxml(data);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void Error(params JsValue[] data)
    {
        _console.Value.Error(data);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void Group(params JsValue[] data)
    {
        _console.Value.Group(data);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void GroupCollapsed(params JsValue[] data)
    {
        _console.Value.GroupCollapsed(data);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void GroupEnd()
    {
        _console.Value.GroupEnd();
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void Info(params JsValue[] data)
    {
        _console.Value.Info(data);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void Log(params JsValue[] data)
    {
        _console.Value.Log(data);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void Table(JsValue tabularData, string[]? properties = null)
    {
        _console.Value.Table(tabularData, properties);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void Time(string label)
    {
        _console.Value.Time(label);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void TimeEnd(string label)
    {
        _console.Value.TimeEnd(label);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void TimeLog(string label, params JsValue[] data)
    {
        _console.Value.TimeLog(label, data);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void TimeStamp(string label)
    {
        _console.Value.TimeStamp(label);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void Trace(params JsValue[] data)
    {
        _console.Value.Trace(data);
    }

    ///<summary>
    /// <inheritdoc/>
    ///</summary>
    public void Warn(params JsValue[] data)
    {
        _console.Value.Warn(data);
    }
}
