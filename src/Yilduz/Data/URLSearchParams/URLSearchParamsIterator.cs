using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace Yilduz.Data.URLSearchParams;

internal sealed class URLSearchParamsIterator : ObjectInstance
{
    private readonly URLSearchParamsInstance _searchParams;
    private readonly URLSearchParamsIteratorType _kind;
    private int _position;

    public URLSearchParamsIterator(
        Engine engine,
        URLSearchParamsInstance searchParams,
        URLSearchParamsIteratorType kind
    )
        : base(engine)
    {
        _searchParams = searchParams;
        _kind = kind;
        _position = 0;

        FastSetProperty(
            "next",
            new(
                new ClrFunction(engine, "next", Next, 0, PropertyFlag.Configurable),
                true,
                false,
                true
            )
        );

        FastSetProperty(
            GlobalSymbolRegistry.Iterator,
            new(new ClrFunction(engine, "[Symbol.iterator]", ToIterator), true, false, true)
        );

        FastSetProperty(
            GlobalSymbolRegistry.ToStringTag,
            new("URLSearchParams Iterator", true, false, true)
        );
    }

    private JsValue ToIterator(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject;
    }

    public JsValue Next(JsValue thisObject, JsValue[] arguments)
    {
        var list = _searchParams.QueryList;

        if (_position < list.Count)
        {
            var pair = list[_position];
            _position++;

            return _kind switch
            {
                URLSearchParamsIteratorType.Key => CreateIteratorResult(pair.Key, false),
                URLSearchParamsIteratorType.Value => CreateIteratorResult(pair.Value, false),
                _ => CreateEntryIteratorResult(pair.Key, pair.Value),
            };
        }

        return CreateIteratorResult(Undefined, true);
    }

    private JsValue CreateIteratorResult(JsValue value, bool done)
    {
        var result = Engine.Intrinsics.Object.Construct(Arguments.Empty);
        result.FastSetProperty("value", new(value, true, true, true));
        result.FastSetProperty("done", new(done, true, true, true));
        return result;
    }

    private JsValue CreateEntryIteratorResult(string key, string value)
    {
        var array = Engine.Intrinsics.Array.Construct(Arguments.Empty);
        array.Push(key);
        array.Push(value);

        return CreateIteratorResult(array, false);
    }
}
