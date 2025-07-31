using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace Yilduz.Network.FormData;

internal sealed class FormDataIterator : ObjectInstance
{
    private readonly FormDataInstance _formData;
    private readonly FormDataIteratorType _kind;
    private int _position;

    public FormDataIterator(Engine engine, FormDataInstance formData, FormDataIteratorType kind)
        : base(engine)
    {
        _formData = formData;
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
            new("FormData Iterator", true, false, true)
        );
    }

    private JsValue ToIterator(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject;
    }

    public JsValue Next(JsValue thisObject, JsValue[] arguments)
    {
        var list = _formData.EntryList;

        if (_position < list.Count)
        {
            var entry = list[_position];
            _position++;

            return _kind switch
            {
                FormDataIteratorType.Key => CreateIteratorResult(entry.Name, false),
                FormDataIteratorType.Value => CreateIteratorResult(entry.Value, false),
                _ => CreateEntryIteratorResult(entry.Name, entry.Value),
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

    private JsValue CreateEntryIteratorResult(string key, JsValue value)
    {
        var array = Engine.Intrinsics.Array.Construct(Arguments.Empty);
        array.Push(key);
        array.Push(value);

        return CreateIteratorResult(array, false);
    }
}
