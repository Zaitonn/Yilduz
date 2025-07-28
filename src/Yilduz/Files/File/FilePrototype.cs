using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Utils;

namespace Yilduz.Files.File;

internal sealed class FilePrototype : ObjectInstance
{
    private static readonly string LastModifiedName = nameof(FileInstance.LastModified)
        .ToJsStyleName();
    private static readonly string LastModifiedGetterName = LastModifiedName.ToJsGetterName();
    private static readonly string NameName = nameof(FileInstance.Name).ToJsStyleName();
    private static readonly string NameGetterName = NameName.ToJsGetterName();

    public FilePrototype(Engine engine, FileConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(File));
        SetOwnProperty("constructor", new(constructor, false, false, false));

        FastSetProperty(
            LastModifiedName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, LastModifiedGetterName, GetLastModified),
                set: null,
                false,
                true
            )
        );
        FastSetProperty(
            NameName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, NameGetterName, GetName),
                set: null,
                false,
                true
            )
        );
    }

    private JsValue GetLastModified(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<FileInstance>().LastModified;
    }

    private JsValue GetName(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<FileInstance>().Name;
    }
}
