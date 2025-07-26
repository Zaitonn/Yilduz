using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Utils;

namespace Yilduz.Data.URLSearchParams;

internal sealed class URLSearchParamsPrototype : ObjectInstance
{
    public URLSearchParamsPrototype(Engine engine, URLSearchParamsConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(URLSearchParams));
        SetOwnProperty("constructor", new(constructor, false, false, true));

        // Size property
        FastSetProperty(
            nameof(URLSearchParamsInstance.Size).ToJsStyleName(),
            new GetSetPropertyDescriptor(
                get: new ClrFunction(
                    engine,
                    nameof(URLSearchParamsInstance.Size).ToJsGetterName(),
                    GetSize
                ),
                set: null,
                false,
                true
            )
        );

        // Methods
        FastSetProperty(
            nameof(URLSearchParamsInstance.Append).ToJsStyleName(),
            new(
                new ClrFunction(
                    Engine,
                    nameof(URLSearchParamsInstance.Append).ToJsStyleName(),
                    Append
                ),
                false,
                false,
                true
            )
        );

        FastSetProperty(
            nameof(URLSearchParamsInstance.Delete).ToJsStyleName(),
            new(
                new ClrFunction(
                    Engine,
                    nameof(URLSearchParamsInstance.Delete).ToJsStyleName(),
                    Delete
                ),
                false,
                false,
                true
            )
        );

        FastSetProperty(
            nameof(URLSearchParamsInstance.Get).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(URLSearchParamsInstance.Get).ToJsStyleName(), Get),
                false,
                false,
                true
            )
        );

        FastSetProperty(
            nameof(URLSearchParamsInstance.GetAll).ToJsStyleName(),
            new(
                new ClrFunction(
                    Engine,
                    nameof(URLSearchParamsInstance.GetAll).ToJsStyleName(),
                    GetAll
                ),
                false,
                false,
                true
            )
        );

        FastSetProperty(
            nameof(URLSearchParamsInstance.Has).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(URLSearchParamsInstance.Has).ToJsStyleName(), Has),
                false,
                false,
                true
            )
        );

        FastSetProperty(
            nameof(URLSearchParamsInstance.Set).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(URLSearchParamsInstance.Set).ToJsStyleName(), Set),
                false,
                false,
                true
            )
        );

        FastSetProperty(
            nameof(URLSearchParamsInstance.Sort).ToJsStyleName(),
            new(
                new ClrFunction(Engine, nameof(URLSearchParamsInstance.Sort).ToJsStyleName(), Sort),
                false,
                false,
                true
            )
        );

        FastSetProperty(
            "toString",
            new(new ClrFunction(Engine, "toString", ToString), false, false, true)
        );
    }

    private JsValue GetSize(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<URLSearchParamsInstance>().Size;
    }

    private JsValue Append(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(2, Engine, "Failed to execute 'append' on 'URLSearchParams'");

        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        var name = arguments[0].ToString();
        var value = arguments[1].ToString();
        instance.Append(name, value);
        return Undefined;
    }

    private JsValue Delete(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(1, Engine, "delete", nameof(URLSearchParams));

        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        var name = arguments[0].ToString();
        instance.Delete(name);
        return Undefined;
    }

    private JsValue Get(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(1, Engine, "get", nameof(URLSearchParams));

        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        var name = arguments[0].ToString();
        var result = instance.Get(name);
        return result ?? Null;
    }

    private JsValue GetAll(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(1, Engine, "getAll", nameof(URLSearchParams));

        var instance = thisObject.EnsureThisObject<URLSearchParamsInstance>();
        var name = arguments[0].ToString();
        var result = instance.GetAll(name);
        return FromObject(Engine, result);
    }

    private JsValue Has(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(1, Engine, "has", nameof(URLSearchParams));

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
        arguments.EnsureCount(2, Engine, "set", nameof(URLSearchParams));

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
}
