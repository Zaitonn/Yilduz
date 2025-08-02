using Jint;
using Jint.Native.Object;
using Jint.Native.Symbol;

namespace Yilduz.Streams.ReadableByteStreamController;

internal sealed class ReadableByteStreamControllerPrototype : ObjectInstance
{
    public ReadableByteStreamControllerPrototype(
        Engine engine,
        ReadableByteStreamControllerConstructor constructor
    )
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(ReadableStream));
        SetOwnProperty("constructor", new(constructor, false, false, true));
    }
}
