using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Encoding.TextEncoderStream;

internal sealed class TextEncoderStreamPrototype : ObjectInstance
{
    private static readonly string EncodingName = nameof(TextEncoderStreamInstance.Encoding)
        .ToJsStyleName();
    private static readonly string EncodingGetterName = EncodingName.ToJsGetterName();

    private static readonly string ReadableName = nameof(TextEncoderStreamInstance.Readable)
        .ToJsStyleName();
    private static readonly string ReadableGetterName = ReadableName.ToJsGetterName();

    private static readonly string WritableName = nameof(TextEncoderStreamInstance.Writable)
        .ToJsStyleName();
    private static readonly string WritableGetterName = WritableName.ToJsGetterName();

    public TextEncoderStreamPrototype(Engine engine, TextEncoderStreamConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(TextEncoderStream));
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

    private static JsValue GetEncoding(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<TextEncoderStreamInstance>().Encoding;
    }

    private static JsValue GetReadable(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<TextEncoderStreamInstance>().Readable;
    }

    private static JsValue GetWritable(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<TextEncoderStreamInstance>().Writable;
    }
}
