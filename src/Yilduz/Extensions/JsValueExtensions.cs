using System;
using System.Collections.Generic;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Yilduz.Utils;

namespace Yilduz.Extensions;

internal static class JsValueExtensions
{
    public static T EnsureThisObject<T>(this JsValue thisObject)
        where T : ObjectInstance
    {
        var obj = thisObject.AsObject();

        if (obj is T t)
        {
            return t;
        }

        TypeErrorHelper.Throw(obj.Engine, "Illegal invocation: this is not of the expected type");
        return null;
    }

    public static bool ConvertToBoolean(this JsValue value)
    {
        return value switch
        {
            JsBoolean jsBoolean => jsBoolean.AsBoolean(),
            JsNumber jsNumber => jsNumber != 0,
            JsString jsString => !string.IsNullOrEmpty(jsString.ToString()),
            _ => !value.IsNull() && !value.IsUndefined(),
        };
    }

    public static IEnumerator<JsValue> GetEnumerator(this ObjectInstance objectInstance)
    {
        var iterator = objectInstance.Get(GlobalSymbolRegistry.Iterator);
        if (iterator is Function iteratorFunction)
        {
            var iteratorObject = objectInstance
                .Engine.Call(iteratorFunction, objectInstance, Arguments.Empty)
                .AsObject();

            while (true)
            {
                var nextMethod = iteratorObject.Get("next").AsFunctionInstance();

                var result = objectInstance
                    .Engine.Call(nextMethod, iteratorObject, Arguments.Empty)
                    .AsObject();

                var done = result["done"].AsBoolean();
                if (done)
                {
                    break;
                }

                yield return result["value"];
            }
        }
        else
        {
            TypeErrorHelper.Throw(
                objectInstance.Engine,
                $"{objectInstance} is not iterable (cannot read property Symbol(Symbol.iterator))"
            );
        }
    }

    public static byte[]? TryAsBytes(this JsValue input)
    {
        if (input.IsArrayBuffer())
        {
            return input.AsArrayBuffer();
        }

        if (input.IsDataView())
        {
            return input.AsDataView();
        }

        if (input is JsTypedArray typedArray)
        {
            var buffer = typedArray.Get("buffer");
            var arrayBuffer = buffer.AsArrayBuffer()!;
            var byteOffset = (int)typedArray.Get("byteOffset").AsNumber();
            var byteLength = (int)typedArray.Get("byteLength").AsNumber();

            return [.. arrayBuffer.Skip(byteOffset).Take(byteLength)];
        }

        if (!input.IsArray())
        {
            return null;
        }

        var array = input.AsArray();
        var bytes = (
            from element in array
            where element.IsNumber()
            select (byte)((int)element.AsNumber() & 0xFF)
        ).ToList();

        return [.. bytes];
    }

    /// <summary>
    /// Performs a structured clone of the given value
    /// https://html.spec.whatwg.org/multipage/structured-data.html#structured-clone
    /// </summary>
    public static JsValue StructuredClone(this JsValue value)
    {
        if (
            value.IsUndefined()
            || value.IsNull()
            || value.IsBoolean()
            || value.IsNumber()
            || value.IsString()
        )
        {
            return value;
        }

        if (!value.IsObject())
        {
            throw new NotSupportedException("Structured clone of this type is not supported");
        }

        var obj = value.AsObject();

        // Handle arrays
        if (obj.IsArray())
        {
            var array = obj.AsArray();
            var clonedArray = array.Engine.Intrinsics.Array.Construct(Arguments.Empty);

            for (uint i = 0; i < array.Length; i++)
            {
                if (array.HasProperty(i))
                {
                    var element = array.Get(i);
                    clonedArray.Set(i, element.StructuredClone());
                }
            }

            return clonedArray;
        }

        // Handle plain objects
        var clonedObj = obj.Engine.Intrinsics.Object.Construct(Arguments.Empty);
        var properties = obj.GetOwnProperties();

        foreach (var prop in properties)
        {
            if (prop.Value.Enumerable == true)
            {
                var propValue = obj.Get(prop.Key);
                clonedObj.Set(prop.Key, propValue.StructuredClone());
            }
        }

        return clonedObj;
    }

    public static ulong GetByteLength(this JsValue typedArray)
    {
        return (ulong)typedArray.Get("byteLength").AsNumber();
    }

    public static ulong GetByteOffset(this JsValue typedArray)
    {
        return (ulong)typedArray.Get("byteOffset").AsNumber();
    }
}
