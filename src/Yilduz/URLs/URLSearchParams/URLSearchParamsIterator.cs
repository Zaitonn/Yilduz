using Jint;
using Jint.Native;
using Yilduz.Iterator;

namespace Yilduz.URLs.URLSearchParams;

internal sealed class URLSearchParamsIterator(
    Engine engine,
    URLSearchParamsInstance searchParams,
    IteratorType kind
) : BaseIterator(engine, kind, "URLSearchParams Iterator")
{
    private readonly URLSearchParamsInstance _searchParams = searchParams;

    protected override bool TryGetEntry(int index, out JsValue key, out JsValue value)
    {
        var list = _searchParams.QueryList;

        if (index < list.Count)
        {
            var pair = list[index];
            key = pair.Key;
            value = pair.Value;
            return true;
        }

        key = Undefined;
        value = Undefined;
        return false;
    }
}
