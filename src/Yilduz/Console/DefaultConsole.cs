using System;
using System.Collections.Generic;
using Jint;
using SysConsole = System.Console;

namespace Yilduz.Console;

internal sealed class DefaultConsole(Engine engine) : IConsole
{
    private readonly Dictionary<string, int> _counters = [];
    private readonly Dictionary<string, DateTime> _timers = [];
    private readonly Engine _engine = engine;

    private static string FormatMessage(params object[] data)
    {
        return string.Join(" ", data);
    }

    public void Assert(bool condition, params object[] data)
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

    public void Count(string? label = null)
    {
        label ??= "default";
        int count = 0;
        lock (_counters)
        {
            _counters.TryGetValue(label, out count);
            count++;
            _counters[label] = count;
        }

        SysConsole.WriteLine($"[Count] {label}: {count}");
    }

    public void CountReset(string? label = null)
    {
        label ??= "default";
        lock (_counters)
        {
            _counters.Remove(label);
        }
    }

    public void Debug(params object[] data)
    {
        SysConsole.ForegroundColor = ConsoleColor.DarkGray;
        SysConsole.WriteLine($"[Debug] {FormatMessage(data)}");
        SysConsole.ResetColor();
    }

    public void Dir(object? item = null, object? options = null) { }

    public void Dirxml(params object[] data) { }

    public void Error(params object[] data)
    {
        SysConsole.ForegroundColor = ConsoleColor.Red;
        SysConsole.WriteLine($"[Error] {FormatMessage(data)}");
        SysConsole.ResetColor();
    }

    public void Group(params object[] data) { }

    public void GroupCollapsed(params object[] data) { }

    public void GroupEnd() { }

    public void Info(params object[] data)
    {
        SysConsole.ForegroundColor = ConsoleColor.Cyan;
        SysConsole.WriteLine($"[Info] {FormatMessage(data)}");
        SysConsole.ResetColor();
    }

    public void Log(params object[] data)
    {
        Info(data);
    }

    public void Table(object? tabularData = null, string[]? properties = null) { }

    public void Time(string? label = null)
    {
        label ??= "default";
        lock (_timers)
        {
            if (_timers.ContainsKey(label))
            {
                Warn($"Timer '{label}' already exists");
                return;
            }
            _timers[label] = DateTime.Now;
        }
    }

    public void TimeEnd(string? label = null)
    {
        label ??= "default";

        lock (_timers)
        {
            if (_timers.TryGetValue(label, out DateTime startTime))
            {
                Info($"{label}: {(DateTime.Now - startTime).TotalMilliseconds} ms");
                _timers.Remove(label);
            }
            else
            {
                Warn($"Timer '{label}' does not exist");
            }
        }
    }

    public void TimeLog(string? label = null, params object[] data)
    {
        label ??= "default";

        lock (_timers)
        {
            if (_timers.TryGetValue(label, out DateTime startTime))
            {
                Info(
                    $"{label}: {(DateTime.Now - startTime).TotalMilliseconds} ms "
                        + FormatMessage(data)
                );
            }
            else
            {
                Warn($"Timer '{label}' does not exist");
            }
        }
    }

    public void TimeStamp(string? label = null) { }

    public void Trace(params object[] data)
    {
        SysConsole.ForegroundColor = ConsoleColor.Gray;
        SysConsole.WriteLine($"[Trace] {FormatMessage(data)}");
        SysConsole.WriteLine(_engine.Advanced.StackTrace);
        SysConsole.ResetColor();
    }

    public void Warn(params object[] data)
    {
        SysConsole.ForegroundColor = ConsoleColor.Yellow;
        SysConsole.WriteLine($"[Warn] {FormatMessage(data)}");
        SysConsole.ResetColor();
    }
}
