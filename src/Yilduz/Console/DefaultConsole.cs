using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;
using SysConsole = System.Console;

namespace Yilduz.Console;

internal sealed class DefaultConsole(Engine engine) : IConsole
{
    private readonly Dictionary<string, int> _counters = [];
    private readonly Dictionary<string, DateTime> _timers = [];
    private readonly Engine _engine = engine;

    private static string FormatMessage(params JsValue[] data)
    {
        return string.Join<JsValue>(" ", data);
    }

    private JsValue ToJs(object value) => JsValue.FromObject(_engine, value);

    public void Assert(bool condition, params JsValue[] data)
    {
        if (condition)
        {
            return;
        }

        var message = FormatMessage(data);

        if (string.IsNullOrEmpty(message))
        {
            message = "console.assert";
        }

        SysConsole.ForegroundColor = ConsoleColor.Red;
        SysConsole.WriteLine($"[Assert] Assert failed {message}");
        SysConsole.ResetColor();
    }

    public void Clear()
    {
        SysConsole.Clear();
    }

    public void Count(string label)
    {
        int count = 0;
        lock (_counters)
        {
            _counters.TryGetValue(label, out count);
            count++;
            _counters[label] = count;
        }

        SysConsole.WriteLine($"[Count] {label}: {count}");
    }

    public void CountReset(string label)
    {
        lock (_counters)
        {
            _counters.Remove(label);
        }
    }

    public void Debug(params JsValue[] data)
    {
        SysConsole.ForegroundColor = ConsoleColor.DarkGray;
        SysConsole.WriteLine($"[Debug] {FormatMessage(data)}");
        SysConsole.ResetColor();
    }

    public void Dir(JsValue item, JsValue options) { }

    public void Dirxml(params JsValue[] data) { }

    public void Error(params JsValue[] data)
    {
        SysConsole.ForegroundColor = ConsoleColor.Red;
        SysConsole.WriteLine($"[Error] {FormatMessage(data)}");
        SysConsole.ResetColor();
    }

    public void Group(params JsValue[] data) { }

    public void GroupCollapsed(params JsValue[] data) { }

    public void GroupEnd() { }

    public void Info(params JsValue[] data)
    {
        SysConsole.ForegroundColor = ConsoleColor.Cyan;
        SysConsole.WriteLine($"[Info] {FormatMessage(data)}");
        SysConsole.ResetColor();
    }

    public void Log(params JsValue[] data)
    {
        Info(data);
    }

    public void Table(JsValue tabularData, string[]? properties = null) { }

    public void Time(string label)
    {
        lock (_timers)
        {
            if (_timers.ContainsKey(label))
            {
                Warn(ToJs($"Timer '{label}' already exists"));
                return;
            }
            _timers[label] = DateTime.Now;
        }
    }

    public void TimeEnd(string label)
    {
        lock (_timers)
        {
            if (_timers.TryGetValue(label, out DateTime startTime))
            {
                Info(ToJs($"{label}: {(DateTime.Now - startTime).TotalMilliseconds} ms"));
                _timers.Remove(label);
            }
            else
            {
                Warn(ToJs($"Timer '{label}' does not exist"));
            }
        }
    }

    public void TimeLog(string label, params JsValue[] data)
    {
        lock (_timers)
        {
            if (_timers.TryGetValue(label, out DateTime startTime))
            {
                Info(
                    ToJs(
                        $"{label}: {(DateTime.Now - startTime).TotalMilliseconds} ms "
                            + FormatMessage(data)
                    )
                );
            }
            else
            {
                Warn(ToJs($"Timer '{label}' does not exist"));
            }
        }
    }

    public void TimeStamp(string label) { }

    public void Trace(params JsValue[] data)
    {
        SysConsole.ForegroundColor = ConsoleColor.Gray;
        SysConsole.WriteLine($"[Trace] {FormatMessage(data)}");
        SysConsole.WriteLine(_engine.Advanced.StackTrace);
        SysConsole.ResetColor();
    }

    public void Warn(params JsValue[] data)
    {
        SysConsole.ForegroundColor = ConsoleColor.Yellow;
        SysConsole.WriteLine($"[Warn] {FormatMessage(data)}");
        SysConsole.ResetColor();
    }
}
