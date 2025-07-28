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
    internal AbortControllerInstance(Engine engine)
        : base(engine) { }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/AbortController/signal
    /// </summary>
    public required AbortSignalInstance Signal { get; init; }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/AbortController/abort
    /// </summary>
    public void Abort(JsValue reason)
    {
        Signal.SetAborted(reason);
    }
}
