using System;
using System.Threading;
using Jint;
using Jint.Runtime.Interop;
using Yilduz.Aborting.AbortController;
using Yilduz.Aborting.AbortSignal;
using Yilduz.Events.Event;
using Yilduz.Events.EventTarget;
using Yilduz.Storages.Storage;
using Yilduz.Timers;

namespace Yilduz;

/// <summary>
/// Extension methods for the Jint engine.
/// </summary>
public static class EngineExtensions
{
    /// <summary>
    /// Adds all APIs to the engine with default options.
    /// </summary>
    public static Engine AddAllApi(this Engine engine)
    {
        return engine.AddAllApi(new());
    }

    /// <summary>
    /// Adds all APIs to the engine with the specified options.
    /// </summary>
    public static Engine AddAllApi(this Engine engine, Options options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        engine.AddAbortingApi();
        engine.AddEventsApi();
        engine.AddStorageApi(
            options.Storage.LocalStorageFactory.Invoke(engine),
            options.Storage.SessionStorageFactory.Invoke(engine)
        );
        engine.AddTimerApi(options.WaitingTimeout, options.CancellationToken);
        return engine;
    }

    /// <summary>
    /// Adds aborting API to the engine.
    /// </summary>
    public static Engine AddAbortingApi(this Engine engine)
    {
        var abortSignalConstructor = new AbortSignalConstructor(engine);
        engine.SetValue(nameof(Aborting.AbortSignal), abortSignalConstructor);
        engine.SetValue(
            nameof(Aborting.AbortController),
            new AbortControllerConstructor(engine, abortSignalConstructor)
        );
        return engine;
    }

    /// <summary>
    /// Adds events API to the engine.
    /// </summary>
    public static Engine AddEventsApi(this Engine engine)
    {
        engine.SetValue(nameof(Events.Event), new EventConstructor(engine));
        engine.SetValue(nameof(Events.EventTarget), new EventTargetConstructor(engine));
        return engine;
    }

    /// <summary>
    /// Adds storage API to the engine with default local and session storage.
    /// </summary>
    public static Engine AddStorageApi(this Engine engine)
    {
        engine.AddStorageApi(new(engine), new(engine));
        return engine;
    }

    /// <summary>
    /// Adds storage API to the engine with custom local and session storage factories.
    /// </summary>
    public static Engine AddStorageApi(
        this Engine engine,
        StorageInstance localStorage,
        StorageInstance sessionStorage
    )
    {
        if (localStorage is null)
        {
            throw new ArgumentNullException(nameof(localStorage));
        }
        if (sessionStorage is null)
        {
            throw new ArgumentNullException(nameof(sessionStorage));
        }

        engine.SetValue("localStorage", localStorage);
        engine.SetValue("sessionStorage", sessionStorage);

        return engine;
    }

    /// <summary>
    /// Adds timer API to the engine.
    /// </summary>
    public static Engine AddTimerApi(
        this Engine engine,
        TimeSpan waitingTimeout,
        CancellationToken cancellationToken
    )
    {
        if (waitingTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(waitingTimeout),
                "Waiting timeout must be greater than zero."
            );
        }

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
