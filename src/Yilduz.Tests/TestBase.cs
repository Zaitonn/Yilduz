using System;
using System.Threading;
using Jint;

namespace Yilduz.Tests;

public abstract class TestBase : IDisposable
{
    protected TestBase()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        Engine = new Engine(
            new Jint.Options { Modules = { RegisterRequire = true } }.CancellationToken(
                _cancellationTokenSource.Token
            )
        );
        Engine.AddAPIs(GetOptions());
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
}
