using Jint;
using Jint.Native;
using Yilduz.Events.Event;

namespace Yilduz.Events.MessageEvent;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/MessageEvent
/// <br/>
/// https://html.spec.whatwg.org/multipage/comms.html#the-messageevent-interface
/// </summary>
public sealed class MessageEventInstance : EventInstance
{
    internal MessageEventInstance(Engine engine, string type, JsValue data, string origin)
        : base(engine, type, Undefined)
    {
        Data = data;
        Origin = origin;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/MessageEvent/data
    /// <br/>
    /// https://html.spec.whatwg.org/multipage/comms.html#dom-messageevent-data
    /// </summary>
    public JsValue Data { get; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/MessageEvent/origin
    /// <br/>
    /// https://html.spec.whatwg.org/multipage/comms.html#dom-messageevent-origin
    /// </summary>
    public string Origin { get; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/MessageEvent/lastEventId
    /// <br/>
    /// https://html.spec.whatwg.org/multipage/comms.html#dom-messageevent-lasteventid
    /// </summary>
    public string LastEventId { get; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/MessageEvent/source
    /// <br/>
    /// https://html.spec.whatwg.org/multipage/comms.html#dom-messageevent-source
    /// </summary>
    public JsValue Source { get; } = Null;
}
