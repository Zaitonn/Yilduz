using Jint.Native;

namespace Yilduz.Streams.ReadableByteStreamController;

internal record struct PullIntoDescriptor
{
    public PullIntoDescriptor() { }

    public byte[] Buffer { get; } = [];

    public int BufferByteLength { get; }

    public int ByteOffset { get; init; }

    public int ByteLength { get; init; }

    public int BytesFilled { get; set; }

    public int MinimumFill { get; set; }

    public int ElementSize { get; set; }

    public JsValue ViewConstructor { get; set; } = JsValue.Null;

    public ReaderType ReaderType { get; init; } = ReaderType.None;
}
