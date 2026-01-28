using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Native.Promise;
using Jint.Runtime;

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
        var result = new JsValue[promises.Count()];

        Task.Run(() =>
        {
            try
            {
                Task.WaitAll(
                    [
                        .. promises.Select(
                            (p, i) =>
                                Task.Run(() =>
                                {
                                    result[i] = p.UnwrapIfPromise();
                                })
                        ),
                    ]
                );
                manualPromise.Resolve(engine.Intrinsics.Array.Construct(result));
            }
            catch (AggregateException e) when (e.InnerException is PromiseRejectedException ex)
            {
                manualPromise.Reject(ex.RejectedValue);
            }
        });

        return manualPromise.Promise;
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
        Func<JsValue, JsValue>? onRejected = null
    )
    {
        if (!jsValue.IsPromise())
        {
            return onFulfilled is null ? jsValue : onFulfilled(jsValue);
        }

        var engine = jsValue.AsObject().Engine;
        var manualPromise = engine.Advanced.RegisterPromise();

        Task.Run(() =>
        {
            try
            {
                var result = jsValue.UnwrapIfPromise();

                lock (engine)
                {
                    if (onFulfilled is null)
                    {
                        manualPromise.Resolve(result);
                    }
                    else
                    {
                        manualPromise.Resolve(onFulfilled(result));
                    }
                }
            }
            catch (PromiseRejectedException e)
            {
                lock (engine)
                {
                    if (onRejected is null)
                    {
                        manualPromise.Reject(e.RejectedValue);
                    }
                    else
                    {
                        manualPromise.Reject(onRejected(e.RejectedValue));
                    }
                }
            }
        });

        return manualPromise.Promise;
    }
}
