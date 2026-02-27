using System;
using System.Collections.Generic;
using System.Linq;
using Jint;
using Jint.Native;
using Spectre.Console;
using Yilduz.Console;

namespace Yilduz.Repl;

internal sealed class ReplConsole : IConsole
{
    private readonly Engine _engine;
    private readonly Dictionary<string, int> _counters = [];
    private readonly Dictionary<string, DateTime> _timers = [];

    public ReplConsole(Engine engine)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
    }

    public void Assert(bool condition, params JsValue[] data)
    {
        if (condition)
        {
            return;
        }

        WriteLine("ASSERT", "red", Prepend(condition, data));
    }

    public void Clear()
    {
        AnsiConsole.Clear();
    }

    public void Count(string label)
    {
        int count;
        lock (_counters)
        {
            _counters.TryGetValue(label, out count);
            count++;
            _counters[label] = count;
        }

        WriteLine(
            "COUNT",
            "yellow3",
            JsValue.FromObject(_engine, label),
            JsValue.FromObject(_engine, count)
        );
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
        WriteLine("DEBUG", "grey50", data);
    }

    public void Dir(JsValue item, JsValue options)
    {
        WriteLine("DIR", "lightsteelblue3", item, options);
    }

    public void Dirxml(params JsValue[] data)
    {
        WriteLine("DIROBJ", "lightsteelblue3", data);
    }

    public void Error(params JsValue[] data)
    {
        WriteLine("ERROR", "red3", data);
    }

    public void Group(params JsValue[] data)
    {
        WriteLine("GROUP", "mediumturquoise", data);
    }

    public void GroupCollapsed(params JsValue[] data)
    {
        WriteLine("GROUP", "mediumturquoise", data);
    }

    public void GroupEnd()
    {
        WriteLine("GROUP", "mediumturquoise");
    }

    public void Info(params JsValue[] data)
    {
        WriteLine("INFO", "deepskyblue1", data);
    }

    public void Log(params JsValue[] data)
    {
        WriteLine("INFO", "deepskyblue1", data);
    }

    public void Table(JsValue tabularData, string[]? properties = null)
    {
        WriteLine(
            "TABLE",
            "palegreen3",
            tabularData,
            JsValue.FromObject(_engine, properties ?? Array.Empty<string>())
        );
    }

    public void Time(string label)
    {
        lock (_timers)
        {
            if (_timers.ContainsKey(label))
            {
                WriteLine("WARN", "yellow3", $"Timer '{label}' already exists");
                return;
            }

            _timers[label] = DateTime.UtcNow;
        }

        WriteLine("TIME", "DodgerBlue1", JsValue.FromObject(_engine, $"Started '{label}'"));
    }

    public void TimeEnd(string label)
    {
        double? elapsedMs = null;

        lock (_timers)
        {
            if (_timers.TryGetValue(label, out var start))
            {
                elapsedMs = (DateTime.UtcNow - start).TotalMilliseconds;
                _timers.Remove(label);
            }
        }

        if (elapsedMs is double value)
        {
            WriteLine(
                "TIME",
                "DodgerBlue1",
                JsValue.FromObject(_engine, label),
                JsValue.FromObject(_engine, $"{value:0.###} ms")
            );
            return;
        }

        WriteLine(
            "WARN",
            "yellow3",
            JsValue.FromObject(_engine, $"Timer '{label}' does not exist")
        );
    }

    public void TimeLog(string label, params JsValue[] data)
    {
        if (!_timers.TryGetValue(label, out var start))
        {
            WriteLine(
                "WARN",
                "yellow3",
                JsValue.FromObject(_engine, $"Timer '{label}' does not exist")
            );
            return;
        }

        var elapsedMs = (DateTime.UtcNow - start).TotalMilliseconds;
        WriteLine(
            "TIME",
            "DodgerBlue1",
            Prepend(JsValue.FromObject(_engine, $"{label}: {elapsedMs:0.###} ms"), data)
        );
    }

    public void TimeStamp(string label)
    {
        var stamp = DateTime.Now.ToString("O");
        WriteLine(
            "TIME",
            "DodgerBlue1",
            JsValue.FromObject(_engine, label),
            JsValue.FromObject(_engine, stamp)
        );
    }

    public void Trace(params JsValue[] data)
    {
        WriteLine("TRACE", "slateblue3", data);
        AnsiConsole.WriteLine(_engine.Advanced.StackTrace);
    }

    public void Warn(params JsValue[] data)
    {
        WriteLine("WARN", "Gold1", data);
    }

    private void WriteLine(string label, string labelColor, params JsValue[] data)
    {
        var labelMarkup = $"[{labelColor}]{label.EscapeMarkup()}[/]";
        if (data is null || data.Length == 0)
        {
            AnsiConsole.MarkupLine(labelMarkup);
            return;
        }

        var rendered = string.Join(" ", data.Select(OutputRenderer.RenderMarkup));

        AnsiConsole.Write(
            new Table()
                .AddColumn("1")
                .AddColumn("1")
                .AddRow(labelMarkup, rendered)
                .NoBorder()
                .NoSafeBorder()
                .HideRowSeparators()
                .HideFooters()
                .HideHeaders()
        );
    }

    private static JsValue[] Prepend(JsValue value, JsValue[] data)
    {
        var result = new JsValue[data.Length + 1];
        result[0] = value;
        Array.Copy(data, 0, result, 1, data.Length);
        return result;
    }
}
