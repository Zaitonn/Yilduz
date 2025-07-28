using System.Collections.Generic;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;

namespace Yilduz.Utils;

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
}
