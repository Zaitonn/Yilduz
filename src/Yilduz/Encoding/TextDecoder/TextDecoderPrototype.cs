using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;

namespace Yilduz.Encoding.TextDecoder;

internal sealed class TextDecoderPrototype : PrototypeBase<TextDecoderInstance>
{
    public TextDecoderPrototype(Engine engine, TextDecoderConstructor constructor)
        : base(engine, nameof(TextDecoder), constructor)
    {
        RegisterProperty("encoding", decoder => decoder.Encoding);
        RegisterProperty("fatal", decoder => decoder.Fatal);
        RegisterProperty("ignoreBOM", decoder => decoder.IgnoreBOM);

        RegisterMethod("decode", Decode);
    }

    private static JsValue Decode(TextDecoderInstance decoder, JsValue[] arguments)
    {
        return decoder.Decode(arguments.At(0), arguments.At(1));
    }
}
