using System;
using System.Threading;
using Jint;
using Yilduz.Console;

namespace Yilduz;

/// <summary>
/// Options for configuring the Yilduz web API.
/// </summary>
public sealed partial class Options
{
    /// <summary>
    /// A factory that provides the <see cref="IConsole"/> implementation used by the <c>console</c> global object.
    /// </summary>
    public Func<Engine, IConsole>? ConsoleFactory { get; init; }

    /// <summary>
    /// A cancellation token that is passed to all web API operations used to manage their lifetime. This allows for cooperative cancellation of long-running operations.
    /// <br/>
    /// It is required that the provided token can be cancelled, otherwise an exception will be thrown during initialization.
    /// So just passing <see cref="CancellationToken.None"/> is not sufficient, as it cannot be cancelled.
    /// <br/>
    /// It's recommended to use the same cancellation token as the engine's. This way, when the engine is disposed, all ongoing web API operations will be cancelled as well.
    /// </summary>
    public required CancellationToken CancellationToken { get; init; }

    /// <summary>
    /// The maximum amount of time to wait for a response from a web API operation before timing out. This applies to the event loop and timers.
    /// </summary>
    public TimeSpan WaitingTimeout { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>
    /// An optional handler that is called when an unhandled exception occurs in any asynchronous operation. This can be used to log or handle exceptions that would otherwise be swallowed.
    /// </summary>
    public Action<Exception>? UnhandledExceptionHandler { get; init; }

    /// <summary>
    /// The base URL used for resolving relative URLs in network APIs like <c>fetch()</c>, <c>XMLHttpRequest</c> or <c>WebSocket</c>.
    /// </summary>
    public Uri? BaseUrl { get; init; }
}
