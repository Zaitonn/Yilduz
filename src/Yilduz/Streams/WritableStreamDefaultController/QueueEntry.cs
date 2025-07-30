using Jint.Native;

namespace Yilduz.Streams.WritableStreamDefaultController;

/// <summary>
/// Internal record for queue entries
/// </summary>
internal readonly record struct QueueEntry
{
    public JsValue Value { get; init; }
    public double Size { get; init; }
}
