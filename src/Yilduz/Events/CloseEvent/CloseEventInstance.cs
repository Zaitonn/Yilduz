using Jint;
using Jint.Native;
using Yilduz.Events.Event;

namespace Yilduz.Events.CloseEvent;

/// <summary>
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
    /// https://websockets.spec.whatwg.org/#dom-closeevent-wasclean
    /// </summary>
    public bool WasClean { get; }

    /// <summary>
    /// https://websockets.spec.whatwg.org/#dom-closeevent-code
    /// </summary>
    public ushort Code { get; }

    /// <summary>
    /// https://websockets.spec.whatwg.org/#dom-closeevent-reason
    /// </summary>
    public string Reason { get; }
}
