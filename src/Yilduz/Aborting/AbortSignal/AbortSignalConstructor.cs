using System;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Extensions;
using Yilduz.Utils;

namespace Yilduz.Aborting.AbortSignal;

public sealed class AbortSignalConstructor : Constructor
{
    private static readonly string AbortName = nameof(Abort).ToJsStyleName();
    private static readonly string TimeoutName = nameof(Timeout).ToJsStyleName();
    private static readonly string AnyName = nameof(Any).ToJsStyleName();

    internal AbortSignalConstructor(Engine engine, WebApiIntrinsics webApiIntrinsics)
        : base(engine, nameof(AbortSignal))
    {
        PrototypeObject = new AbortSignalPrototype(engine, this)
        {
            Prototype = webApiIntrinsics.EventTarget.PrototypeObject,
        };

        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
        SetOwnProperty(
            AbortName,
            new(new ClrFunction(engine, AbortName, Abort), true, false, true)
        );
        SetOwnProperty(
            TimeoutName,
            new(new ClrFunction(engine, TimeoutName, Timeout), true, false, true)
        );
        SetOwnProperty(AnyName, new(new ClrFunction(engine, AnyName, Any), true, false, true));
    }

    internal AbortSignalPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        TypeErrorHelper.Throw(Engine, "Failed to construct 'AbortSignal': Illegal constructor");
        return null!;
    }

    internal AbortSignalInstance ConstructAbortSignal()
    {
        return new AbortSignalInstance(Engine) { Prototype = PrototypeObject };
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal/abort_static
    /// </summary>
    public AbortSignalInstance Abort(JsValue reason)
    {
        var signal = ConstructAbortSignal();
        signal.SetAborted(reason);

        return signal;
    }

    private AbortSignalInstance Abort(JsValue thisObject, JsValue[] arguments)
    {
        return Abort(arguments.At(0));
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal/timeout_static
    /// </summary>
    public AbortSignalInstance Timeout(ulong time)
    {
        var signal = ConstructAbortSignal();

        Task.Delay(TimeSpan.FromMilliseconds(time))
            .ContinueWith(_ =>
                signal.SetAborted(ErrorHelper.Create(Engine, "TimeoutError", "signal timed out"))
            );

        return signal;
    }

    private AbortSignalInstance Timeout(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, TimeoutName, nameof(AbortSignal));

        return Timeout((ulong)arguments[0].AsNumber());
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal/any_static
    /// </summary>
    public AbortSignalInstance Any(JsArray signals)
    {
        var abortSignal = ConstructAbortSignal();
        foreach (var signal in signals)
        {
            if (signal is AbortSignalInstance instance)
            {
                if (instance.Aborted)
                {
                    abortSignal.SetAborted(instance.Reason);
                    break;
                }

                instance.Abort += (_, _) => abortSignal.SetAborted(instance.Reason);
            }
            else
            {
                TypeErrorHelper.Throw(
                    Engine,
                    "Failed to convert value to 'AbortSignal'.",
                    AnyName,
                    nameof(AbortSignal)
                );
            }
        }

        return abortSignal;
    }

    private AbortSignalInstance Any(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, AnyName, nameof(AbortSignal));

        return Any(arguments[0].AsArray());
    }
}
