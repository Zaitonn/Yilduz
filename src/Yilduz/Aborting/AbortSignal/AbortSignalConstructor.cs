using System;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Events.EventTarget;
using Yilduz.Utils;

namespace Yilduz.Aborting.AbortSignal;

internal sealed class AbortSignalConstructor : Constructor
{
    private static readonly string AbortName = nameof(Abort).ToJsStyleName();
    private static readonly string TimeoutName = nameof(Timeout).ToJsStyleName();
    private static readonly string AnyName = nameof(Any).ToJsStyleName();

    public AbortSignalConstructor(Engine engine)
        : base(engine, nameof(AbortSignal))
    {
        PrototypeObject = new AbortSignalPrototype(engine, this)
        {
            Prototype = new EventTargetConstructor(engine).PrototypeObject,
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

    public AbortSignalPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        throw new JavaScriptException(
            Engine.Intrinsics.TypeError,
            "Failed to construct 'AbortSignal': Illegal constructor"
        );
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal/abort_static
    /// </summary>
    public AbortSignalInstance Abort(JsValue reason)
    {
        var signal = new AbortSignalInstance(Engine) { Prototype = PrototypeObject };
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
        var signal = new AbortSignalInstance(Engine) { Prototype = PrototypeObject };

        Task.Delay(TimeSpan.FromMilliseconds(time)).ContinueWith(_ => signal.SetAborted("Timeout"));

        return signal;
    }

    private AbortSignalInstance Timeout(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(1, Engine, TimeoutName, "AbortSignal");

        return Timeout((ulong)arguments[0].AsNumber());
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/AbortSignal/any_static
    /// </summary>
    public AbortSignalInstance Any(JsArray signals)
    {
        var abortSignal = new AbortSignalInstance(Engine) { Prototype = PrototypeObject };
        foreach (var signal in signals)
        {
            if (signal is not AbortSignalInstance instance)
            {
                throw new JavaScriptException(
                    Engine.Intrinsics.TypeError,
                    "Value is not of type 'AbortSignal'."
                );
            }

            if (instance.Aborted)
            {
                abortSignal.SetAborted(instance.Reason);
                break;
            }

            instance.Abort += (_, _) => abortSignal.SetAborted(instance.Reason);
        }

        return abortSignal;
    }

    private AbortSignalInstance Any(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(1, Engine, AnyName, "AbortSignal");

        return Any(arguments[0].AsArray());
    }
}
