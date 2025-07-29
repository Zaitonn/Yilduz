using Jint.Native;
using Jint.Native.Promise;

namespace Yilduz.Streams.WritableStream;

internal readonly record struct PendingAbortRequest
{
    public JsValue Reason { get; init; }

    public ManualPromise Promise { get; init; }

    public bool WasAlreadyErroring { get; init; }
}
