using System;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;

namespace Yilduz.Models;

public abstract class PrototypeBase<T> : ObjectInstance
    where T : ObjectInstance
{
    private readonly string _name;

    private protected PrototypeBase(Engine engine, string name, Constructor constructor)
        : base(engine)
    {
        _name = name;

        Set(GlobalSymbolRegistry.ToStringTag, _name);
        SetOwnProperty("constructor", new(constructor, false, false, true));
    }

    private protected void RegisterMethod(
        string name,
        Func<T, JsValue[], JsValue> method,
        int minimumArguments = 0
    )
    {
        FastSetProperty(
            name,
            new(new ClrFunction(Engine, name, WrapMethod(method)), false, false, true)
        );

        Func<JsValue, JsValue[], JsValue> WrapMethod(Func<T, JsValue[], JsValue> method)
        {
            return (thisObject, arguments) =>
            {
                var t = thisObject.EnsureThisObject<T>();

                if (arguments.Length < minimumArguments)
                {
                    arguments.EnsureCount(
                        Engine,
                        minimumArguments,
                        $"Failed to execute '{name}' on '{_name}'"
                    );
                }

                return method(t, arguments);
            };
        }
    }

    private protected void RegisterProperty(
        string name,
        Func<T, JsValue>? getter = null,
        Func<T, JsValue, JsValue>? setter = null
    )
    {
        FastSetProperty(
            name,
            new GetSetPropertyDescriptor(
                get: getter != null
                    ? new ClrFunction(Engine, "get " + name, WrapGetter(getter))
                    : null,
                set: setter != null
                    ? new ClrFunction(Engine, "set " + name, WrapSetter(setter))
                    : null,
                true,
                true
            )
        );

        Func<JsValue, JsValue[], JsValue> WrapGetter(Func<T, JsValue> getter)
        {
            return (thisObject, arguments) => getter(thisObject.EnsureThisObject<T>());
        }

        Func<JsValue, JsValue[], JsValue> WrapSetter(Func<T, JsValue, JsValue> setter)
        {
            return (thisObject, arguments) =>
                setter(thisObject.EnsureThisObject<T>(), arguments.At(0));
        }
    }

    private protected void RegisterConstant<TEnum>(string name, TEnum value)
        where TEnum : struct, Enum
    {
        FastSetProperty(name, new(Convert.ToInt32(value), false, false, true));
    }

    private protected void RegisterIterator(Func<T, JsValue[], JsValue> iteratorMethod)
    {
        FastSetProperty(
            GlobalSymbolRegistry.Iterator,
            new(
                new ClrFunction(
                    Engine,
                    GlobalSymbolRegistry.Iterator.ToString(),
                    (thisObject, args) => iteratorMethod(thisObject.EnsureThisObject<T>(), args)
                ),
                false,
                false,
                true
            )
        );
    }
}
