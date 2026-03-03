using Jint;
using Yilduz.Models;

namespace Yilduz.Streams.TransformStream;

internal sealed class TransformStreamPrototype : PrototypeBase<TransformStreamInstance>
{
    public TransformStreamPrototype(Engine engine, TransformStreamConstructor constructor)
        : base(engine, nameof(TransformStream), constructor)
    {
        RegisterProperty("readable", instance => instance.Readable);
        RegisterProperty("writable", instance => instance.Writable);
    }
}
