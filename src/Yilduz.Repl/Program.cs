using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using Jint;
using Jint.Native.Json;
using Jint.Runtime;
using PrettyPrompt;
using Spectre.Console;
using Yilduz;

var engine = new Engine(cfg => cfg.AllowClr()).InitializeWebApi(new());
var assembly = Assembly.GetExecutingAssembly();
var version = assembly.GetName().Version?.ToString();

Console.CancelKeyPress += (_, e) => e.Cancel = true;
AnsiConsole.MarkupLine($"Welcome to [steelblue1_1]Yilduz.Repl[/] ({version})");
AnsiConsole.WriteLine();

var parsingOptions = new ScriptParsingOptions { Tolerant = true };
var serializer = new JsonSerializer(engine);

await StartLoop();

async Task StartLoop()
{
    var prompt = new Prompt(configuration: new(prompt: "Yilduz> "));
    while (true)
    {
        var promptResult = await prompt.ReadLineAsync();
        var input = promptResult.Text;

        if (!promptResult.IsSuccess)
        {
            continue;
        }
        if (input is "exit" or ".exit")
        {
            return;
        }

        try
        {
            var result = engine.Evaluate(input, parsingOptions);

            switch (result.Type)
            {
                case Types.Null:
                case Types.Undefined:
                case Types.Boolean:
                    AnsiConsole.MarkupLineInterpolated($"[deepskyblue3_1]{result}[/]");
                    break;
                case Types.String:
                    AnsiConsole.MarkupLineInterpolated(
                        $"[darkorange3]{HttpUtility.JavaScriptStringEncode(result.AsString(), true)}[/]"
                    );
                    break;
                case Types.BigInt:
                case Types.Number:
                    AnsiConsole.MarkupLineInterpolated($"[darkseagreen2]{result}[/]");
                    break;
                case Types.Symbol:
                    AnsiConsole.MarkupLineInterpolated($"[lightsteelblue3]{result}[/]");
                    break;

                default:
                    AnsiConsole.WriteLine(result.ToString());
                    break;
            }
        }
        catch (JavaScriptException je)
        {
            AnsiConsole.MarkupLine($"[red]{je.Error.ToString().EscapeMarkup()}[/]");
        }
        catch (Exception e)
        {
            AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
        }
    }
}
