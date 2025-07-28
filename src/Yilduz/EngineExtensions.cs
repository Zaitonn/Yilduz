using System;
using Jint;
using Jint.Runtime.Interop;
using Yilduz.Aborting.AbortController;
using Yilduz.Aborting.AbortSignal;
using Yilduz.Console;
using Yilduz.Data;
using Yilduz.Data.Files.Blob;
using Yilduz.Data.Files.File;
using Yilduz.Data.URL;
using Yilduz.Data.URLSearchParams;
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
    /// Adds all web APIs to the engine with default options.
    /// </summary>
    public static Engine AddWebAPIs(this Engine engine)
    {
        return engine.AddWebAPIs(new());
    }

    /// <summary>
    /// Adds all web APIs to the engine with the specified options.
    /// </summary>
    public static Engine AddWebAPIs(this Engine engine, Options options)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        #region Events

        var eventTargetConstructor = new EventTargetConstructor(engine);
        engine.SetValue(nameof(Events.Event), new EventConstructor(engine));
        engine.SetValue(nameof(Events.EventTarget), eventTargetConstructor);

        #endregion

        #region Aborting

        var abortSignalConstructor = new AbortSignalConstructor(engine, eventTargetConstructor);
        engine.SetValue(nameof(Aborting.AbortSignal), abortSignalConstructor);
        engine.SetValue(
            nameof(Aborting.AbortController),
            new AbortControllerConstructor(engine, abortSignalConstructor)
        );

        #endregion

        #region Data

        var blobConstructor = new BlobConstructor(engine);
        engine.SetValue(nameof(Data.Files.Blob), blobConstructor);
        engine.SetValue(nameof(Data.Files.File), new FileConstructor(engine, blobConstructor));

        var urlSearchParamsConstructor = new URLSearchParamsConstructor(engine);
        engine.SetValue(nameof(Data.URLSearchParams), urlSearchParamsConstructor);
        engine.SetValue(nameof(Data.URL), new URLConstructor(engine, urlSearchParamsConstructor));

        var base64Provider = new Base64Provider(engine);

        engine.SetValue("atob", new ClrFunction(engine, "atob", base64Provider.Decode));
        engine.SetValue("btoa", new ClrFunction(engine, "btoa", base64Provider.Encode));

        #endregion

        #region Timers

        if (options.WaitingTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                "options.WaitingTimeout must be greater than zero."
            );
        }

        var timerProvider = new TimerProvider(
            engine,
            options.WaitingTimeout,
            options.CancellationToken
        );
        engine.SetValue(
            "setTimeout",
            new ClrFunction(engine, "setTimeout", timerProvider.SetTimeout)
        );
        engine.SetValue(
            "setInterval",
            new ClrFunction(engine, "setInterval", timerProvider.SetInterval)
        );
        engine.SetValue(
            "clearTimeout",
            new ClrFunction(engine, "clearTimeout", timerProvider.Clear)
        );
        engine.SetValue(
            "clearInterval",
            new ClrFunction(engine, "clearInterval", timerProvider.Clear)
        );

        #endregion

        #region Storage

        var storageConstructor = new StorageConstructor(engine);
        engine.SetValue(nameof(Storages.Storage), storageConstructor);

        var localStorage = storageConstructor.CreateInstance(
            options.Storage.LocalStorageDataProvider
        );
        var sessionStorage = storageConstructor.CreateInstance(
            options.Storage.SessionStorageDataProvider
        );

        options.Storage.LocalStorageConfigurator?.Invoke(localStorage);
        options.Storage.SessionStorageConfigurator?.Invoke(sessionStorage);

        engine.SetValue("localStorage", localStorage);
        engine.SetValue("sessionStorage", sessionStorage);

        #endregion

        #region Console

        var console = options.ConsoleFactory?.Invoke(engine) ?? new DefaultConsole(engine);
        engine.SetValue("console", new ConsoleInstance(engine, console));

        #endregion

        return engine;
    }
}
