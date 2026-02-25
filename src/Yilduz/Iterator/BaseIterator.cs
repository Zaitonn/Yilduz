using System;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace Yilduz.Iterator;

internal abstract class BaseIterator : ObjectInstance
{
    private readonly IteratorType _kind;
    private int _position;

    protected BaseIterator(Engine engine, IteratorType kind, string toStringTag)
        : base(engine)
    {
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

        FastSetProperty(GlobalSymbolRegistry.ToStringTag, new(toStringTag, true, false, true));
    }

    private static JsValue ToIterator(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject;
    }

    private JsValue Next(JsValue thisObject, JsValue[] arguments)
    {
        if (TryGetEntry(_position, out var key, out var value))
        {
            _position++;

            return _kind switch
            {
                IteratorType.Key => CreateIteratorResult(key, false),
                IteratorType.Value => CreateIteratorResult(value, false),
                IteratorType.KeyAndValue => CreateEntryIteratorResult(key, value),
                _ => throw new InvalidOperationException("Invalid iterator type"),
            };
        }

        return CreateIteratorResult(Undefined, true);
    }

    protected abstract bool TryGetEntry(int index, out JsValue key, out JsValue value);

    protected virtual JsValue CreateEntryIteratorResult(JsValue key, JsValue value)
    {
        var array = Engine.Intrinsics.Array.Construct(Arguments.Empty);
        array.Push(key);
        array.Push(value);

        return CreateIteratorResult(array, false);
    }

    protected JsValue CreateIteratorResult(JsValue value, bool done)
    {
        var result = Engine.Intrinsics.Object.Construct(Arguments.Empty);
        result.FastSetProperty("value", new(value, true, true, true));
        result.FastSetProperty("done", new(done, true, true, true));
        return result;
    }
}
