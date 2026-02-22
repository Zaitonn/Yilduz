using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Extensions;
using Yilduz.Iterator;
using Yilduz.Utils;

namespace Yilduz.Network.Headers;

internal sealed class HeadersPrototype : ObjectInstance
{
    private static readonly string AppendName = nameof(Append).ToJsStyleName();
    private static readonly string DeleteName = nameof(Delete).ToJsStyleName();
    private static readonly string GetName = nameof(Get).ToJsStyleName();
    private static readonly string HasName = nameof(Has).ToJsStyleName();
    private static readonly string SetName = nameof(Set).ToJsStyleName();
    private static readonly string EntriesName = nameof(Entries).ToJsStyleName();
    private static readonly string KeysName = nameof(Keys).ToJsStyleName();
    private static readonly string ValuesName = nameof(Values).ToJsStyleName();
    private static readonly string ForEachName = nameof(ForEach).ToJsStyleName();
    private static readonly string GetSetCookieName = nameof(GetSetCookie).ToJsStyleName();

    public HeadersPrototype(Engine engine, HeadersConstructor constructor)
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(Headers));
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
        FastSetProperty(HasName, new(new ClrFunction(Engine, HasName, Has), false, false, true));
        FastSetProperty(SetName, new(new ClrFunction(Engine, SetName, Set), false, false, true));
        FastSetProperty(
            GetSetCookieName,
            new(new ClrFunction(Engine, GetSetCookieName, GetSetCookie), false, false, true)
        );
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
        arguments.EnsureCount(Engine, 2, AppendName, nameof(Headers));

        var instance = thisObject.EnsureThisObject<HeadersInstance>();
        var name = arguments[0].ToString();
        var value = arguments[1].ToString();

        instance.Append(name, value);
        return Undefined;
    }

    private JsValue Delete(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, DeleteName, nameof(Headers));

        var instance = thisObject.EnsureThisObject<HeadersInstance>();
        var name = arguments[0].ToString();
        instance.Delete(name);
        return Undefined;
    }

    private JsValue Get(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, GetName, nameof(Headers));

        var instance = thisObject.EnsureThisObject<HeadersInstance>();
        var name = arguments[0].ToString();
        var result = instance.Get(name);
        return result ?? Null;
    }

    private JsValue GetSetCookie(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<HeadersInstance>();
        var result = instance.GetSetCookie();
        return result is null
            ? Null
            : Engine.Intrinsics.Array.Construct([.. result.Select<string, JsValue>(r => r)]);
    }

    private JsValue Has(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, HasName, nameof(Headers));

        var instance = thisObject.EnsureThisObject<HeadersInstance>();
        var name = arguments[0].ToString();
        return instance.Has(name);
    }

    private JsValue Set(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 2, SetName, nameof(Headers));

        var instance = thisObject.EnsureThisObject<HeadersInstance>();
        var name = arguments[0].ToString();
        var value = arguments[1].ToString();
        instance.Set(name, value);
        return Undefined;
    }

    private HeadersIterator Entries(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<HeadersInstance>();
        return new HeadersIterator(Engine, instance, IteratorType.KeyAndValue);
    }

    private HeadersIterator Keys(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<HeadersInstance>();
        return new HeadersIterator(Engine, instance, IteratorType.Key);
    }

    private HeadersIterator Values(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<HeadersInstance>();
        return new HeadersIterator(Engine, instance, IteratorType.Value);
    }

    private JsValue ForEach(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, ForEachName, nameof(Headers));

        var instance = thisObject.EnsureThisObject<HeadersInstance>();

        if (arguments.At(0) is not Function callback)
        {
            TypeErrorHelper.Throw(
                Engine,
                "parameter 1 is not of type 'Function'.",
                ForEachName,
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
