using System;
using System.Text;
using System.Threading;
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
}
