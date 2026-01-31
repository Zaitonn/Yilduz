using Yilduz.Aborting.AbortSignal;

namespace Yilduz.Events.EventTarget;

/// <summary>
/// Represents options for event listeners on an EventTarget.
/// </summary>
public readonly record struct EventTargetOptions
{
    public bool Capture { get; init; }

    public bool Once { get; init; }

    public bool Passive { get; init; }

    public AbortSignalInstance? Signal { get; init; }
}
