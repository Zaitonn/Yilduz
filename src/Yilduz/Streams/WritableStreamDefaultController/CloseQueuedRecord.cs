using Jint.Native;
using Jint.Runtime;

namespace Yilduz.Streams.WritableStreamDefaultController;

/// <summary>
/// Special value to indicate a close operation in the queue
/// </summary>
internal sealed class CloseQueuedRecord : JsValue
{
    public static readonly JsValue Instance = new CloseQueuedRecord();

    private CloseQueuedRecord()
        : base(Types.Object) { }

    public override object? ToObject() => null;
}
