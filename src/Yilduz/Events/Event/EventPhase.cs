namespace Yilduz.Events.Event;

/// <summary>
/// Represents the different phases of an event in the event flow.
/// </summary>
public enum EventPhase
{
    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/eventPhase#event.none
    /// </summary>
    NONE = 0,

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/eventPhase#event.capturing_phase
    /// </summary>
    CAPTURING_PHASE = 1,

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/eventPhase#event.at_target
    /// </summary>
    AT_TARGET = 2,

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Event/eventPhase#event.bubbling_phase
    /// </summary>
    BUBBLING_PHASE = 3,
}
