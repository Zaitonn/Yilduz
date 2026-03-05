using System;
using System.Runtime.CompilerServices;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using Yilduz.Aborting.AbortController;
using Yilduz.Aborting.AbortSignal;
using Yilduz.Compression.CompressionStream;
using Yilduz.Compression.DecompressionStream;
using Yilduz.Console;
using Yilduz.Data.Blob;
using Yilduz.Data.File;
using Yilduz.Data.FileReader;
using Yilduz.Data.FileReaderSync;
using Yilduz.Data.FormData;
using Yilduz.DOM.DOMException;
using Yilduz.Encoding.TextDecoder;
using Yilduz.Encoding.TextDecoderStream;
using Yilduz.Encoding.TextEncoder;
using Yilduz.Encoding.TextEncoderStream;
using Yilduz.Events.CloseEvent;
using Yilduz.Events.Event;
using Yilduz.Events.EventTarget;
using Yilduz.Events.MessageEvent;
using Yilduz.Events.ProgressEvent;
using Yilduz.Network.Fetch;
using Yilduz.Network.Headers;
using Yilduz.Network.Request;
using Yilduz.Network.Response;
using Yilduz.Network.WebSocket;
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
using Yilduz.Streams.ReadableStreamBYOBRequest;
using Yilduz.Streams.ReadableStreamDefaultController;
using Yilduz.Streams.ReadableStreamDefaultReader;
using Yilduz.Streams.TransformStream;
using Yilduz.Streams.TransformStreamDefaultController;
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
    internal TextEncoderStreamConstructor TextEncoderStream { get; }
    internal TextDecoderStreamConstructor TextDecoderStream { get; }

    internal DOMExceptionConstructor DOMException { get; }

    public EventTargetConstructor EventTarget { get; }
    public EventConstructor Event { get; }
    internal ProgressEventConstructor ProgressEvent { get; }
    internal MessageEventConstructor MessageEvent { get; }
    internal CloseEventConstructor CloseEvent { get; }

    internal URLConstructor URL { get; }
    internal URLSearchParamsConstructor URLSearchParams { get; }

    internal ReadableStreamConstructor ReadableStream { get; }
    internal ReadableStreamDefaultControllerConstructor ReadableStreamDefaultController { get; }
    internal ReadableStreamDefaultReaderConstructor ReadableStreamDefaultReader { get; }
    internal ReadableStreamBYOBReaderConstructor ReadableStreamBYOBReader { get; }
    internal ReadableStreamBYOBRequestConstructor ReadableStreamBYOBRequest { get; }
    internal ReadableByteStreamControllerConstructor ReadableByteStreamController { get; }
    internal WritableStreamConstructor WritableStream { get; }
    internal WritableStreamDefaultWriterConstructor WritableStreamDefaultWriter { get; }
    internal WritableStreamDefaultControllerConstructor WritableStreamDefaultController { get; }
    internal CountQueuingStrategyConstructor CountQueuingStrategy { get; }
    internal ByteLengthQueuingStrategyConstructor ByteLengthQueuingStrategy { get; }
    internal TransformStreamConstructor TransformStream { get; }
    internal TransformStreamDefaultControllerConstructor TransformStreamDefaultController { get; }
    internal CompressionStreamConstructor CompressionStream { get; }
    internal DecompressionStreamConstructor DecompressionStream { get; }

    internal StorageConstructor Storage { get; }
    public StorageInstance LocalStorage { get; }
    public StorageInstance SessionStorage { get; }

    internal Base64Provider Base64Provider { get; }
    internal EventLoop EventLoop { get; }
    internal TimerProvider TimerProvider { get; }
    public ConsoleInstance Console { get; }

    internal FormDataConstructor FormData { get; }
    internal RequestConstructor Request { get; }
    internal ResponseConstructor Response { get; }
    internal HeadersConstructor Headers { get; }
    internal WebSocketConstructor WebSocket { get; }
    internal XMLHttpRequestConstructor XMLHttpRequest { get; }
    internal XMLHttpRequestEventTargetConstructor XMLHttpRequestEventTarget { get; }
    internal XMLHttpRequestUploadConstructor XMLHttpRequestUpload { get; }

    internal FetchProvider FetchProvider { get; }

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
        MessageEvent = new(_engine, this);
        CloseEvent = new(_engine, this);
        EventTarget = new(_engine);

        AbortSignal = new(_engine, this);
        AbortController = new(_engine, this);

        Blob = new(_engine);
        File = new(_engine, this);
        FileReader = new(_engine, this);
        FileReaderSync = new(_engine);

        TextEncoder = new(_engine);
        TextDecoder = new(_engine);
        TextEncoderStream = new(_engine);
        TextDecoderStream = new(_engine);

        DOMException = new(_engine);

        URL = new(_engine);
        URLSearchParams = new(_engine);

        WritableStream = new(_engine);
        WritableStreamDefaultWriter = new(_engine);
        WritableStreamDefaultController = new(_engine);

        ReadableStream = new(_engine, this);
        ReadableStreamDefaultController = new(_engine);
        ReadableStreamDefaultReader = new(_engine);
        ReadableStreamBYOBReader = new(_engine);
        ReadableStreamBYOBRequest = new(_engine);
        ReadableByteStreamController = new(_engine);

        TransformStream = new(_engine);
        TransformStreamDefaultController = new(_engine);

        CompressionStream = new(_engine, this);
        DecompressionStream = new(_engine, this);

        CountQueuingStrategy = new(_engine);
        ByteLengthQueuingStrategy = new(_engine);

        FormData = new(_engine);
        Request = new(_engine, this);
        Response = new(_engine, this);
        Headers = new(_engine);
        WebSocket = new(_engine, this);
        XMLHttpRequestEventTarget = new(_engine, this);
        XMLHttpRequestUpload = new(_engine, this);
        XMLHttpRequest = new(_engine, this);

        FetchProvider = new(_engine, this);

        Storage = new(_engine);

        Base64Provider = new(_engine);
        EventLoop = new(_engine, Options);
        TimerProvider = new(_engine, Options, EventLoop);

        Console = new(
            _engine,
            new(
                () =>
                    Options.ConsoleFactory is not null
                        ? Options.ConsoleFactory.Invoke(engine)
                            ?? throw new InvalidOperationException("Console factory returned null.")
                        : new DefaultConsole(engine)
            )
        );

        LocalStorage = Storage.CreateInstance(Options.Storage.LocalStorageDataProvider);
        SessionStorage = Storage.CreateInstance(Options.Storage.SessionStorageDataProvider);
        Options.Storage.LocalStorageConfigurator?.Invoke(LocalStorage);
        Options.Storage.SessionStorageConfigurator?.Invoke(SessionStorage);

        ConfigureEngine();
    }

    private void ConfigureEngine()
    {
        FastSet(AbortController);
        FastSet(AbortSignal);
        FastSet(Blob);
        FastSet(File);
        FastSet(FileReader);
        FastSet(FileReaderSync);
        FastSet(TextEncoder);
        FastSet(TextDecoder);
        FastSet(TextEncoderStream);
        FastSet(TextDecoderStream);
        FastSet(DOMException);
        FastSet(Event);
        FastSet(ProgressEvent);
        FastSet(MessageEvent);
        FastSet(CloseEvent);
        FastSet(EventTarget);
        FastSet(URL);
        FastSet(URLSearchParams);
        FastSet(CountQueuingStrategy);
        FastSet(ByteLengthQueuingStrategy);
        FastSet(ReadableStream);
        FastSet(ReadableStreamDefaultController);
        FastSet(ReadableStreamDefaultReader);
        FastSet(ReadableStreamBYOBReader);
        FastSet(ReadableStreamBYOBRequest);
        FastSet(ReadableByteStreamController);
        FastSet(WritableStream);
        FastSet(WritableStreamDefaultWriter);
        FastSet(WritableStreamDefaultController);
        FastSet(TransformStream);
        FastSet(TransformStreamDefaultController);
        FastSet(CompressionStream);
        FastSet(DecompressionStream);
        FastSet(Storage);
        FastSet(FormData);
        FastSet(Request);
        FastSet(Response);
        FastSet(Headers);
        FastSet(WebSocket);
        FastSet(XMLHttpRequest);
        FastSet(XMLHttpRequestEventTarget);
        FastSet(XMLHttpRequestUpload);

        Set("console", Console);
        Set("localStorage", LocalStorage);
        Set("sessionStorage", SessionStorage);
        Set("atob", new ClrFunction(_engine, "atob", Base64Provider.Decode));
        Set("btoa", new ClrFunction(_engine, "btoa", Base64Provider.Encode));
        Set("fetch", new ClrFunction(_engine, "fetch", FetchProvider.Fetch));
        Set("setTimeout", new ClrFunction(_engine, "setTimeout", TimerProvider.SetTimeout));
        Set("setInterval", new ClrFunction(_engine, "setInterval", TimerProvider.SetInterval));
        Set("clearTimeout", new ClrFunction(_engine, "clearTimeout", TimerProvider.Clear));
        Set("clearInterval", new ClrFunction(_engine, "clearInterval", TimerProvider.Clear));

        void Set(string key, JsValue jsValue)
        {
            _engine.SetValue(key, jsValue);
        }

        void FastSet(JsValue jsValue, [CallerArgumentExpression(nameof(jsValue))] string key = "")
        {
            _engine.SetValue(key, jsValue);
        }
    }
}
