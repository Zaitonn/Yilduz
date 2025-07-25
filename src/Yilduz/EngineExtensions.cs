using System;
using System.Threading;
using Jint;
using Jint.Runtime.Interop;
using Yilduz.Aborting.AbortController;
using Yilduz.Aborting.AbortSignal;
using Yilduz.Events.Event;
using Yilduz.Events.EventTarget;
using Yilduz.Timer;

namespace Yilduz;

public static class EngineExtensions
{
    public static Engine AddAbortingApi(this Engine engine)
    {
        ArgumentNullException.ThrowIfNull(engine);

        var abortSignalConstructor = new AbortSignalConstructor(engine);
        engine.SetValue(nameof(Aborting.AbortSignal), abortSignalConstructor);
        engine.SetValue(
            nameof(Aborting.AbortController),
            new AbortControllerConstructor(engine, abortSignalConstructor)
        );
        return engine;
    }

    public static Engine AddEventsApi(this Engine engine)
    {
        ArgumentNullException.ThrowIfNull(engine);

        engine.SetValue(nameof(Events.Event), new EventConstructor(engine));
        engine.SetValue(nameof(Events.EventTarget), new EventTargetConstructor(engine));
        return engine;
    }

    public static Engine AddTimerApi(
        this Engine engine,
        TimeSpan waitingTimeout,
        CancellationToken cancellationToken
    )
    {
        ArgumentNullException.ThrowIfNull(engine);

        var provider = new TimerProvider(engine, waitingTimeout, cancellationToken);

        engine.SetValue("setTimeout", new ClrFunction(engine, "setTimeout", provider.SetTimeout));
        engine.SetValue(
            "setInterval",
            new ClrFunction(engine, "setInterval", provider.SetInterval)
        );
        engine.SetValue("clearTimeout", new ClrFunction(engine, "clearTimeout", provider.Clear));
        engine.SetValue("clearInterval", new ClrFunction(engine, "clearInterval", provider.Clear));

        return engine;
    }
}
