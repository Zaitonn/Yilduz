using System;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Events.EventTarget;

namespace Yilduz.Aborting.AbortSignal;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal
/// </summary>
public class AbortSignalInstance : EventTargetInstance
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
    /// Notifies when the signal is aborted.
    /// </summary>
    public event EventHandler? Abort;

    /// <summary>
    /// Sets the signal as aborted.
    /// </summary>
    protected internal void SetAborted(JsValue reason)
    {
        if (Aborted)
        {
            return;
        }

        Aborted = true;
        Reason = reason;

        Abort?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal/throwIfAborted
    /// </summary>
    public void ThrowIfAborted()
    {
        if (Aborted)
        {
            throw new JavaScriptException(
                Reason.IsUndefined()
                    ? "signal is aborted without reason"
                    : $"signal is aborted with reason: {Reason}"
            );
        }
    }
}
