using System;
using Jint;
using Jint.Runtime.Interop;
using Yilduz.Aborting.AbortController;
using Yilduz.Aborting.AbortSignal;
using Yilduz.Console;
using Yilduz.DOM.DOMException;
using Yilduz.Encoding.TextDecoder;
using Yilduz.Encoding.TextEncoder;
using Yilduz.Events.Event;
using Yilduz.Events.EventTarget;
using Yilduz.Events.ProgressEvent;
using Yilduz.Files.Blob;
using Yilduz.Files.File;
using Yilduz.Files.FileReader;
using Yilduz.Files.FileReaderSync;
using Yilduz.Network.FormData;
using Yilduz.Network.XMLHttpRequest;
using Yilduz.Network.XMLHttpRequestEventTarget;
using Yilduz.Network.XMLHttpRequestUpload;
using Yilduz.Services;
using Yilduz.Storages.Storage;
using Yilduz.Streams.ByteLengthQueuingStrategy;
using Yilduz.Streams.CountQueuingStrategy;
using Yilduz.Streams.ReadableByteStreamController;
using Yilduz.Streams.ReadableStream;
using Yilduz.Streams.ReadableStreamBYOBReader;
using Yilduz.Streams.ReadableStreamDefaultController;
using Yilduz.Streams.ReadableStreamDefaultReader;
using Yilduz.Streams.WritableStream;
using Yilduz.Streams.WritableStreamDefaultController;
using Yilduz.Streams.WritableStreamDefaultWriter;
using Yilduz.URLs.URL;
using Yilduz.URLs.URLSearchParams;

namespace Yilduz;

public sealed class WebApiIntrinsics
{
    public Options Options { get; }
    internal AbortControllerConstructor AbortController { get; }
    public AbortSignalConstructor AbortSignal { get; }

    internal BlobConstructor Blob { get; }
    internal FileConstructor File { get; }
    internal FileReaderConstructor FileReader { get; }
    internal FileReaderSyncConstructor FileReaderSync { get; }

    internal TextEncoderConstructor TextEncoder { get; }
    internal TextDecoderConstructor TextDecoder { get; }

    internal DOMExceptionConstructor DOMException { get; }

    public EventTargetConstructor EventTarget { get; }
    public EventConstructor Event { get; }
    internal ProgressEventConstructor ProgressEvent { get; }

    internal URLConstructor URL { get; }
    internal URLSearchParamsConstructor URLSearchParams { get; }

    internal ReadableStreamConstructor ReadableStream { get; }
    internal ReadableStreamDefaultControllerConstructor ReadableStreamDefaultController { get; }
    internal ReadableStreamDefaultReaderConstructor ReadableStreamDefaultReader { get; }
    internal ReadableStreamBYOBReaderConstructor ReadableStreamBYOBReader { get; }
    internal ReadableByteStreamControllerConstructor ReadableByteStreamController { get; }
    internal WritableStreamConstructor WritableStream { get; }
    internal WritableStreamDefaultWriterConstructor WritableStreamDefaultWriter { get; }
    internal WritableStreamDefaultControllerConstructor WritableStreamDefaultController { get; }
    internal CountQueuingStrategyConstructor CountQueuingStrategy { get; }
    internal ByteLengthQueuingStrategyConstructor ByteLengthQueuingStrategy { get; }

    internal StorageConstructor Storage { get; }
    public StorageInstance LocalStorage { get; }
    public StorageInstance SessionStorage { get; }

    internal Base64Provider Base64Provider { get; }
    internal TimerProvider TimerProvider { get; }
    public ConsoleInstance Console { get; }

    internal FormDataConstructor FormData { get; }
    internal XMLHttpRequestConstructor XMLHttpRequest { get; }
    internal XMLHttpRequestEventTargetConstructor XMLHttpRequestEventTarget { get; }
    internal XMLHttpRequestUploadConstructor XMLHttpRequestUpload { get; }

    private readonly Engine _engine;

    internal WebApiIntrinsics(Engine engine, Options options)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Options.CancellationToken.ThrowIfCancellationRequested();

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

        TextEncoder = new(_engine);
        TextDecoder = new(_engine);

        DOMException = new(_engine);

        URL = new(_engine);
        URLSearchParams = new(_engine);

        WritableStream = new(_engine);
        WritableStreamDefaultWriter = new(_engine);
        WritableStreamDefaultController = new(_engine);

        ReadableStream = new(_engine);
        ReadableStreamDefaultController = new(_engine);
        ReadableStreamDefaultReader = new(_engine);
        ReadableStreamBYOBReader = new(_engine);
        ReadableByteStreamController = new(_engine);

        CountQueuingStrategy = new(_engine);
        ByteLengthQueuingStrategy = new(_engine);

        FormData = new(_engine);
        XMLHttpRequestEventTarget = new(_engine, this);
        XMLHttpRequestUpload = new(_engine, this);
        XMLHttpRequest = new(_engine, this);

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
        _engine.SetValue(nameof(TextEncoder), TextEncoder);
        _engine.SetValue(nameof(TextDecoder), TextDecoder);
        _engine.SetValue(nameof(DOMException), DOMException);
        _engine.SetValue(nameof(Event), Event);
        _engine.SetValue(nameof(ProgressEvent), ProgressEvent);
        _engine.SetValue(nameof(EventTarget), EventTarget);
        _engine.SetValue(nameof(URL), URL);
        _engine.SetValue(nameof(URLSearchParams), URLSearchParams);
        _engine.SetValue(nameof(CountQueuingStrategy), CountQueuingStrategy);
        _engine.SetValue(nameof(ByteLengthQueuingStrategy), ByteLengthQueuingStrategy);
        _engine.SetValue(nameof(ReadableStream), ReadableStream);
        _engine.SetValue(nameof(ReadableStreamDefaultController), ReadableStreamDefaultController);
        _engine.SetValue(nameof(ReadableStreamDefaultReader), ReadableStreamDefaultReader);
        _engine.SetValue(nameof(ReadableStreamBYOBReader), ReadableStreamBYOBReader);
        _engine.SetValue(nameof(ReadableByteStreamController), ReadableByteStreamController);
        _engine.SetValue(nameof(WritableStream), WritableStream);
        _engine.SetValue(nameof(WritableStreamDefaultWriter), WritableStreamDefaultWriter);
        _engine.SetValue(nameof(WritableStreamDefaultController), WritableStreamDefaultController);
        _engine.SetValue(nameof(Storage), Storage);
        _engine.SetValue(nameof(FormData), FormData);
        _engine.SetValue(nameof(XMLHttpRequest), XMLHttpRequest);
        _engine.SetValue(nameof(XMLHttpRequestEventTarget), XMLHttpRequestEventTarget);
        _engine.SetValue(nameof(XMLHttpRequestUpload), XMLHttpRequestUpload);
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
