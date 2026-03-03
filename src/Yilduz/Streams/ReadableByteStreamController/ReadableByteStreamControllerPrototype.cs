using Jint;
using Yilduz.Models;

namespace Yilduz.Streams.ReadableByteStreamController;

internal sealed class ReadableByteStreamControllerPrototype
    : PrototypeBase<ReadableByteStreamControllerInstance>
{
    public ReadableByteStreamControllerPrototype(
        Engine engine,
        ReadableByteStreamControllerConstructor constructor
    )
        : base(engine, nameof(ReadableByteStreamController), constructor) { }
}
