using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;
using Yilduz.Utils;

namespace Yilduz.Encoding.TextEncoder;

internal sealed class TextEncoderPrototype : ObjectInstance
{
    private static readonly string EncodingName = nameof(TextEncoderInstance.Encoding)
        .ToJsStyleName();
    private static readonly string EncodingGetterName = EncodingName.ToJsGetterName();
    private static readonly string EncodeName = nameof(Encode).ToJsStyleName();
    private static readonly string EncodeIntoName = nameof(EncodeInto).ToJsStyleName();

    public TextEncoderPrototype(Engine engine, TextEncoderConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(TextEncoder));
        SetOwnProperty("constructor", new(constructor, false, false, false));

        // encoding property
        FastSetProperty(
            EncodingName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, EncodingGetterName, GetEncoding),
                set: null,
                false,
                true
            )
        );

        // encode method
        FastSetProperty(
            EncodeName,
            new PropertyDescriptor(
                new ClrFunction(engine, EncodeName, Encode, length: 1),
                false,
                false,
                true
            )
        );

        // encodeInto method
        FastSetProperty(
            EncodeIntoName,
            new PropertyDescriptor(
                new ClrFunction(engine, EncodeIntoName, EncodeInto, length: 2),
                false,
                false,
                true
            )
        );
    }

    private JsValue GetEncoding(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<TextEncoderInstance>().Encoding;
    }

    private JsValue Encode(JsValue thisObject, JsValue[] arguments)
    {
        var input = arguments.Length > 0 ? arguments[0].ToArgumentString() : string.Empty;

        return thisObject.EnsureThisObject<TextEncoderInstance>().Encode(input);
    }

    private JsValue EncodeInto(JsValue thisObject, JsValue[] arguments)
    {
        var textEncoder = thisObject.EnsureThisObject<TextEncoderInstance>();

        arguments.EnsureCount(Engine, 2, EncodeIntoName, nameof(TextEncoder));

        var input = arguments[0].ToArgumentString();
        var destination = arguments[1];

        if (!destination.IsUint8Array() || destination is not JsTypedArray de)
        {
            TypeErrorHelper.Throw(
                Engine,
                "parameter 2 is not of type 'Uint8Array'.",
                EncodeIntoName,
                nameof(TextEncoder)
            );
            return Undefined;
        }

        return textEncoder.EncodeInto(input, de);
    }
}
