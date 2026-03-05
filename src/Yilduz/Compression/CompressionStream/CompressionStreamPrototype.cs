using Jint;
using Yilduz.Models;

namespace Yilduz.Compression.CompressionStream;

internal sealed class CompressionStreamPrototype : PrototypeBase<CompressionStreamInstance>
{
    public CompressionStreamPrototype(Engine engine, CompressionStreamConstructor constructor)
        : base(engine, nameof(CompressionStream), constructor)
    {
        RegisterProperty("readable", stream => stream.Readable);
        RegisterProperty("writable", stream => stream.Writable);
    }
}
