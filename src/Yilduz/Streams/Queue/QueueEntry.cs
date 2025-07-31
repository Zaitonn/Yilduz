using Jint.Native;

namespace Yilduz.Streams.Queue;

internal sealed record QueueEntry(JsValue Value, double Size);
