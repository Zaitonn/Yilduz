using Jint;
using Yilduz.Models;

namespace Yilduz.Encoding.TextDecoderStream;

internal sealed class TextDecoderStreamPrototype : PrototypeBase<TextDecoderStreamInstance>
{
    public TextDecoderStreamPrototype(Engine engine, TextDecoderStreamConstructor constructor)
        : base(engine, nameof(TextDecoderStream), constructor)
    {
        RegisterProperty("encoding", stream => stream.Encoding);
        RegisterProperty("fatal", stream => stream.Fatal);
        RegisterProperty("ignoreBOM", stream => stream.IgnoreBOM);
        RegisterProperty("readable", stream => stream.Readable);
        RegisterProperty("writable", stream => stream.Writable);
    }
}
