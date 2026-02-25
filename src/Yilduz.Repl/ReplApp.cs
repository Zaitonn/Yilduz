using System;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Runtime;
using PrettyPrompt;
using Spectre.Console;

namespace Yilduz.Repl;

internal sealed class ReplApp
{
    private readonly EngineExecutor _executor;
    private readonly ScriptParsingOptions _parsingOptions;
    private readonly Prompt _prompt;
    private readonly string _promptText;

    public ReplApp(EngineExecutor executor, string promptText)
    {
        _executor = executor ?? throw new ArgumentNullException(nameof(executor));
        _parsingOptions = new() { Tolerant = true };
        _promptText = promptText;
        _prompt = new(".history", configuration: new() { Prompt = _promptText });
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var inputResult = await ReadLineAsync(cancellationToken);
            if (inputResult is null)
            {
                continue;
            }

            var (input, submitWithCtrlEnter) = inputResult.Value;

            if (input is "#c")
            {
                AnsiConsole.Clear();
                continue;
            }
            else if (input is "#r")
            {
                await _executor.ResetEngine(cancellationToken);
                AnsiConsole.MarkupLine("[green]Engine reset.[/]");
                continue;
            }
            else if (input is "#q" or "exit")
            {
                break;
            }

            var script = NormalizeInput(input);
            if (string.IsNullOrWhiteSpace(script))
            {
                continue;
            }

            try
            {
                var result = await _executor.EvaluateAsync(
                    script: script,
                    parsingOptions: _parsingOptions,
                    cancellationToken: cancellationToken
                );
                if (submitWithCtrlEnter)
                {
                    OutputRenderer.RenderObjectTable(result);
                }
                else
                {
                    OutputRenderer.RenderValue(result);
                }
            }
            catch (JavaScriptException je)
            {
                OutputRenderer.RenderError(je);
                if (submitWithCtrlEnter)
                {
                    OutputRenderer.RenderObjectTable(je.Error);
                }
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e, ExceptionFormats.ShortenEverything);
            }
        }
    }

    private async Task<(string Text, bool SubmitWithCtrlEnter)?> ReadLineAsync(
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await _prompt.ReadLineAsync();
        cancellationToken.ThrowIfCancellationRequested();
        if (!result.IsSuccess)
        {
            return null;
        }

        var isCtrlEnter =
            result.SubmitKeyInfo.Key == ConsoleKey.Enter
            && result.SubmitKeyInfo.Modifiers.HasFlag(ConsoleModifiers.Control);

        return (result.Text, isCtrlEnter);
    }

    private static string NormalizeInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return string.Empty;
        }

        return input;
    }
}
