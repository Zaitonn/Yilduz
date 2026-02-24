using System;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Yilduz.Extensions;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamBYOBRequest;

internal sealed class ReadableStreamBYOBRequestPrototype : ObjectInstance
{
    private static readonly string ViewName = nameof(ReadableStreamBYOBRequestInstance.View)
        .ToJsStyleName();
    private static readonly string ViewGetterName = ViewName.ToJsGetterName();

    private static readonly string RespondName = nameof(Respond).ToJsStyleName();
    private static readonly string RespondWithNewViewName = nameof(RespondWithNewView)
        .ToJsStyleName();

    public ReadableStreamBYOBRequestPrototype(
        Engine engine,
        ReadableStreamBYOBRequestConstructor constructor
    )
        : base(engine)
    {
        Set(GlobalSymbolRegistry.ToStringTag, nameof(ReadableStreamBYOBRequest));
        SetOwnProperty("constructor", new(constructor, false, false, true));

        FastSetProperty(
            ViewName,
            new GetSetPropertyDescriptor(
                get: new ClrFunction(engine, ViewGetterName, GetView),
                set: null,
                false,
                true
            )
        );

        FastSetProperty(
            RespondName,
            new(new ClrFunction(engine, RespondName, Respond), false, false, true)
        );
        FastSetProperty(
            RespondWithNewViewName,
            new(
                new ClrFunction(engine, RespondWithNewViewName, RespondWithNewView),
                false,
                false,
                true
            )
        );
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#rs-byob-request-view
    /// </summary>
    private static JsValue GetView(JsValue thisObject, JsValue[] arguments)
    {
        return thisObject.EnsureThisObject<ReadableStreamBYOBRequestInstance>().View;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#rs-byob-request-respond
    /// </summary>
    private static JsValue Respond(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamBYOBRequestInstance>();
        var bytesWritten = ToEnforcedUnsignedLongLong(instance.Engine, arguments.At(0), "respond");

        instance.Respond(bytesWritten);
        return Undefined;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#rs-byob-request-respond-with-new-view
    /// </summary>
    private static JsValue RespondWithNewView(JsValue thisObject, JsValue[] arguments)
    {
        var instance = thisObject.EnsureThisObject<ReadableStreamBYOBRequestInstance>();
        instance.RespondWithNewView(arguments.At(0));
        return Undefined;
    }

    /// <summary>
    /// https://webidl.spec.whatwg.org/#es-unsigned-long-long
    /// </summary>
    private static ulong ToEnforcedUnsignedLongLong(Engine engine, JsValue value, string methodName)
    {
        if (!value.IsNumber())
        {
            TypeErrorHelper.Throw(
                engine,
                $"Failed to execute '{methodName}' on 'ReadableStreamBYOBRequest': value is not a number."
            );
        }

        var number = value.AsNumber();
        if (
            double.IsNaN(number)
            || double.IsInfinity(number)
            || number < 0
            || number > ulong.MaxValue
        )
        {
            TypeErrorHelper.Throw(
                engine,
                $"Failed to execute '{methodName}' on 'ReadableStreamBYOBRequest': value is out of range for unsigned long long."
            );
        }

        var truncated = Math.Truncate(number);
        if (truncated != number)
        {
            TypeErrorHelper.Throw(
                engine,
                $"Failed to execute '{methodName}' on 'ReadableStreamBYOBRequest': value is not an integer."
            );
        }

        return (ulong)truncated;
    }
}
