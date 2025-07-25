using System;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Utils;

namespace Yilduz.Aborting.AbortSignal;

internal class AbortSignalConstructor : Constructor
{
    public AbortSignalConstructor(Engine engine)
        : base(engine, nameof(AbortSignal))
    {
        PrototypeObject = new AbortSignalPrototype(engine, this);

        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));
        SetOwnProperty(
            nameof(Abort).ToJsStylePropertyName(),
            new(
                new ClrFunction(engine, nameof(Abort).ToJsStylePropertyName(), Abort),
                true,
                false,
                true
            )
        );
        SetOwnProperty(
            nameof(Timeout).ToJsStylePropertyName(),
            new(
                new ClrFunction(engine, nameof(Timeout).ToJsStylePropertyName(), Timeout),
                true,
                false,
                true
            )
        );
        SetOwnProperty(
            nameof(Any).ToJsStylePropertyName(),
            new(
                new ClrFunction(engine, nameof(Any).ToJsStylePropertyName(), Any),
                true,
                false,
                true
            )
        );
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

    private AbortSignalInstance Abort(JsValue thisObject, params JsValue[] arguments)
    {
        return arguments.Length > 0 ? Abort(arguments[0]) : Abort(Undefined);
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
        return arguments.Length == 0
            ? throw new JavaScriptException(
                Engine.Intrinsics.TypeError,
                "1 argument required, but only 0 present."
            )
            : Timeout((ulong)arguments[0].AsNumber());
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
        return arguments.Length == 0 && !arguments[0].IsArray()
            ? throw new JavaScriptException(
                Engine.Intrinsics.TypeError,
                "1 argument required, but only 0 present."
            )
            : Any(arguments[0].AsArray());
    }
}
