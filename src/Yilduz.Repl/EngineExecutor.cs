using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Spectre.Console;

namespace Yilduz.Repl;

internal sealed class EngineExecutor
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly CancellationToken _cancellationToken;
    private CancellationTokenSource? _cancellationTokenSource;

    public EngineExecutor(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        Engine = CreateNew();
    }

    public Engine Engine { get; private set; }

    private Engine CreateNew()
    {
        if (_cancellationTokenSource is not null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            _cancellationToken
        );

        return new Engine(cfg =>
            cfg.AllowClr().CancellationToken(_cancellationTokenSource.Token)
        ).InitializeWebApi(
            new()
            {
                CancellationToken = _cancellationTokenSource.Token,
                ConsoleFactory = engine => new ReplConsole(engine),
                UnhandledExceptionHandler = ex =>
                {
                    AnsiConsole.WriteLine();
                    if (ex is JavaScriptException jsEx)
                    {
                        OutputRenderer.RenderError(jsEx, "Unhandled JavaScript Exception:");
                    }
                    else
                    {
                        AnsiConsole.WriteException(ex);
                    }
                },
            }
        );
    }

    public async Task ResetEngine(CancellationToken cancellationToken)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            Engine.Dispose();
            Engine = CreateNew();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<JsValue> EvaluateAsync(
        string script,
        ScriptParsingOptions? parsingOptions = null,
        CancellationToken cancellationToken = default
    )
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            return parsingOptions is not null
                ? Engine.Evaluate(script, parsingOptions)
                : Engine.Evaluate(script);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ExecuteAsync(
        string script,
        string source,
        CancellationToken cancellationToken = default
    )
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            Engine.Execute(script, source);
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
