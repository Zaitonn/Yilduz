using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Streams.TransformStream;

internal sealed class TransformStreamPrototype : ObjectInstance
{
    private static readonly string ReadableName = nameof(TransformStreamInstance.Readable)
        .ToJsStyleName();
    private static readonly string ReadableGetterName = ReadableName.ToJsGetterName();

    private static readonly string WritableName = nameof(TransformStreamInstance.Writable)
        .ToJsStyleName();
    private static readonly string WritableGetterName = WritableName.ToJsGetterName();

    private readonly TransformStreamConstructor _constructor;

    public TransformStreamPrototype(Engine engine, TransformStreamConstructor constructor)
        : base(engine)
    {
        _constructor = constructor;
        Set(GlobalSymbolRegistry.ToStringTag, nameof(TransformStream));
        SetOwnProperty("constructor", new(_constructor, false, false, true));
        DefineProperties();
    }

    private void DefineProperties()
    {
        // Readable property (getter only)
        FastSetProperty(
            ReadableName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(Engine, ReadableGetterName, GetReadable),
                set: null,
                false,
                true
            )
        );

        // Writable property (getter only)
        FastSetProperty(
            WritableName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(Engine, WritableGetterName, GetWritable),
                set: null,
                false,
                true
            )
        );
    }

    private JsValue GetReadable(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<TransformStreamInstance>().Readable;
    }

    private JsValue GetWritable(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<TransformStreamInstance>().Writable;
    }
}
