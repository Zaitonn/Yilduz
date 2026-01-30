using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Encoding.TextDecoderStream;

internal sealed class TextDecoderStreamPrototype : ObjectInstance
{
    private static readonly string EncodingName = nameof(TextDecoderStreamInstance.Encoding)
        .ToJsStyleName();
    private static readonly string EncodingGetterName = EncodingName.ToJsGetterName();

    private static readonly string FatalName = nameof(TextDecoderStreamInstance.Fatal)
        .ToJsStyleName();
    private static readonly string FatalGetterName = FatalName.ToJsGetterName();

    private static readonly string IgnoreBOMName = nameof(TextDecoderStreamInstance.IgnoreBOM)
        .ToJsStyleName();
    private static readonly string IgnoreBOMGetterName = IgnoreBOMName.ToJsGetterName();

    private static readonly string ReadableName = nameof(TextDecoderStreamInstance.Readable)
        .ToJsStyleName();
    private static readonly string ReadableGetterName = ReadableName.ToJsGetterName();

    private static readonly string WritableName = nameof(TextDecoderStreamInstance.Writable)
        .ToJsStyleName();
    private static readonly string WritableGetterName = WritableName.ToJsGetterName();

    public TextDecoderStreamPrototype(Engine engine, TextDecoderStreamConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(TextDecoderStream));
        SetOwnProperty("constructor", new(constructor, false, false, true));

        FastSetProperty(
            EncodingName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, EncodingGetterName, GetEncoding),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            FatalName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, FatalGetterName, GetFatal),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            IgnoreBOMName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, IgnoreBOMGetterName, GetIgnoreBOM),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            ReadableName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ReadableGetterName, GetReadable),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            WritableName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, WritableGetterName, GetWritable),
                set: null,
                false,
                true
            )
        );
    }

    private JsValue GetEncoding(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<TextDecoderStreamInstance>().Encoding;
    }

    private JsValue GetFatal(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<TextDecoderStreamInstance>().Fatal;
    }

    private JsValue GetIgnoreBOM(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<TextDecoderStreamInstance>().IgnoreBOM;
    }

    private JsValue GetReadable(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<TextDecoderStreamInstance>().Readable;
    }

    private JsValue GetWritable(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<TextDecoderStreamInstance>().Writable;
    }
}
