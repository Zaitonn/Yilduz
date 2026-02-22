using System;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Events.EventTarget;
using Yilduz.Utils;

namespace Yilduz.Aborting.AbortSignal;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal
/// </summary>
public sealed class AbortSignalInstance : EventTargetInstance
{
    internal AbortSignalInstance(Engine engine)
        : base(engine) { }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal/aborted
    /// </summary>
    public bool Aborted { get; private set; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal/reason
    /// </summary>
    public JsValue Reason { get; private set; } = Undefined;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal/abort_event
    /// </summary>
    public JsValue OnAbort { get; set; } = Null;

    /// <summary>
    /// Notifies when the signal is aborted.
    /// </summary>
    public event EventHandler? Abort;

    internal void SetAborted(JsValue reason)
    {
        if (Aborted)
        {
            return;
        }

        Aborted = true;
        Reason = reason;

        if (reason.IsUndefined())
        {
            Reason = DOMExceptionHelper.CreateAbortError(
                Engine,
                "signal is aborted without reason"
            );
        }

        Abort?.Invoke(this, EventArgs.Empty);

        DispatchEvent(_webApiIntrinsics.Event.ConstructWithEventName("abort", Undefined));
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal/throwIfAborted
    /// </summary>
    public void ThrowIfAborted()
    {
        if (Aborted)
        {
            throw new JavaScriptException(Reason);
        }
    }
}
