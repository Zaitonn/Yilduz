using System;
using System.Threading;

namespace Yilduz;

public sealed partial class Options
{
    public CancellationToken CancellationToken { get; init; }

    public TimeSpan WaitingTimeout { get; init; } = TimeSpan.FromSeconds(10);
}
