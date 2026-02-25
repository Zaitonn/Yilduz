using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Runtime;
using Spectre.Console;

namespace Yilduz.Repl;

internal static class OutputRenderer
{
    public static void RenderValue(JsValue value)
    {
        AnsiConsole.MarkupLine(RenderMarkup(value));
    }

    public static void RenderObjectTable(JsValue value)
    {
        if (value is not ObjectInstance objectInstance)
        {
            RenderValue(value);
            return;
        }

        if (objectInstance.IsArray())
        {
            RenderArrayTable(objectInstance.AsArray());
            return;
        }

        var table = new Table().AddColumns("Key", "Value").RoundedBorder();

        foreach (var (key, propValue, depth, error) in EnumeratePropertyEntries(objectInstance))
        {
            try
            {
                var valueCell = error is null ? RenderMarkup(propValue) : RenderException(error);
                table.AddRow(ColorizeKey(key, depth), valueCell);
            }
            catch (Exception ex)
            {
                table.AddRow(ColorizeKey(key, depth), RenderException(ex));
            }
        }

        AnsiConsole.Write(table);
    }

    public static string RenderMarkup(JsValue value)
    {
        switch (value.Type)
        {
            case Types.Empty:
                return string.Empty;

            case Types.Null:
            case Types.Undefined:
            case Types.Boolean:
                return $"[deepskyblue3_1]{value}[/]";

            case Types.String:
                return $"[LightSalmon3_1]\"{value.AsString().Replace("\"", "\\\"").EscapeMarkup()}\"[/]";

            case Types.BigInt:
            case Types.Number:
                return $"[darkseagreen2]{value}[/]";

            case Types.Symbol:
                return $"[DarkSeaGreen3_1]{value}[/]";

            case Types.Object:
            default:
                return RenderObject(value);
        }
    }

    private static string RenderObject(JsValue value)
    {
        switch (value)
        {
            case Constructor:
                return $"[LightPink3]{value.ToString().EscapeMarkup()}[/]";

            case Function:
                return $"[MediumPurple2_1]{value.ToString().EscapeMarkup()}[/]";

            default:
                return $"[DarkSlateGray3]{value.ToString().EscapeMarkup()}[/]";
        }
    }

    private static void RenderArrayTable(JsArray array)
    {
        var table = new Table()
            .AddColumn("Index", column => column.Alignment = Justify.Center)
            .AddColumn("Value")
            .RoundedBorder();

        for (var i = 0; i < array.Length; i++)
        {
            var index = $"[[[darkseagreen2]{i}[/]]]";
            try
            {
                var element = array[i];
                table.AddRow(index, RenderMarkup(element));
            }
            catch (Exception ex)
            {
                table.AddRow(index, RenderException(ex));
            }
        }

        AnsiConsole.Write(table);
    }

    public static void RenderError(JavaScriptException javaScriptException)
    {
        AnsiConsole.Write(
            new Panel(
                javaScriptException.Error.ToString().EscapeMarkup()
                    + Environment.NewLine
                    + $"[italic Gray54]{javaScriptException.JavaScriptStackTrace.EscapeMarkup()}[/]"
            )
                .BorderColor(Color.Red)
                .RoundedBorder()
        );
    }

    private static string RenderException(Exception ex)
    {
        var error =
            ex is JavaScriptException jsEx ? jsEx.Error
            : ex.InnerException is JavaScriptException innerJsEx ? innerJsEx.Error
            : null;

        if (error is null)
        {
            return $"[red italic][[!]]{(ex.GetType() + ex.Message).EscapeMarkup()}[/]";
        }
        return $"[red italic][[!]]{error.ToString().EscapeMarkup()}[/]";
    }

    private static IEnumerable<(
        JsValue Key,
        JsValue Value,
        int Depth,
        Exception? Error
    )> EnumeratePropertyEntries(ObjectInstance instance)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var current = instance;
        var depth = 0;

        while (current is not null)
        {
            foreach (var key in current.GetOwnPropertyKeys())
            {
                var normalized = key.ToString();
                if (seen.Add(normalized))
                {
                    Exception? exception = null;
                    var value = JsValue.Undefined;
                    try
                    {
                        value = instance.Get(key);
                    }
                    catch (Exception ex)
                    {
                        exception = ex;
                    }

                    yield return (key, value, depth, exception);
                }
            }

            current = current.Prototype;
            depth++;
        }
    }

    private static string ColorizeKey(JsValue key, int depth)
    {
        // depth: 0 = own, 1 = prototype, 2 = prototype's prototype, ...
        var text = key.ToString().EscapeMarkup();
        var isSymbol = key.IsSymbol();

        if (depth < 2)
        {
            return isSymbol ? $"[DarkSeaGreen3_1]{text}[/]" : text;
        }

        ReadOnlySpan<string> colors = ["gray50", "gray42", "gray35", "gray27"];
        var index = Math.Min(depth - 2, colors.Length - 1);
        var color = colors[index];
        return $"[{color}]{text}[/]";
    }
}
