using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Encoding.TextDecoder;

internal sealed class TextDecoderPrototype : ObjectInstance
{
    private static readonly string EncodingName = nameof(TextDecoderInstance.Encoding)
        .ToJsStyleName();
    private static readonly string EncodingGetterName = EncodingName.ToJsGetterName();
    private static readonly string FatalName = nameof(TextDecoderInstance.Fatal).ToJsStyleName();
    private static readonly string FatalGetterName = FatalName.ToJsGetterName();
    private static readonly string IgnoreBOMName = nameof(TextDecoderInstance.IgnoreBOM)
        .ToJsStyleName();
    private static readonly string IgnoreBOMGetterName = IgnoreBOMName.ToJsGetterName();
    private static readonly string DecodeName = nameof(Decode).ToJsStyleName();

    public TextDecoderPrototype(Engine engine, TextDecoderConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(TextDecoder));
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

        // fatal property
        FastSetProperty(
            FatalName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, FatalGetterName, GetFatal),
                set: null,
                false,
                true
            )
        );

        // ignoreBOM property
        FastSetProperty(
            IgnoreBOMName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, IgnoreBOMGetterName, GetIgnoreBOM),
                set: null,
                false,
                true
            )
        );

        // decode method
        FastSetProperty(
            DecodeName,
            new PropertyDescriptor(
                new ClrFunction(engine, DecodeName, Decode, length: 0),
                false,
                false,
                true
            )
        );
    }

    private JsValue GetEncoding(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<TextDecoderInstance>().Encoding;
    }

    private JsValue GetFatal(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<TextDecoderInstance>().Fatal;
    }

    private JsValue GetIgnoreBOM(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<TextDecoderInstance>().IgnoreBOM;
    }

    private JsValue Decode(JsValue thisObject, JsValue[] arguments)
    {
        var decoder = thisObject.EnsureThisObject<TextDecoderInstance>();

        var input = arguments.At(0);
        var options = arguments.At(1);

        return decoder.Decode(input, options);
    }
}
