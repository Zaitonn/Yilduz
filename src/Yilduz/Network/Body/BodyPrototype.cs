using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Network.Body;

internal abstract class BodyPrototype : ObjectInstance
{
    private static readonly string ArrayBufferName = nameof(BodyInstance.ArrayBuffer)
        .ToJsStyleName();
    private static readonly string BlobName = nameof(BodyInstance.Blob).ToJsStyleName();
    private static readonly string FormDataName = nameof(BodyInstance.FormData).ToJsStyleName();
    private static readonly string JsonName = nameof(BodyInstance.Json).ToJsStyleName();
    private static readonly string TextName = nameof(BodyInstance.Text).ToJsStyleName();
    private static readonly string BytesName = nameof(BodyInstance.Bytes).ToJsStyleName();

    protected BodyPrototype(Engine engine)
        : base(engine)
    {
        FastSetProperty(
            ArrayBufferName,
            new PropertyDescriptor(
                new ClrFunction(engine, ArrayBufferName, ArrayBuffer),
                false,
                false,
                true
            )
        );

        FastSetProperty(
            BlobName,
            new PropertyDescriptor(new ClrFunction(engine, BlobName, Blob), false, false, true)
        );

        FastSetProperty(
            FormDataName,
            new PropertyDescriptor(
                new ClrFunction(engine, FormDataName, FormData),
                false,
                false,
                true
            )
        );

        FastSetProperty(
            JsonName,
            new PropertyDescriptor(new ClrFunction(engine, JsonName, Json), false, false, true)
        );

        FastSetProperty(
            TextName,
            new PropertyDescriptor(new ClrFunction(engine, TextName, Text), false, false, true)
        );

        FastSetProperty(
            BytesName,
            new PropertyDescriptor(new ClrFunction(engine, BytesName, Bytes), false, false, true)
        );
    }

    private static JsValue ArrayBuffer(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<BodyInstance>().ArrayBuffer();
    }

    private static JsValue Blob(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<BodyInstance>().Blob();
    }

    private static JsValue FormData(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<BodyInstance>().FormData();
    }

    private static JsValue Json(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<BodyInstance>().Json();
    }

    private static JsValue Text(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<BodyInstance>().Text();
    }

    private static JsValue Bytes(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<BodyInstance>().Bytes();
    }
}
