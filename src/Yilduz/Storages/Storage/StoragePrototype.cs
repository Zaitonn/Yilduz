using Jint;
using Jint.Native;
using Yilduz.Models;

namespace Yilduz.Storages.Storage;

internal sealed class StoragePrototype : PrototypeBase<StorageInstance>
{
    public StoragePrototype(Engine engine, StorageConstructor constructor)
        : base(engine, nameof(Storage), constructor)
    {
        RegisterProperty("length", storage => storage.Length);

        RegisterMethod("getItem", GetItem, 1);
        RegisterMethod("setItem", SetItem, 2);
        RegisterMethod("removeItem", RemoveItem, 1);
        RegisterMethod("clear", Clear);
        RegisterMethod("key", Key, 1);
    }

    private static JsValue GetItem(StorageInstance instance, JsValue[] arguments)
    {
        var key = arguments[0].ToString();
        return instance.GetItem(key) ?? Null;
    }

    private static JsValue SetItem(StorageInstance instance, JsValue[] arguments)
    {
        var key = arguments[0].ToString();
        var value = arguments[1];
        instance.SetItem(
            key,
            // not standard
            // just to keep same behavior as in Browser
            value.IsArray()
                ? string.Join(",", value.AsArray())
                : value.ToString()
        );
        return Undefined;
    }

    private static JsValue RemoveItem(StorageInstance instance, JsValue[] arguments)
    {
        var key = arguments[0].ToString();
        instance.RemoveItem(key);
        return Undefined;
    }

    private static JsValue Clear(StorageInstance instance, JsValue[] arguments)
    {
        instance.Clear();
        return Undefined;
    }

    private static JsValue Key(StorageInstance instance, JsValue[] arguments)
    {
        var index = arguments[0].AsNumber();
        return instance.Key((int)index) ?? Null;
    }
}
