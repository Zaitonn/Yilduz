using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Runtime;
using Yilduz.Iterator;
using Yilduz.Models;
using Yilduz.Utils;

namespace Yilduz.Network.Headers;

internal sealed class HeadersPrototype : PrototypeBase<HeadersInstance>
{
    public HeadersPrototype(Engine engine, HeadersConstructor constructor)
        : base(engine, nameof(Headers), constructor)
    {
        RegisterMethod("append", Append, 2);
        RegisterMethod("delete", Delete, 1);
        RegisterMethod("get", Get, 1);
        RegisterMethod("getSetCookie", GetSetCookie);
        RegisterMethod("has", Has, 1);
        RegisterMethod("set", Set, 2);
        RegisterMethod("entries", Entries);
        RegisterMethod("keys", Keys);
        RegisterMethod("values", Values);
        RegisterMethod("forEach", ForEach, 1);

        RegisterIterator(Entries);
    }

    private JsValue Append(HeadersInstance instance, JsValue[] arguments)
    {
        var name = arguments[0].ToString();
        var value = arguments[1].ToString();

        instance.Append(name, value);
        return Undefined;
    }

    private JsValue Delete(HeadersInstance instance, JsValue[] arguments)
    {
        var name = arguments[0].ToString();
        instance.Delete(name);
        return Undefined;
    }

    private JsValue Get(HeadersInstance instance, JsValue[] arguments)
    {
        var name = arguments[0].ToString();
        var result = instance.Get(name);
        return result ?? Null;
    }

    private JsValue GetSetCookie(HeadersInstance instance, JsValue[] arguments)
    {
        var result = instance.GetSetCookie();
        return result is null
            ? Null
            : Engine.Intrinsics.Array.Construct([.. result.Select<string, JsValue>(r => r)]);
    }

    private JsValue Has(HeadersInstance instance, JsValue[] arguments)
    {
        var name = arguments[0].ToString();
        return instance.Has(name);
    }

    private JsValue Set(HeadersInstance instance, JsValue[] arguments)
    {
        var name = arguments[0].ToString();
        var value = arguments[1].ToString();
        instance.Set(name, value);
        return Undefined;
    }

    private HeadersIterator Entries(HeadersInstance instance, JsValue[] arguments)
    {
        return new(Engine, instance, IteratorType.KeyAndValue);
    }

    private HeadersIterator Keys(HeadersInstance instance, JsValue[] arguments)
    {
        return new(Engine, instance, IteratorType.Key);
    }

    private HeadersIterator Values(HeadersInstance instance, JsValue[] arguments)
    {
        return new(Engine, instance, IteratorType.Value);
    }

    private JsValue ForEach(HeadersInstance instance, JsValue[] arguments)
    {
        if (arguments.At(0) is not Function callback)
        {
            TypeErrorHelper.Throw(
                Engine,
                "parameter 1 is not of type 'Function'.",
                "forEach",
                nameof(Headers)
            );
            return Undefined;
        }

        var thisArg = arguments.At(1);

        foreach (var (name, value) in instance.GetSortedAndCombinedEntries())
        {
            Engine.Call(callback, thisArg, [value, name, instance]);
        }

        return Undefined;
    }
}
