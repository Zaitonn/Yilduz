using System.Collections.Generic;
using Jint;
using Jint.Native;
using Yilduz.Iterator;

namespace Yilduz.Network.Headers;

internal sealed class HeadersIterator(Engine engine, HeadersInstance headers, IteratorType kind)
    : BaseIterator(engine, kind, "Headers Iterator")
{
    private readonly List<(string Name, string Value)> _entries =
        headers.GetSortedAndCombinedEntries();

    protected override bool TryGetEntry(int index, out JsValue key, out JsValue value)
    {
        if (index < _entries.Count)
        {
            var entry = _entries[index];
            key = entry.Name;
            value = entry.Value;
            return true;
        }

        key = Undefined;
        value = Undefined;
        return false;
    }
}
