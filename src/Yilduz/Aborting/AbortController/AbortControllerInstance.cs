using Jint;
using Jint.Native;
using Jint.Native.Object;
using Yilduz.Aborting.AbortSignal;

namespace Yilduz.Aborting.AbortController;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/AbortController
/// </summary>
public sealed class AbortControllerInstance : ObjectInstance
{
    internal AbortControllerInstance(Engine engine, AbortSignalPrototype abortSignalPrototype)
        : base(engine)
    {
        Signal = new(engine) { Prototype = abortSignalPrototype };
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/AbortController/signal
    /// </summary>
    public AbortSignalInstance Signal { get; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/AbortController/abort
    /// </summary>
    public void Abort(JsValue reason)
    {
        Signal.SetAborted(reason);
    }

    public override string ToString()
    {
        return "[object AbortController]";
    }
}
