using Jint;
using Jint.Native;

namespace Yilduz.Compression.DecompressionStream;

/// <summary>
/// https://compression.spec.whatwg.org/#decompression-stream
/// </summary>
public sealed class DecompressionStreamInstance : CompressionStreamBase
{
    internal DecompressionStreamInstance(
        Engine engine,
        WebApiIntrinsics webApiIntrinsics,
        JsValue format
    )
        : base(
            engine,
            webApiIntrinsics,
            ResolveProvider(
                engine,
                webApiIntrinsics.Options.Compression.DecompressorFactory,
                format.AsString()
            ),
            nameof(DecompressionStream)
        ) { }
}
