using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;
using Yilduz.Utils;

namespace Yilduz.Files.Blob;

internal sealed class BlobPrototype : ObjectInstance
{
    private static readonly string SizeName = nameof(BlobInstance.Size).ToJsStyleName();
    private static readonly string SizeGetterName = SizeName.ToJsGetterName();
    private static readonly string TypeName = nameof(BlobInstance.Type).ToJsStyleName();
    private static readonly string TypeGetterName = TypeName.ToJsGetterName();

    private static readonly string SliceName = nameof(Slice).ToJsStyleName();
    private static readonly string StreamName = nameof(Stream).ToJsStyleName();
    private static readonly string TextName = nameof(Text).ToJsStyleName();
    private static readonly string ArrayBufferName = nameof(ArrayBuffer).ToJsStyleName();
    private static readonly string BytesName = nameof(Bytes).ToJsStyleName();

    public BlobPrototype(Engine engine, BlobConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(Blob));
        SetOwnProperty("constructor", new(constructor, false, false, false));

        FastSetProperty(
            SizeName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, SizeGetterName, GetSize),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            TypeName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, TypeGetterName, GetType),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(TextName, new(new ClrFunction(Engine, TextName, Text), false, false, true));
        FastSetProperty(
            StreamName,
            new(new ClrFunction(Engine, StreamName, Stream), false, false, true)
        );
        FastSetProperty(
            ArrayBufferName,
            new(new ClrFunction(Engine, ArrayBufferName, ArrayBuffer), false, false, true)
        );
        FastSetProperty(
            BytesName,
            new(new ClrFunction(Engine, BytesName, Bytes), false, false, true)
        );
        FastSetProperty(
            SliceName,
            new(new ClrFunction(Engine, SliceName, Slice), false, false, true)
        );
    }

    private static JsValue GetSize(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<BlobInstance>().Size;
    }

    private static JsValue GetType(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<BlobInstance>().Type;
    }

    private JsValue Text(JsValue thisObject, JsValue[] arguments)
    {
        var blob = thisObject.EnsureThisObject<BlobInstance>();
        return PromiseHelper.CreateResolvedPromise(Engine, blob.Text()).Promise;
    }

    private JsValue Stream(JsValue thisObject, JsValue[] arguments)
    {
        var blob = thisObject.EnsureThisObject<BlobInstance>();
        return PromiseHelper.CreateResolvedPromise(Engine, blob.Stream()).Promise;
    }

    private JsValue ArrayBuffer(JsValue thisObject, JsValue[] arguments)
    {
        var blob = thisObject.EnsureThisObject<BlobInstance>();
        return PromiseHelper.CreateResolvedPromise(Engine, blob.ArrayBuffer()).Promise;
    }

    private JsValue Bytes(JsValue thisObject, JsValue[] arguments)
    {
        var blob = thisObject.EnsureThisObject<BlobInstance>();
        return PromiseHelper.CreateResolvedPromise(Engine, blob.Bytes()).Promise;
    }

    private JsValue Slice(JsValue thisObject, JsValue[] arguments)
    {
        var blob = thisObject.EnsureThisObject<BlobInstance>();

        var start = arguments.At(0).IsUndefined() ? 0 : (int)arguments.At(0).AsNumber();
        var end = arguments.At(1).IsUndefined() ? null : (int?)arguments.At(1).AsNumber();
        var contentType = arguments.At(2).IsUndefined() ? string.Empty : arguments.At(2).AsString();

        return blob.Slice(start, end, contentType);
    }
}
