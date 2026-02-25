using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Data.Blob;
using Yilduz.Extensions;
using Yilduz.Iterator;

namespace Yilduz.Data.FormData;

internal sealed class FormDataPrototype : ObjectInstance
{
    private static readonly string AppendName = nameof(Append).ToJsStyleName();
    private static readonly string DeleteName = nameof(Delete).ToJsStyleName();
    private static readonly string GetName = nameof(Get).ToJsStyleName();
    private static readonly string GetAllName = nameof(GetAll).ToJsStyleName();
    private static readonly string HasName = nameof(Has).ToJsStyleName();
    private static readonly string SetName = nameof(Set).ToJsStyleName();
    private static readonly string EntriesName = nameof(Entries).ToJsStyleName();
    private static readonly string KeysName = nameof(Keys).ToJsStyleName();
    private static readonly string ValuesName = nameof(Values).ToJsStyleName();
    private static readonly string ForEachName = nameof(ForEach).ToJsStyleName();

    public FormDataPrototype(Engine engine, FormDataConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(FormData));
        SetOwnProperty("constructor", new(constructor, false, false, true));

        FastSetProperty(
            AppendName,
            new(new ClrFunction(Engine, AppendName, Append), false, false, true)
        );
        FastSetProperty(
            DeleteName,
            new(new ClrFunction(Engine, DeleteName, Delete), false, false, true)
        );
        FastSetProperty(GetName, new(new ClrFunction(Engine, GetName, Get), false, false, true));
        FastSetProperty(
            GetAllName,
            new(new ClrFunction(Engine, GetAllName, GetAll), false, false, true)
        );
        FastSetProperty(HasName, new(new ClrFunction(Engine, HasName, Has), false, false, true));
        FastSetProperty(SetName, new(new ClrFunction(Engine, SetName, Set), false, false, true));
        FastSetProperty(
            EntriesName,
            new(new ClrFunction(Engine, EntriesName, Entries), false, false, true)
        );
        FastSetProperty(KeysName, new(new ClrFunction(Engine, KeysName, Keys), false, false, true));
        FastSetProperty(
            ValuesName,
            new(new ClrFunction(Engine, ValuesName, Values), false, false, true)
        );
        FastSetProperty(
            ForEachName,
            new(new ClrFunction(Engine, ForEachName, ForEach), false, false, true)
        );
        FastSetProperty(
            GlobalSymbolRegistry.Iterator,
            new(
                new ClrFunction(Engine, GlobalSymbolRegistry.Iterator.ToString(), Entries),
                false,
                false,
                true
            )
        );
    }

    private JsValue Append(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 2, AppendName, nameof(FormData));

        var instance = thisObject.EnsureThisObject<FormDataInstance>();
        var name = arguments[0].ToString();

        if (arguments[1] is BlobInstance blobValue)
        {
            var fileName = arguments.Length >= 3 ? arguments[2].ToString() : null;
            instance.Append(name, blobValue, fileName);
        }
        else
        {
            var value = arguments[1].ToString();
            instance.Append(name, value);
        }

        return Undefined;
    }

    private JsValue Delete(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, DeleteName, nameof(FormData));

        var instance = thisObject.EnsureThisObject<FormDataInstance>();
        var name = arguments[0].ToString();
        instance.Delete(name);
        return Undefined;
    }

    private JsValue Get(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, GetName, nameof(FormData));

        var instance = thisObject.EnsureThisObject<FormDataInstance>();
        var name = arguments[0].ToString();
        var result = instance.Get(name);

        return result.HasValue ? result.Value.Value : Null;
    }

    private JsValue GetAll(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, GetAllName, nameof(FormData));

        var instance = thisObject.EnsureThisObject<FormDataInstance>();
        var name = arguments[0].ToString();
        var array = Engine.Intrinsics.Array.Construct(Arguments.Empty);

        foreach (var (_, value, _) in instance.GetAll(name))
        {
            array.Push(value);
        }

        return array;
    }

    private JsValue Has(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, HasName, nameof(FormData));

        var instance = thisObject.EnsureThisObject<FormDataInstance>();
        var name = arguments[0].ToString();
        return instance.Has(name);
    }

    private JsValue Set(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 2, SetName, nameof(FormData));

        var instance = thisObject.EnsureThisObject<FormDataInstance>();
        var name = arguments[0].ToString();

        if (arguments[1] is BlobInstance blobValue)
        {
            var fileName = arguments.Length >= 3 ? arguments[2].ToString() : null;
            instance.Set(name, blobValue, fileName);
        }
        else
        {
            var value = arguments[1].ToString();
            instance.Set(name, value);
        }

        return Undefined;
    }

    private ObjectInstance Entries(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<FormDataInstance>();
        return new FormDataIterator(Engine, instance, IteratorType.KeyAndValue);
    }

    private ObjectInstance Keys(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<FormDataInstance>();
        return new FormDataIterator(Engine, instance, IteratorType.Key);
    }

    private ObjectInstance Values(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<FormDataInstance>();
        return new FormDataIterator(Engine, instance, IteratorType.Value);
    }

    private JsValue ForEach(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, ForEachName, nameof(FormData));
        var instance = thisObject.EnsureThisObject<FormDataInstance>();

        var callback = arguments.At(0).AsFunctionInstance();
        var thisArg = arguments.At(1);

        foreach (var (name, value, _) in instance.EntryList.ToArray())
        {
            Engine.Call(callback, thisArg, [value, name, instance]);
        }

        return Undefined;
    }
}
