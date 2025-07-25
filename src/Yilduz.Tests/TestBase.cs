using System;
using System.Threading;
using Jint;

namespace Yilduz.Tests;

public abstract class TestBase : IDisposable
{
    protected TestBase()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        Engine = EngineFactory.Create(_cancellationTokenSource.Token);
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
}
