using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Extensions;
using Yilduz.Models;
using Yilduz.Utils;

namespace Yilduz.Encoding.TextEncoder;

internal sealed class TextEncoderPrototype : PrototypeBase<TextEncoderInstance>
{
    public TextEncoderPrototype(Engine engine, TextEncoderConstructor constructor)
        : base(engine, nameof(TextEncoder), constructor)
    {
        RegisterProperty("encoding", encoder => encoder.Encoding);

        RegisterMethod("encode", Encode);
        RegisterMethod("encodeInto", EncodeInto, 2);
    }

    private JsValue Encode(TextEncoderInstance encoder, JsValue[] arguments)
    {
        var input = arguments.Length > 0 ? arguments[0].ToArgumentString() : string.Empty;
        return Engine.Intrinsics.Uint8Array.Construct(encoder.Encode(input));
    }

    private JsValue EncodeInto(TextEncoderInstance encoder, JsValue[] arguments)
    {
        var destination = arguments[1];

        if (!destination.IsUint8Array() || destination is not JsTypedArray de)
        {
            TypeErrorHelper.Throw(
                Engine,
                "parameter 2 is not of type 'Uint8Array'.",
                "encodeInto",
                nameof(TextEncoder)
            );
            return Undefined;
        }

        var (read, written) = encoder.EncodeInto(arguments.At(0).ToString(), de);
        var result = Engine.Intrinsics.Object.Construct(Arguments.Empty);
        result.Set("read", read);
        result.Set("written", written);
        return result;
    }
}
