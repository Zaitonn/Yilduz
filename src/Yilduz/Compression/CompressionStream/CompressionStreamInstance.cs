using Jint;
using Jint.Native;

namespace Yilduz.Compression.CompressionStream;

/// <summary>
/// https://compression.spec.whatwg.org/#compression-stream
/// </summary>
public sealed class CompressionStreamInstance : CompressionStreamBase
{
    internal CompressionStreamInstance(
        Engine engine,
        WebApiIntrinsics webApiIntrinsics,
        JsValue format
    )
        : base(
            engine,
            webApiIntrinsics,
            ResolveProvider(
                engine,
                webApiIntrinsics.Options.Compression.CompressorFactory,
                format.AsString()
            ),
            nameof(CompressionStream)
        ) { }
}
