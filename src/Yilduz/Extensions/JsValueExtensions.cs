using System;
using System.Collections.Generic;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;

namespace Yilduz.Extensions;

internal static class JsValueExtensions
{
    public static T EnsureThisObject<T>(this JsValue thisObject)
        where T : ObjectInstance
    {
        var obj = thisObject.AsObject();

        return thisObject.As<T>()
            ?? throw new JavaScriptException(obj.Engine.Intrinsics.TypeError, "Illegal invocation");
    }

    public static bool ToBoolean(this JsValue value)
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
                var nextMethod =
                    iteratorObject.Get("next") as Function
                    ?? throw new JavaScriptException(
                        iteratorObject.Engine.Intrinsics.TypeError,
                        "Iterator next must be a function"
                    );

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
            throw new JavaScriptException(
                objectInstance.Engine.Intrinsics.TypeError,
                $"{objectInstance} is not iterable (cannot read property Symbol(Symbol.iterator))"
            );
        }
    }

    public static byte[]? TryAsBytes(this JsValue input)
    {
        if (input.IsArrayBuffer())
        {
            return input.AsArrayBuffer()!;
        }

        if (input.IsDataView())
        {
            return input.AsDataView()!;
        }

        if (input is JsTypedArray typedArray)
        {
            var buffer = typedArray.Get("buffer");
            var arrayBuffer = buffer.AsArrayBuffer()!;
            var byteOffset = (int)typedArray.Get("byteOffset").AsNumber();
            var byteLength = (int)typedArray.Get("byteLength").AsNumber();

            return [.. arrayBuffer.Skip(byteOffset).Take(byteLength)];
        }

        if (input.IsArray())
        {
            var array = input.AsArray();
            var bytes = new List<byte>();

            for (int i = 0; i < array.Length; i++)
            {
                var element = array[i];
                if (element.IsNumber())
                {
                    bytes.Add((byte)((int)element.AsNumber() & 0xFF));
                }
            }

            return [.. bytes];
        }

        return null;
    }

    /// <summary>
    /// Performs a structured clone of the given value
    /// https://html.spec.whatwg.org/multipage/structured-data.html#structured-clone
    /// </summary>
    public static JsValue StructuredClone(this JsValue value)
    {
        if (value.IsUndefined() || value.IsNull())
        {
            return value;
        }

        if (value.IsBoolean() || value.IsNumber() || value.IsString())
        {
            return value;
        }

        if (value.IsObject())
        {
            var obj = value.AsObject();

            // Handle arrays
            if (obj.IsArray())
            {
                var array = obj.AsArray();
                var clonedArray = array.Engine.Intrinsics.Array.Construct([]);

                for (uint i = 0; i < array.Length; i++)
                {
                    if (array.HasProperty(i))
                    {
                        var element = array.Get(i);
                        clonedArray.Set(i, StructuredClone(element));
                    }
                }

                return clonedArray;
            }

            // Handle plain objects
            var clonedObj = obj.Engine.Intrinsics.Object.Construct([]);
            var properties = obj.GetOwnProperties();

            foreach (var prop in properties)
            {
                if (prop.Value.Enumerable == true)
                {
                    var propValue = obj.Get(prop.Key);
                    clonedObj.Set(prop.Key, StructuredClone(propValue));
                }
            }

            return clonedObj;
        }

        throw new NotSupportedException("Structured clone of this type is not supported");
    }
}
