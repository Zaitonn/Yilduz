using Jint;
using Yilduz.Models;

namespace Yilduz.Compression.DecompressionStream;

internal sealed class DecompressionStreamPrototype : PrototypeBase<DecompressionStreamInstance>
{
    public DecompressionStreamPrototype(Engine engine, DecompressionStreamConstructor constructor)
        : base(engine, nameof(DecompressionStream), constructor)
    {
        RegisterProperty("readable", stream => stream.Readable);
        RegisterProperty("writable", stream => stream.Writable);
    }
}
