using Jint;
using Jint.Native;
using Yilduz.Events.EventTarget;

namespace Yilduz.Network.XMLHttpRequestEventTarget;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequestEventTarget
/// </summary>
public class XMLHttpRequestEventTargetInstance : EventTargetInstance
{
    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/abort_event
    /// </summary>
    public JsValue OnAbort { get; set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/error_event
    /// </summary>
    public JsValue OnError { get; set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/load_event
    /// </summary>
    public JsValue OnLoad { get; set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/loadstart_event
    /// </summary>
    public JsValue OnLoadStart { get; set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/progress_event
    /// </summary>
    public JsValue OnProgress { get; set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/timeout_event
    /// </summary>
    public JsValue OnTimeout { get; set; } = Null;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/XMLHttpRequest/loadend_event
    /// </summary>
    public JsValue OnLoadEnd { get; set; } = Null;

    internal XMLHttpRequestEventTargetInstance(Engine engine)
        : base(engine) { }
}
