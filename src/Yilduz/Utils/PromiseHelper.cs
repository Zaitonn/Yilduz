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
                    [.. promises.Select((p, i) => Task.Run(() => result[i] = p.UnwrapIfPromise()))]
                );

                lock (engine)
                {
                    manualPromise.Resolve(engine.Intrinsics.Array.Construct(result));
                }
            }
            catch (Exception e)
            {
                lock (engine)
                {
                    manualPromise.Reject(
                        e switch
                        {
                            AggregateException ae => ae.InnerException switch
                            {
                                PromiseRejectedException pre => pre.RejectedValue,
                                JavaScriptException jse => jse.Error,
                                _ => e.Message,
                            },
                            _ => e.Message,
                        }
                    );
                }
            }
        });

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
        CancellationToken cancellationToken = default
    )
    {
        if (!jsValue.IsPromise())
        {
            return onFulfilled is null ? jsValue : onFulfilled(jsValue);
        }

        var engine = jsValue.AsObject().Engine;
        var manualPromise = engine.Advanced.RegisterPromise();

        Task.Run(
            () =>
            {
                JsValue? result = null;
                Exception? error = null;
                try
                {
                    result = jsValue.UnwrapIfPromise(cancellationToken);
                }
                catch (Exception e)
                {
                    error = e;
                }

                lock (engine)
                {
                    try
                    {
                        switch (error)
                        {
                            case PromiseRejectedException pre:
                                if (onRejected is null)
                                {
                                    manualPromise.Reject(pre.RejectedValue);
                                }
                                else
                                {
                                    manualPromise.Resolve(onRejected(pre.RejectedValue));
                                }
                                break;

                            case JavaScriptException jse:
                                if (onRejected is null)
                                {
                                    manualPromise.Reject(jse.Error);
                                }
                                else
                                {
                                    manualPromise.Resolve(onRejected(jse.Error));
                                }
                                break;

                            case null:
                                if (onFulfilled is null)
                                {
                                    manualPromise.Resolve(result!);
                                }
                                else
                                {
                                    manualPromise.Resolve(onFulfilled(result!));
                                }
                                break;

                            default:
                                manualPromise.Reject(error.Message);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        manualPromise.Reject(JsValue.FromObject(engine, ex.Message));
                    }
                }
            },
            cancellationToken
        );

        return manualPromise.Promise;
    }
}
