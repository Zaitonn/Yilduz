namespace Yilduz.Events.Event;

/// <summary>
/// Represents the different phases of an event in the event flow.
/// </summary>
public static class EventPhases
{
    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/eventPhase#event.none
    /// </summary>
    public const int NONE = 0;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/eventPhase#event.capturing_phase
    /// </summary>
    public const int CAPTURING_PHASE = 1;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/eventPhase#event.at_target
    /// </summary>
    public const int AT_TARGET = 2;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/eventPhase#event.bubbling_phase
    /// </summary>
    public const int BUBBLING_PHASE = 3;
}
