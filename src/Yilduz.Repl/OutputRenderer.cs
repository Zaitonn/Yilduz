using System;
using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Runtime;
using Spectre.Console;
using Spectre.Console.Rendering;

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
                var valueCell = error is null
                    ? RenderMarkup(propValue)
                    : RenderExceptionInline(error);
                table.AddRow(ColorizeKey(key, depth), valueCell);
            }
            catch (Exception ex)
            {
                table.AddRow(ColorizeKey(key, depth), RenderExceptionInline(ex));
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
                return $"[#d69d85]\"{value.AsString().Replace("\"", "\\\"").EscapeMarkup()}\"[/]";

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
        var str = value.ToString().EscapeMarkup();
        switch (value)
        {
            case Constructor:
                return $"[#4EC9B0]{str}[/]";

            case Function:
                return $"[MediumPurple2_1]{str}[/]";

            case JsArray:
                return $"[Tan]{str}[/]";

            case JsDate:
                return $"[LightSeaGreen]{str}[/]";

            case JsRegExp:
                return $"[IndianRed_1]{str}[/]";

            case JsError:
                return $"[red]{str}[/]";

            case ObjectInstance when value.IsPromise():
                return $"[LightSlateBlue]{str}[/]";

            default:
                return $"[DarkSlateGray3]{str}[/]";
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
                table.AddRow(index, RenderExceptionInline(ex));
            }
        }

        AnsiConsole.Write(table);
    }

    public static void RenderError(JavaScriptException javaScriptException, string? title = null)
    {
        List<IRenderable> items =
        [
            new Markup(javaScriptException.Error.ToString().EscapeMarkup()),
            new Markup(
                $"[italic Gray54]{javaScriptException.JavaScriptStackTrace.EscapeMarkup()}[/]"
            ),
        ];

        if (!string.IsNullOrEmpty(title))
        {
            items.Insert(0, Text.NewLine);
            items.Insert(0, new Markup($"[red bold]{title}[/]"));
        }

        AnsiConsole.Write(new Panel(new Rows(items)).BorderColor(Color.Red).RoundedBorder());
    }

    private static readonly ExceptionSettings ExceptionSettings = new()
    {
        Format = ExceptionFormats.ShortenTypes | ExceptionFormats.ShortenPaths,
        Style =
        {
            Exception = new Style(Color.Red, Color.Default, Decoration.Bold),
            Message = Color.Red,
            Path = new Style(Color.Grey, Color.Default, Decoration.Italic),
            LineNumber = new Style(Color.Grey, Color.Default, Decoration.Italic),
            Dimmed = Color.Grey,
            NonEmphasized = Color.FromHex("#DCDCDC"),
            Method = Color.FromHex("#DCDCAA"),
            ParameterType = Color.FromHex("#4EC9B0"),
            ParameterName = Color.FromHex("#9CDCFE"),
        },
    };

    public static void RenderException(Exception ex)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.WriteException(ex, ExceptionSettings);
    }

    private static string RenderExceptionInline(Exception ex)
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
