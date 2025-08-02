namespace Yilduz.Streams.ReadableByteStreamController;

internal sealed record ReadableByteStreamQueueEntry
{
    public byte[] Buffer { get; init; } = [];

    public int ByteOffset { get; init; }

    public int ByteLength { get; init; }
}
