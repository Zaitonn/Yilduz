using System;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Events.Event;
using Yilduz.Events.EventTarget;

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

    public JsValue Onabort { get; set; } = Undefined;

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

        Abort?.Invoke(this, EventArgs.Empty);
        DispatchEvent(new(Engine, "abort", Undefined) { Prototype = new EventPrototype(Engine) });
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

    public override string ToString()
    {
        return "[object AbortSignal]";
    }
}
