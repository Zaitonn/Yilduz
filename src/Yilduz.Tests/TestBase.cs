using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Jint;

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
    }

    protected virtual void OnDisposing() { }

    protected virtual Options GetOptions()
    {
        return new() { CancellationToken = Token };
    }

    /// <summary>
    /// Waits for a JavaScript condition to become true by polling at regular intervals.
    /// This is more reliable than fixed delays as it responds immediately when the condition is met.
    /// </summary>
    /// <param name="condition">JavaScript expression that should evaluate to true</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds (default: 5000)</param>
    /// <param name="pollingIntervalMs">Interval between condition checks in milliseconds (default: 10)</param>
    /// <returns>A task that completes when the condition is true or throws TimeoutException if timeout is reached</returns>
    protected async Task WaitForJsConditionAsync(
        string condition,
        int timeoutMs = 5000,
        int pollingIntervalMs = 10
    )
    {
        var startTime = DateTime.UtcNow;
        var timeout = TimeSpan.FromMilliseconds(timeoutMs);

        while (DateTime.UtcNow - startTime < timeout)
        {
            try
            {
                var result = Engine.Evaluate(condition);
                if (result.IsBoolean() && result.AsBoolean())
                {
                    return;
                }
            }
            catch
            {
                // Condition might reference undefined variables initially; continue polling
            }

            await Task.Delay(pollingIntervalMs, Token);
        }

        throw new TimeoutException(
            $"Condition '{condition}' did not become true within {timeoutMs}ms"
        );
    }
}
