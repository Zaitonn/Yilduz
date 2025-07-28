using System;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Utils;

namespace Yilduz.Data.URLSearchParams;

internal sealed class URLSearchParamsPrototype : ObjectInstance
{
    private static readonly string SizePropertyName = nameof(URLSearchParamsInstance.Size)
        .ToJsStyleName();
    private static readonly string SizeGetterName = SizePropertyName.ToJsGetterName();
    private static readonly string AppendName = nameof(Append).ToJsStyleName();
    private static readonly string DeleteName = nameof(Delete).ToJsStyleName();
    private static readonly string GetName = nameof(Get).ToJsStyleName();
    private static readonly string GetAllName = nameof(GetAll).ToJsStyleName();
    private static readonly string HasName = nameof(Has).ToJsStyleName();
    private static readonly string SetName = nameof(Set).ToJsStyleName();
    private static readonly string SortName = nameof(Sort).ToJsStyleName();
    private static readonly string ToStringName = nameof(ToString).ToJsStyleName();
    private static readonly string ToJSONName = nameof(ToJSON).ToJsStyleName();
    private static readonly string ForEachName = nameof(ForEach).ToJsStyleName();
    private static readonly string EntriesName = nameof(Entries).ToJsStyleName();
    private static readonly string KeysName = nameof(Keys).ToJsStyleName();
    private static readonly string ValuesName = nameof(Values).ToJsStyleName();

    public URLSearchParamsPrototype(Engine engine, URLSearchParamsConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(URLSearchParams));
        SetOwnProperty("constructor", new(constructor, false, false, true));

        FastSetProperty(
            SizePropertyName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, SizeGetterName, GetSize),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            AppendName,
            new(new ClrFunction(Engine, AppendName, Append), false, false, true)
        );
        FastSetProperty(
            DeleteName,
            new(new ClrFunction(Engine, DeleteName, Delete), false, false, true)
        );
        FastSetProperty(
            GetAllName,
            new(new ClrFunction(Engine, GetAllName, GetAll), false, false, true)
        );
        FastSetProperty(GetName, new(new ClrFunction(Engine, GetName, Get), false, false, true));
        FastSetProperty(HasName, new(new ClrFunction(Engine, HasName, Has), false, false, true));
        FastSetProperty(SetName, new(new ClrFunction(Engine, SetName, Set), false, false, true));
        FastSetProperty(SortName, new(new ClrFunction(Engine, SortName, Sort), false, false, true));
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
            ToStringName,
            new(new ClrFunction(Engine, ToStringName, ToString), false, false, true)
        );
        FastSetProperty(
            ToJSONName,
            new(new ClrFunction(Engine, ToJSONName, ToJSON), false, false, true)
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

    private JsValue GetSize(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<URLSearchParamsInstance>().Size;
    }

    private JsValue Append(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 2, AppendName, nameof(URLSearchParams));

        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        var name = arguments[0].ToString();
        var value = arguments[1].ToString();
        instance.Append(name, value);
        return Undefined;
    }

    private JsValue Delete(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, DeleteName, nameof(URLSearchParams));

        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        var name = arguments[0].ToString();
        instance.Delete(name);
        return Undefined;
    }

    private JsValue Get(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, GetName, nameof(URLSearchParams));

        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        var name = arguments[0].ToString();
        var result = instance.Get(name);
        return result ?? Null;
    }

    private JsValue GetAll(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, GetAllName, nameof(URLSearchParams));

        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        var name = arguments[0].ToString();
        var result = instance.GetAll(name);
        return FromObject(Engine, result);
    }

    private JsValue Has(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, HasName, nameof(URLSearchParams));

        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        var name = arguments[0].ToString();

        if (arguments.Length >= 2)
        {
            var value = arguments[1].ToString();
            return instance.Has(name, value);
        }

        return instance.Has(name);
    }

    private JsValue Set(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 2, SetName, nameof(URLSearchParams));

        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        var name = arguments[0].ToString();
        var value = arguments[1].ToString();
        instance.Set(name, value);
        return Undefined;
    }

    private JsValue Sort(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        instance.Sort();
        return Undefined;
    }

    private JsValue ToString(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        return instance.ToString();
    }

    private JsValue ToJSON(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        return instance.ToString();
    }

    private JsValue ForEach(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, ForEachName, nameof(URLSearchParams));
        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();

        if (arguments.At(0) is not Function callback)
        {
            TypeErrorHelper.Throw(
                Engine,
                "parameter 1 is not of type 'Function'.",
                ForEachName,
                nameof(URLSearchParams)
            );
            return Undefined;
        }

        foreach (var pair in instance.QueryList.ToArray())
        {
            Engine.Call(callback, arguments.At(1), [pair.Value, pair.Key, instance]);
        }

        return Undefined;
    }

    private ObjectInstance Entries(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        return new URLSearchParamsIterator(
            Engine,
            instance,
            URLSearchParamsIteratorType.KeyAndValue
        );
    }

    private ObjectInstance Keys(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        return new URLSearchParamsIterator(Engine, instance, URLSearchParamsIteratorType.Key);
    }

    private ObjectInstance Values(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        return new URLSearchParamsIterator(Engine, instance, URLSearchParamsIteratorType.Value);
    }
}
