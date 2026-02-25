using System;
using System.IO;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Jint.Runtime;
using Spectre.Console;

namespace Yilduz.Repl;

internal sealed class ScriptWatcher
{
    private readonly string _fullPath;
    private readonly string _directory;
    private readonly string _fileName;
    private readonly EngineExecutor _executor;

    public ScriptWatcher(string path, EngineExecutor executor)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("Path cannot be empty.", nameof(path));
        }

        _fullPath = Path.GetFullPath(path);
        _directory = Path.GetDirectoryName(_fullPath) ?? Environment.CurrentDirectory;
        _fileName = Path.GetFileName(_fullPath);
        _executor = executor;
    }

    public void PrintWatching()
    {
        AnsiConsole.MarkupLine($"[steelblue1_1]Watching[/] {_fullPath}");
    }

    public Task ExecuteInitialAsync(CancellationToken cancellationToken)
    {
        return ExecuteAsync(cancellationToken, suppressChangeMessage: true);
    }

    public async Task RunAsync(
        CancellationToken cancellationToken,
        bool skipInitial = false,
        bool printWatching = true
    )
    {
        if (printWatching)
        {
            PrintWatching();
        }
        if (!skipInitial)
        {
            await ExecuteAsync(cancellationToken);
        }

        using var watcher = new FileSystemWatcher(_directory, _fileName)
        {
            IncludeSubdirectories = false,
            NotifyFilter =
                NotifyFilters.LastWrite
                | NotifyFilters.Size
                | NotifyFilters.FileName
                | NotifyFilters.CreationTime,
            EnableRaisingEvents = true,
        };

        var channel = Channel.CreateUnbounded<bool>();
        watcher.Changed += (_, __) => channel.Writer.TryWrite(true);
        watcher.Created += (_, __) => channel.Writer.TryWrite(true);
        watcher.Renamed += (_, __) => channel.Writer.TryWrite(true);

        try
        {
            while (
                !cancellationToken.IsCancellationRequested
                && await channel.Reader.WaitToReadAsync(cancellationToken)
            )
            {
                while (channel.Reader.TryRead(out _)) { }

                try
                {
                    await Task.Delay(100, cancellationToken);
                    await ExecuteAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown; no-op.
        }
    }

    private async Task ExecuteAsync(
        CancellationToken cancellationToken,
        bool suppressChangeMessage = false
    )
    {
        if (!File.Exists(_fullPath))
        {
            AnsiConsole.MarkupLine($"[red]File not found:[/] {_fullPath}");
            return;
        }

        var script = await File.ReadAllTextAsync(_fullPath, cancellationToken);
        if (!suppressChangeMessage)
        {
            AnsiConsole.Clear();
            AnsiConsole.MarkupLineInterpolated($"[palegreen3]{_fileName} changed.[/]");
        }

        await _executor.ResetEngine(cancellationToken);

        try
        {
            await _executor.ExecuteAsync(script, cancellationToken);
            AnsiConsole.MarkupLine("[palegreen3 italic]Execution completed.[/]");
        }
        catch (JavaScriptException je)
        {
            OutputRenderer.RenderError(je);
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
        }
    }
}
