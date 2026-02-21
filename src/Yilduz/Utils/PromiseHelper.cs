using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Promise;
using Jint.Runtime;
using Jint.Runtime.Interop;

namespace Yilduz.Utils;

internal static class PromiseHelper
{
    public static ManualPromise CreateResolvedPromise(Engine engine, JsValue value)
    {
        var manualPromise = engine.Advanced.RegisterPromise();
        manualPromise.Resolve(value);
        return manualPromise;
    }

    public static ManualPromise CreateRejectedPromise(Engine engine, JsValue reason)
    {
        var manualPromise = engine.Advanced.RegisterPromise();
        manualPromise.Reject(reason);
        return manualPromise;
    }

    public static JsValue All(Engine engine, IEnumerable<JsValue> promises)
    {
        var manualPromise = engine.Advanced.RegisterPromise();
        var values = promises.ToArray();

        if (values.Length == 0)
        {
            manualPromise.Resolve(engine.Intrinsics.Array.Construct(Array.Empty<JsValue>()));
            return manualPromise.Promise;
        }

        var remaining = values.Length;
        var results = new JsValue[remaining];
        var rejected = false;

        for (var i = 0; i < values.Length; i++)
        {
            var index = i;
            values[i]
                .Then(
                    onFulfilled: v =>
                    {
                        if (rejected)
                        {
                            return v;
                        }

                        results[index] = v;
                        remaining--;

                        if (remaining == 0)
                        {
                            manualPromise.Resolve(engine.Intrinsics.Array.Construct(results));
                        }

                        return v;
                    },
                    onRejected: reason =>
                    {
                        if (rejected)
                        {
                            return reason;
                        }

                        rejected = true;
                        manualPromise.Reject(reason);
                        return reason;
                    }
                );
        }

        return manualPromise.Promise;
    }

    public static bool IsPendingPromise(this JsValue value)
    {
        if (!value.IsPromise())
        {
            return false;
        }

        try
        {
            value.UnwrapIfPromise(new CancellationToken(true));
            return false;
        }
        catch (OperationCanceledException)
        {
            return true;
        }
        catch (PromiseRejectedException)
        {
            return false;
        }
    }

    public static bool TryGetRejectedValue(
        this JsValue value,
        [NotNullWhen(true)] out JsValue? rejectValue
    )
    {
        rejectValue = null;

        if (!value.IsPromise())
        {
            return false;
        }

        try
        {
            value.UnwrapIfPromise(new CancellationToken(true)); // Will not wait the promise
        }
        catch (PromiseRejectedException e)
        {
            rejectValue = e.RejectedValue;
            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }

        return false;
    }

    public static JsValue Then(
        this JsValue jsValue,
        Func<JsValue, JsValue>? onFulfilled = null,
        Func<JsValue, JsValue>? onRejected = null,
        CancellationToken _ = default
    )
    {
        if (!jsValue.IsPromise())
        {
            return onFulfilled is null ? jsValue : onFulfilled(jsValue);
        }

        var engine = jsValue.AsObject().Engine;

        var fulfilledCallback = onFulfilled is null
            ? JsValue.Undefined
            : new ClrFunction(
                engine,
                "",
                (_, args) => onFulfilled(args.Length > 0 ? args[0] : JsValue.Undefined)
            );

        var rejectedCallback = onRejected is null
            ? JsValue.Undefined
            : new ClrFunction(
                engine,
                "",
                (_, args) => onRejected(args.Length > 0 ? args[0] : JsValue.Undefined)
            );

        var thenProperty = jsValue.AsObject().Get("then");
        var thenFunction = thenProperty.AsObject() as Function;

        if (thenFunction is null)
        {
            TypeErrorHelper.Throw(engine, "Promise.then is not callable");
        }

        return thenFunction.Call(jsValue, [fulfilledCallback, rejectedCallback]);
    }
}
