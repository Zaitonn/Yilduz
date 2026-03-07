using Jint;
using Jint.Native;
using Yilduz.Iterator;

namespace Yilduz.Data.FormData;

internal sealed class FormDataIterator(Engine engine, FormDataInstance formData, IteratorType kind)
    : BaseIterator(engine, kind, "FormData Iterator")
{
    protected override bool TryGetEntry(int index, out JsValue key, out JsValue value)
    {
        var list = formData.EntryList;

        if (index < list.Count)
        {
            (key, value, _) = list[index];
            return true;
        }

        key = Undefined;
        value = Undefined;
        return false;
    }
}
