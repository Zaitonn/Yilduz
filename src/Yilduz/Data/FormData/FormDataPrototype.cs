using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Data.Blob;
using Yilduz.Iterator;
using Yilduz.Models;

namespace Yilduz.Data.FormData;

internal sealed class FormDataPrototype : PrototypeBase<FormDataInstance>
{
    public FormDataPrototype(Engine engine, FormDataConstructor constructor)
        : base(engine, nameof(FormData), constructor)
    {
        RegisterMethod("append", Append, 2);
        RegisterMethod("delete", Delete, 1);
        RegisterMethod("get", Get, 1);
        RegisterMethod("getAll", GetAll, 1);
        RegisterMethod("has", Has, 1);
        RegisterMethod("set", Set, 2);
        RegisterMethod("entries", Entries);
        RegisterMethod("keys", Keys);
        RegisterMethod("values", Values);
        RegisterMethod("forEach", ForEach, 1);

        RegisterIterator(Entries);
    }

    private static JsValue Append(FormDataInstance formData, JsValue[] arguments)
    {
        var name = arguments[0].ToString();

        if (arguments[1] is BlobInstance blobValue)
        {
            var fileName = arguments.Length >= 3 ? arguments[2].ToString() : null;
            formData.Append(name, blobValue, fileName);
        }
        else
        {
            formData.Append(name, arguments[1].ToString());
        }

        return Undefined;
    }

    private static JsValue Delete(FormDataInstance formData, JsValue[] arguments)
    {
        formData.Delete(arguments[0].ToString());
        return Undefined;
    }

    private static JsValue Get(FormDataInstance formData, JsValue[] arguments)
    {
        var result = formData.Get(arguments[0].ToString());
        return result?.Value ?? Null;
    }

    private JsValue GetAll(FormDataInstance formData, JsValue[] arguments)
    {
        var array = Engine.Intrinsics.Array.Construct(Arguments.Empty);

        foreach (var (_, value, _) in formData.GetAll(arguments[0].ToString()))
        {
            array.Push(value);
        }

        return array;
    }

    private static JsValue Has(FormDataInstance formData, JsValue[] arguments)
    {
        return formData.Has(arguments[0].ToString());
    }

    private static JsValue Set(FormDataInstance formData, JsValue[] arguments)
    {
        var name = arguments[0].ToString();

        if (arguments[1] is BlobInstance blobValue)
        {
            var fileName = arguments.Length >= 3 ? arguments[2].ToString() : null;
            formData.Set(name, blobValue, fileName);
        }
        else
        {
            formData.Set(name, arguments[1].ToString());
        }

        return Undefined;
    }

    private FormDataIterator Entries(FormDataInstance formData, JsValue[] arguments)
    {
        return new(Engine, formData, IteratorType.KeyAndValue);
    }

    private FormDataIterator Keys(FormDataInstance formData, JsValue[] arguments)
    {
        return new(Engine, formData, IteratorType.Key);
    }

    private FormDataIterator Values(FormDataInstance formData, JsValue[] arguments)
    {
        return new(Engine, formData, IteratorType.Value);
    }

    private JsValue ForEach(FormDataInstance formData, JsValue[] arguments)
    {
        var callback = arguments.At(0).AsFunctionInstance();
        var thisArg = arguments.At(1);

        foreach (var (name, value, _) in formData.EntryList.ToArray())
        {
            Engine.Call(callback, thisArg, [value, name, formData]);
        }

        return Undefined;
    }
}
