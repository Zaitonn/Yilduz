using System;
using System.Threading;
using Jint;
using Yilduz.Console;

namespace Yilduz;

public sealed partial class Options
{
    public Func<Engine, IConsole>? ConsoleFactory { get; init; }

    public required CancellationToken CancellationToken { get; init; }

    public TimeSpan WaitingTimeout { get; init; } = TimeSpan.FromSeconds(10);
}
