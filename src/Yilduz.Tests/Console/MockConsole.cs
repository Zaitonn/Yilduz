using System.Collections.Generic;
using Yilduz.Console;

namespace Yilduz.Tests.Console;

public sealed class MockConsole : IConsole
{
    public List<string> LogCalls = [];

    public void Assert(bool condition, params object[] data)
    {
        LogCalls.Add($"Assert: {condition}, data: {string.Join(", ", data)}");
    }

    public void Clear()
    {
        LogCalls.Add("Clear");
    }

    public void Count(string? label = null)
    {
        LogCalls.Add($"Count: {label ?? "default"}");
    }

    public void CountReset(string? label = null)
    {
        LogCalls.Add($"CountReset: {label ?? "default"}");
    }

    public void Debug(params object[] data)
    {
        LogCalls.Add($"Debug: {string.Join(", ", data)}");
    }

    public void Dir(object? item = null, object? options = null)
    {
        LogCalls.Add($"Dir: {item}, options: {options}");
    }

    public void Dirxml(params object[] data)
    {
        LogCalls.Add($"DirXml: {string.Join(", ", data)}");
    }

    public void Error(params object[] data)
    {
        LogCalls.Add($"Error: {string.Join(", ", data)}");
    }

    public void Group(params object[] data)
    {
        LogCalls.Add($"Group: {string.Join(", ", data)}");
    }

    public void GroupCollapsed(params object[] data)
    {
        LogCalls.Add($"GroupCollapsed: {string.Join(", ", data)}");
    }

    public void GroupEnd()
    {
        LogCalls.Add("GroupEnd");
    }

    public void Info(params object[] data)
    {
        LogCalls.Add($"Info: {string.Join(", ", data)}");
    }

    public void Log(params object[] data)
    {
        LogCalls.Add($"Log: {string.Join(", ", data)}");
    }

    public void Table(object? tabularData = null, string[]? properties = null)
    {
        var props = properties != null ? string.Join(", ", properties) : "null";
        LogCalls.Add($"Table: {tabularData}, properties: {props}");
    }

    public void Time(string? label = null)
    {
        LogCalls.Add($"Time: {label ?? "default"}");
    }

    public void TimeEnd(string? label = null)
    {
        LogCalls.Add($"TimeEnd: {label ?? "default"}");
    }

    public void TimeLog(string? label = null, params object[] data)
    {
        LogCalls.Add($"TimeLog: {label ?? "default"}, data: {string.Join(", ", data)}");
    }

    public void TimeStamp(string? label = null)
    {
        LogCalls.Add($"TimeStamp: {label ?? "default"}");
    }

    public void Trace(params object[] data)
    {
        LogCalls.Add($"Trace: {string.Join(", ", data)}");
    }

    public void Warn(params object[] data)
    {
        LogCalls.Add($"Warn: {string.Join(", ", data)}");
    }
}
