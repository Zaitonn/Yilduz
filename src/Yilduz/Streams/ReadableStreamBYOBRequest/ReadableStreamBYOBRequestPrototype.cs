using System;
using Jint;
using Jint.Native;
using Jint.Runtime;
using Yilduz.Models;
using Yilduz.Utils;

namespace Yilduz.Streams.ReadableStreamBYOBRequest;

internal sealed class ReadableStreamBYOBRequestPrototype
    : PrototypeBase<ReadableStreamBYOBRequestInstance>
{
    public ReadableStreamBYOBRequestPrototype(
        Engine engine,
        ReadableStreamBYOBRequestConstructor constructor
    )
        : base(engine, nameof(ReadableStreamBYOBRequest), constructor)
    {
        RegisterProperty("view", request => request.View);

        RegisterMethod("respond", Respond);
        RegisterMethod("respondWithNewView", RespondWithNewView);
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#rs-byob-request-respond
    /// </summary>
    private static JsValue Respond(ReadableStreamBYOBRequestInstance request, JsValue[] arguments)
    {
        var bytesWritten = ToEnforcedUnsignedLongLong(request.Engine, arguments.At(0), "respond");

        request.Respond(bytesWritten);
        return Undefined;
    }

    /// <summary>
    /// https://streams.spec.whatwg.org/#rs-byob-request-respond-with-new-view
    /// </summary>
    private static JsValue RespondWithNewView(
        ReadableStreamBYOBRequestInstance request,
        JsValue[] arguments
    )
    {
        request.RespondWithNewView(arguments.At(0));
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
