using Jint.Native;
using Yilduz.Streams.ReadableStream;

namespace Yilduz.Network.Body;

internal sealed class BodyConcept(ReadableStreamInstance stream, JsValue? source, long length)
{
    public ReadableStreamInstance Stream { get; private set; } = stream;
    public JsValue? Source { get; } = source;
    public long Length { get; } = length;

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-body-clone
    /// </summary>
    public BodyConcept Clone()
    {
        var (out1, out2) = Stream.Tee();
        Stream = out1;
        return new(out2, Source, Length);
    }
}
