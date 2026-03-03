using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime;
using Yilduz.Iterator;
using Yilduz.Models;
using Yilduz.Utils;

namespace Yilduz.URLs.URLSearchParams;

internal sealed class URLSearchParamsPrototype : PrototypeBase<URLSearchParamsInstance>
{
    public URLSearchParamsPrototype(Engine engine, URLSearchParamsConstructor constructor)
        : base(engine, nameof(URLSearchParams), constructor)
    {
        RegisterProperty("size", searchParams => searchParams.Size);

        RegisterMethod("append", Append, 2);
        RegisterMethod("delete", Delete, 1);
        RegisterMethod("get", Get, 1);
        RegisterMethod("getAll", GetAll, 1);
        RegisterMethod("has", Has, 1);
        RegisterMethod("set", Set, 2);
        RegisterMethod("sort", Sort);
        RegisterMethod("entries", Entries);
        RegisterMethod("keys", Keys);
        RegisterMethod("values", Values);
        RegisterMethod("forEach", ForEach, 1);
        RegisterMethod("toString", ToString);
        RegisterMethod("toJSON", ToJSON);

        RegisterIterator(Entries);
    }

    private static JsValue Append(URLSearchParamsInstance instance, JsValue[] args)
    {
        instance.Append(args[0].ToString(), args[1].ToString());
        return Undefined;
    }

    private static JsValue Delete(URLSearchParamsInstance instance, JsValue[] args)
    {
        instance.Delete(args[0].ToString());
        return Undefined;
    }

    private static JsValue Get(URLSearchParamsInstance instance, JsValue[] args)
    {
        var result = instance.Get(args[0].ToString());
        return result ?? Null;
    }

    private JsValue GetAll(URLSearchParamsInstance instance, JsValue[] args)
    {
        var result = instance.GetAll(args[0].ToString());
        return FromObject(Engine, result);
    }

    private static JsValue Has(URLSearchParamsInstance instance, JsValue[] args)
    {
        var name = args[0].ToString();
        return args.Length >= 2 ? instance.Has(name, args[1].ToString()) : instance.Has(name);
    }

    private static JsValue Set(URLSearchParamsInstance instance, JsValue[] args)
    {
        instance.Set(args[0].ToString(), args[1].ToString());
        return Undefined;
    }

    private static JsValue Sort(URLSearchParamsInstance instance, JsValue[] _)
    {
        instance.Sort();
        return Undefined;
    }

    private URLSearchParamsIterator Entries(URLSearchParamsInstance instance, JsValue[] _)
    {
        return new(Engine, instance, IteratorType.KeyAndValue);
    }

    private URLSearchParamsIterator Keys(URLSearchParamsInstance instance, JsValue[] _)
    {
        return new(Engine, instance, IteratorType.Key);
    }

    private URLSearchParamsIterator Values(URLSearchParamsInstance instance, JsValue[] _)
    {
        return new(Engine, instance, IteratorType.Value);
    }

    private JsValue ForEach(URLSearchParamsInstance instance, JsValue[] args)
    {
        if (args.At(0) is not Function callback)
        {
            TypeErrorHelper.Throw(
                Engine,
                "parameter 1 is not of type 'Function'.",
                "forEach",
                nameof(URLSearchParams)
            );
            return Undefined;
        }

        foreach (var pair in instance.QueryList.ToArray())
        {
            Engine.Call(callback, args.At(1), [pair.Value, pair.Key, instance]);
        }

        return Undefined;
    }

    private static JsValue ToString(URLSearchParamsInstance instance, JsValue[] _)
    {
        return instance.ToString();
    }

    private static JsValue ToJSON(URLSearchParamsInstance instance, JsValue[] _)
    {
        return instance.ToString();
    }
}
