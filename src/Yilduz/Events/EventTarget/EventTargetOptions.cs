using Yilduz.Aborting.AbortSignal;

namespace Yilduz.Events.EventTarget;

public readonly record struct EventTargetOptions
{
    public bool Capture { get; init; }

    public bool Once { get; init; }

    public bool Passive { get; init; }

    public AbortSignalInstance? Signal { get; init; }
}
