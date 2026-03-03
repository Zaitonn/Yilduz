using Jint;
using Yilduz.Models;

namespace Yilduz.Encoding.TextEncoderStream;

internal sealed class TextEncoderStreamPrototype : PrototypeBase<TextEncoderStreamInstance>
{
    public TextEncoderStreamPrototype(Engine engine, TextEncoderStreamConstructor constructor)
        : base(engine, nameof(TextEncoderStream), constructor)
    {
        RegisterProperty("encoding", stream => stream.Encoding);
        RegisterProperty("readable", stream => stream.Readable);
        RegisterProperty("writable", stream => stream.Writable);
    }
}
