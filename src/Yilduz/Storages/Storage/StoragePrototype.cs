using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Utils;

namespace Yilduz.Storages.Storage;

internal sealed class StoragePrototype : ObjectInstance
{
    private static readonly string GetItemName = nameof(GetItem).ToJsStyleName();
    private static readonly string SetItemName = nameof(SetItem).ToJsStyleName();
    private static readonly string RemoveItemName = nameof(RemoveItem).ToJsStyleName();
    private static readonly string ClearName = nameof(Clear).ToJsStyleName();
    private static readonly string KeyName = nameof(Key).ToJsStyleName();
    private static readonly string LengthName = nameof(StorageInstance.Length).ToJsStyleName();

    public StoragePrototype(Engine engine, StorageConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(Storage));
        FastSetProperty("constructor", new(constructor, false, false, true));

        FastSetProperty(
            LengthName,
            new GetSetPropertyDescriptor(
                new ClrFunction(Engine, LengthName.ToJsGetterName(), GetLength),
                null,
                false,
                true
            )
        );

        FastSetProperty(
            GetItemName,
            new(new ClrFunction(Engine, GetItemName, GetItem), false, false, true)
        );
        FastSetProperty(
            SetItemName,
            new(new ClrFunction(Engine, SetItemName, SetItem), false, false, true)
        );
        FastSetProperty(
            RemoveItemName,
            new(new ClrFunction(Engine, RemoveItemName, RemoveItem), false, false, true)
        );
        FastSetProperty(
            ClearName,
            new(new ClrFunction(Engine, ClearName, Clear), false, false, true)
        );
        FastSetProperty(KeyName, new(new ClrFunction(Engine, KeyName, Key), false, false, true));
    }

    private JsValue GetLength(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<StorageInstance>().Length;
    }

    private JsValue GetItem(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, GetItemName, "Storage");

        var key = arguments.At(0).ToString();
        return thisObject.EnsureThisObject<StorageInstance>().GetItem(key) ?? Null;
    }

    private JsValue RemoveItem(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, RemoveItemName, "Storage");

        var key = arguments.At(0).ToString();
        thisObject.EnsureThisObject<StorageInstance>().RemoveItem(key);

        return Undefined;
    }

    private JsValue SetItem(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 2, SetItemName, "Storage");

        var key = arguments.At(0).ToString();
        var value = arguments.At(1);
        thisObject
            .EnsureThisObject<StorageInstance>()
            .SetItem(
                key,
                // not standard
                // to keep same behavior as in Browser
                value.IsArray()
                    ? string.Join(",", value.AsArray())
                    : value.ToString()
            );

        return Undefined;
    }

    private JsValue Clear(JsValue thisObject, JsValue[] arguments)
    {
        thisObject.EnsureThisObject<StorageInstance>().Clear();

        return Undefined;
    }

    private JsValue Key(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, KeyName, "Storage");

        var index = arguments.At(0).AsNumber();
        return thisObject.EnsureThisObject<StorageInstance>().Key((int)index) ?? Null;
    }
}
