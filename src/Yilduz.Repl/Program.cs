using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Spectre.Console;

namespace Yilduz.Repl;

internal static class Program
{
    private const string PromptText = "Yilduz> ";

    public static async Task Main(string[] args)
    {
        using var cts = new CancellationTokenSource();

        System.Console.Clear();
        System.Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            cts.Cancel();
        };

        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        var watchPath = TryGetWatchPath(args);

        PrintWelcome(version);
        PrintHelp();

        var executor = new EngineExecutor(cts.Token);

        var watcherTask = Task.CompletedTask;
        if (!string.IsNullOrWhiteSpace(watchPath))
        {
            var watcher = new ScriptWatcher(watchPath, executor);
            watcher.PrintWatching();

            try
            {
                await watcher.ExecuteInitialAsync(cts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            watcherTask = watcher.RunAsync(cts.Token, skipInitial: true, printWatching: false);
        }

        var repl = new ReplApp(executor, PromptText);
        try
        {
            await repl.RunAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Swallow cancellation on exit.
        }

        cts.Cancel();
        try
        {
            await watcherTask;
        }
        catch (OperationCanceledException)
        {
            // Swallow cancellation on exit.
        }
    }

    private static void PrintWelcome(string? version)
    {
        AnsiConsole.MarkupLine($"Welcome to [Khaki1]Yilduz.Repl[/] ({version})");
        AnsiConsole.WriteLine();
    }

    private static void PrintHelp()
    {
        AnsiConsole.MarkupLine("Commands:");
        AnsiConsole.MarkupLine("  [NavajoWhite1]Enter[/] : evaluate");
        AnsiConsole.MarkupLine(
            "  [NavajoWhite1]Ctrl[/]+[NavajoWhite1]Enter[/] : evaluate (detailed) — expand objects, enumerate lists"
        );
        AnsiConsole.MarkupLine("  [RosyBrown]#q[/] : quit");
        AnsiConsole.MarkupLine("  [RosyBrown]#c[/] : clear screen");
        AnsiConsole.MarkupLine("  [RosyBrown]#r[/] : reset engine");
        AnsiConsole.MarkupLine("  [Cornsilk1]-w <file>[/] : watch a file for changes at startup");
        AnsiConsole.MarkupLine("Repository: [link]https://github.com/Zaitonn/Yilduz[/]");
        AnsiConsole.WriteLine();
    }

    private static string? TryGetWatchPath(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] is "--watch" or "-w")
            {
                return i + 1 < args.Length ? args[i + 1].Trim('"') : null;
            }
        }

        return null;
    }
}
