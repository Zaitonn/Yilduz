using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Runtime;

namespace Yilduz.Tests;

public abstract class TestBase : IDisposable
{
    static TestBase()
    {
        System.Text.Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    protected TestBase()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        Engine = new Engine(
            new Jint.Options { Modules = { RegisterRequire = true } }.CancellationToken(
                _cancellationTokenSource.Token
            )
        );
        Engine.InitializeWebApi(GetOptions());
    }

    private readonly CancellationTokenSource _cancellationTokenSource;

    protected CancellationToken Token => _cancellationTokenSource.Token;
    protected Engine Engine { get; }

    public void Dispose()
    {
        OnDisposing();

        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        Engine.Dispose();

        GC.SuppressFinalize(this);
    }

    protected virtual void OnDisposing() { }

    protected virtual Options GetOptions()
    {
        return new() { CancellationToken = Token };
    }

    protected async Task WaitForJsConditionAsync(
        string condition,
        int timeoutMs = 5000,
        int pollingIntervalMs = 10
    )
    {
        if (string.IsNullOrWhiteSpace(condition))
        {
            throw new ArgumentException("Condition cannot be null or empty", nameof(condition));
        }

        if (timeoutMs <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeoutMs),
                "Timeout must be greater than zero"
            );
        }

        if (pollingIntervalMs <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pollingIntervalMs),
                "Polling interval must be greater than zero"
            );
        }

        if (pollingIntervalMs > timeoutMs)
        {
            throw new ArgumentException("Polling interval must be less than or equal to timeout");
        }

        var startTime = DateTime.UtcNow;
        var timeout = TimeSpan.FromMilliseconds(timeoutMs);

        while (DateTime.UtcNow - startTime < timeout)
        {
            try
            {
                var result = Evaluate(condition);
                if (result.IsBoolean() && result.AsBoolean())
                {
                    return;
                }
            }
            catch (JavaScriptException)
            {
                // Condition might reference undefined variables initially; continue polling
            }

            await Task.Delay(pollingIntervalMs, Token);
        }

        throw new TimeoutException(
            $"Condition '{condition}' did not become true within {timeoutMs}ms"
        );
    }

    protected void Execute(string code)
    {
        lock (Engine)
        {
            Engine.Execute(code);
        }
    }

    protected JsValue Evaluate(string code)
    {
        lock (Engine)
        {
            return Engine.Evaluate(code);
        }
    }
}
