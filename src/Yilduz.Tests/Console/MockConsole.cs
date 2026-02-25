using System.Collections.Generic;
using Jint.Native;
using Yilduz.Console;

namespace Yilduz.Tests.Console;

public sealed class MockConsole : IConsole
{
    public List<string> LogCalls = [];

    public void Assert(bool condition, params JsValue[] data)
    {
        LogCalls.Add($"Assert: {condition}, data: {string.Join<JsValue>(", ", data)}");
    }

    public void Clear()
    {
        LogCalls.Add("Clear");
    }

    public void Count(string label)
    {
        LogCalls.Add($"Count: {label}");
    }

    public void CountReset(string label)
    {
        LogCalls.Add($"CountReset: {label}");
    }

    public void Debug(params JsValue[] data)
    {
        LogCalls.Add($"Debug: {string.Join<JsValue>(", ", data)}");
    }

    public void Dir(JsValue item, JsValue options)
    {
        LogCalls.Add($"Dir: {item}, options: {options}");
    }

    public void Dirxml(params JsValue[] data)
    {
        LogCalls.Add($"DirXml: {string.Join<JsValue>(", ", data)}");
    }

    public void Error(params JsValue[] data)
    {
        LogCalls.Add($"Error: {string.Join<JsValue>(", ", data)}");
    }

    public void Group(params JsValue[] data)
    {
        LogCalls.Add($"Group: {string.Join<JsValue>(", ", data)}");
    }

    public void GroupCollapsed(params JsValue[] data)
    {
        LogCalls.Add($"GroupCollapsed: {string.Join<JsValue>(", ", data)}");
    }

    public void GroupEnd()
    {
        LogCalls.Add("GroupEnd");
    }

    public void Info(params JsValue[] data)
    {
        LogCalls.Add($"Info: {string.Join<JsValue>(", ", data)}");
    }

    public void Log(params JsValue[] data)
    {
        LogCalls.Add($"Log: {string.Join<JsValue>(", ", data)}");
    }

    public void Table(JsValue tabularData, string[]? properties = null)
    {
        var props = properties != null ? string.Join(", ", properties) : "null";
        LogCalls.Add($"Table: {tabularData}, properties: {props}");
    }

    public void Time(string label)
    {
        LogCalls.Add($"Time: {label}");
    }

    public void TimeEnd(string label)
    {
        LogCalls.Add($"TimeEnd: {label}");
    }

    public void TimeLog(string label, params JsValue[] data)
    {
        LogCalls.Add($"TimeLog: {label}, data: {string.Join<JsValue>(", ", data)}");
    }

    public void TimeStamp(string label)
    {
        LogCalls.Add($"TimeStamp: {label}");
    }

    public void Trace(params JsValue[] data)
    {
        LogCalls.Add($"Trace: {string.Join<JsValue>(", ", data)}");
    }

    public void Warn(params JsValue[] data)
    {
        LogCalls.Add($"Warn: {string.Join<JsValue>(", ", data)}");
    }
}
