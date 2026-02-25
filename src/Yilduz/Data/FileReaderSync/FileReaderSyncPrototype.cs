using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Data.FileReaderSync;

internal sealed class FileReaderSyncPrototype : ObjectInstance
{
    public static readonly string ReadAsArrayBufferName = nameof(ReadAsArrayBuffer).ToJsStyleName();
    public static readonly string ReadAsTextName = nameof(ReadAsText).ToJsStyleName();
    public static readonly string ReadAsDataURLName = nameof(ReadAsDataURL).ToJsStyleName();
    public static readonly string ReadAsBinaryStringName = nameof(ReadAsBinaryString)
        .ToJsStyleName();

    public FileReaderSyncPrototype(Engine engine, FileReaderSyncConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(FileReaderSync));
        SetOwnProperty("constructor", new(constructor, false, false, false));

        FastSetProperty(
            ReadAsArrayBufferName,
            new(
                new ClrFunction(Engine, ReadAsArrayBufferName, ReadAsArrayBuffer),
                false,
                false,
                true
            )
        );
        FastSetProperty(
            ReadAsTextName,
            new(new ClrFunction(Engine, ReadAsTextName, ReadAsText), false, false, true)
        );
        FastSetProperty(
            ReadAsDataURLName,
            new(new ClrFunction(Engine, ReadAsDataURLName, ReadAsDataURL), false, false, true)
        );
        FastSetProperty(
            ReadAsBinaryStringName,
            new(
                new ClrFunction(Engine, ReadAsBinaryStringName, ReadAsBinaryString),
                false,
                false,
                true
            )
        );
    }

    private JsValue ReadAsArrayBuffer(JsValue thisObject, JsValue[] arguments)
    {
        var reader = thisObject.EnsureThisObject<FileReaderSyncInstance>();
        arguments.EnsureCount(Engine, 1, ReadAsArrayBufferName, nameof(FileReaderSync));

        return reader.ReadAsArrayBuffer(arguments[0]);
    }

    private JsValue ReadAsText(JsValue thisObject, JsValue[] arguments)
    {
        var reader = thisObject.EnsureThisObject<FileReaderSyncInstance>();
        arguments.EnsureCount(Engine, 1, ReadAsTextName, nameof(FileReaderSync));

        var encoding = arguments.Length > 1 ? arguments[1].ToString() : "UTF-8";
        return reader.ReadAsText(arguments[0], encoding);
    }

    private JsValue ReadAsDataURL(JsValue thisObject, JsValue[] arguments)
    {
        var reader = thisObject.EnsureThisObject<FileReaderSyncInstance>();
        arguments.EnsureCount(Engine, 1, ReadAsDataURLName, nameof(FileReaderSync));

        return reader.ReadAsDataURL(arguments[0]);
    }

    private JsValue ReadAsBinaryString(JsValue thisObject, JsValue[] arguments)
    {
        var reader = thisObject.EnsureThisObject<FileReaderSyncInstance>();
        arguments.EnsureCount(Engine, 1, ReadAsBinaryStringName, nameof(FileReaderSync));

        return reader.ReadAsBinaryString(arguments[0]);
    }
}
