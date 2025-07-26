using System;
using System.Threading;
using Jint;
using Yilduz.Console;

namespace Yilduz;

public sealed partial class Options
{
    public Func<Engine, IConsole>? ConsoleFactory { get; set; }

    public CancellationToken CancellationToken { get; set; }

    public TimeSpan WaitingTimeout { get; set; } = TimeSpan.FromSeconds(10);
}
