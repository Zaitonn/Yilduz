using Jint;
using Yilduz.Events.Event;

namespace Yilduz.Events.CloseEvent;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/CloseEvent
/// <br/>
/// https://websockets.spec.whatwg.org/#the-closeevent-interface
/// </summary>
public sealed class CloseEventInstance : EventInstance
{
    internal CloseEventInstance(Engine engine, bool wasClean, ushort code, string reason)
        : base(engine, "close", Undefined)
    {
        WasClean = wasClean;
        Code = code;
        Reason = reason;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/CloseEvent/wasClean
    /// <br/>
    /// https://websockets.spec.whatwg.org/#dom-closeevent-wasclean
    /// </summary>
    public bool WasClean { get; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/CloseEvent/code
    /// <br/>
    /// https://websockets.spec.whatwg.org/#dom-closeevent-code
    /// </summary>
    public ushort Code { get; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/CloseEvent/reason
    /// <br/>
    /// https://websockets.spec.whatwg.org/#dom-closeevent-reason
    /// </summary>
    public string Reason { get; }
}
