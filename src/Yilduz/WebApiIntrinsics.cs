using System;
using Jint;
using Jint.Runtime.Interop;
using Yilduz.Aborting.AbortController;
using Yilduz.Aborting.AbortSignal;
using Yilduz.Console;
using Yilduz.Data;
using Yilduz.Data.Files.Blob;
using Yilduz.Data.Files.File;
using Yilduz.Data.Files.FileReader;
using Yilduz.Data.Files.FileReaderSync;
using Yilduz.Data.URL;
using Yilduz.Data.URLSearchParams;
using Yilduz.Events.Event;
using Yilduz.Events.EventTarget;
using Yilduz.Events.ProgressEvent;
using Yilduz.Storages.Storage;
using Yilduz.Timers;

namespace Yilduz;

public sealed class WebApiIntrinsics
{
    public Options? Options { get; }
    internal AbortControllerConstructor AbortController { get; }
    public AbortSignalConstructor AbortSignal { get; }

    internal BlobConstructor Blob { get; }
    internal FileConstructor File { get; }
    internal FileReaderConstructor FileReader { get; }
    internal FileReaderSyncConstructor FileReaderSync { get; }

    public EventTargetConstructor EventTarget { get; }
    public EventConstructor Event { get; }
    internal ProgressEventConstructor ProgressEvent { get; }

    internal URLConstructor URL { get; }
    internal URLSearchParamsConstructor URLSearchParams { get; }

    internal StorageConstructor Storage { get; }
    public StorageInstance LocalStorage { get; }
    public StorageInstance SessionStorage { get; }

    internal Base64Provider Base64Provider { get; }
    internal TimerProvider TimerProvider { get; }
    public ConsoleInstance Console { get; }

    private readonly Engine _engine;

    internal WebApiIntrinsics(Engine engine, Options options)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        Options = options ?? throw new ArgumentNullException(nameof(options));

        if (Options.WaitingTimeout < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(Options.WaitingTimeout),
                "Waiting timeout must be non-negative."
            );
        }

        Event = new(_engine);
        ProgressEvent = new(_engine, this);
        EventTarget = new(_engine);

        AbortSignal = new(_engine, this);
        AbortController = new(_engine, this);

        Blob = new(_engine);
        File = new(_engine, this);
        FileReader = new(_engine, this);
        FileReaderSync = new(_engine);

        URL = new(_engine, this);
        URLSearchParams = new(_engine);

        Storage = new(_engine);

        Base64Provider = new(_engine);
        TimerProvider = new(_engine, Options);

        Console = new(
            _engine,
            Options.ConsoleFactory?.Invoke(engine) ?? new DefaultConsole(engine)
        );

        LocalStorage = Storage.CreateInstance(Options.Storage.LocalStorageDataProvider);
        SessionStorage = Storage.CreateInstance(Options.Storage.SessionStorageDataProvider);
        Options.Storage.LocalStorageConfigurator?.Invoke(LocalStorage);
        Options.Storage.SessionStorageConfigurator?.Invoke(SessionStorage);

        ConfigureEngine();
    }

    private void ConfigureEngine()
    {
        _engine.SetValue(nameof(AbortController), AbortController);
        _engine.SetValue(nameof(AbortSignal), AbortSignal);
        _engine.SetValue(nameof(Blob), Blob);
        _engine.SetValue(nameof(File), File);
        _engine.SetValue(nameof(FileReader), FileReader);
        _engine.SetValue(nameof(FileReaderSync), FileReaderSync);
        _engine.SetValue(nameof(Event), Event);
        _engine.SetValue(nameof(ProgressEvent), ProgressEvent);
        _engine.SetValue(nameof(EventTarget), EventTarget);
        _engine.SetValue(nameof(URL), URL);
        _engine.SetValue(nameof(URLSearchParams), URLSearchParams);
        _engine.SetValue(nameof(Storage), Storage);

        _engine.SetValue("console", Console);
        _engine.SetValue("localStorage", LocalStorage);
        _engine.SetValue("sessionStorage", SessionStorage);

        _engine.SetValue("atob", new ClrFunction(_engine, "atob", Base64Provider.Decode));
        _engine.SetValue("btoa", new ClrFunction(_engine, "btoa", Base64Provider.Encode));

        _engine.SetValue(
            "setTimeout",
            new ClrFunction(_engine, "setTimeout", TimerProvider.SetTimeout)
        );
        _engine.SetValue(
            "setInterval",
            new ClrFunction(_engine, "setInterval", TimerProvider.SetInterval)
        );
        _engine.SetValue(
            "clearTimeout",
            new ClrFunction(_engine, "clearTimeout", TimerProvider.Clear)
        );
        _engine.SetValue(
            "clearInterval",
            new ClrFunction(_engine, "clearInterval", TimerProvider.Clear)
        );
    }
}
