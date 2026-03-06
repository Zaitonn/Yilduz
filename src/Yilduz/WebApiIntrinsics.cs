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
    public AbortControllerConstructor AbortController { get; }
    public AbortSignalConstructor AbortSignal { get; }

    public BlobConstructor Blob { get; }
    public FileConstructor File { get; }
    public FileReaderConstructor FileReader { get; }
    public FileReaderSyncConstructor FileReaderSync { get; }

    public TextEncoderConstructor TextEncoder { get; }
    public TextDecoderConstructor TextDecoder { get; }
    public TextEncoderStreamConstructor TextEncoderStream { get; }
    public TextDecoderStreamConstructor TextDecoderStream { get; }

    public DOMExceptionConstructor DOMException { get; }

    public EventTargetConstructor EventTarget { get; }
    public EventConstructor Event { get; }
    public ProgressEventConstructor ProgressEvent { get; }
    public MessageEventConstructor MessageEvent { get; }
    public CloseEventConstructor CloseEvent { get; }

    public URLConstructor URL { get; }
    public URLSearchParamsConstructor URLSearchParams { get; }

    public ReadableStreamConstructor ReadableStream { get; }
    internal ReadableStreamDefaultControllerConstructor ReadableStreamDefaultController { get; }
    internal ReadableStreamDefaultReaderConstructor ReadableStreamDefaultReader { get; }
    public ReadableStreamBYOBReaderConstructor ReadableStreamBYOBReader { get; }
    internal ReadableStreamBYOBRequestConstructor ReadableStreamBYOBRequest { get; }
    internal ReadableByteStreamControllerConstructor ReadableByteStreamController { get; }
    public WritableStreamConstructor WritableStream { get; }
    internal WritableStreamDefaultWriterConstructor WritableStreamDefaultWriter { get; }
    internal WritableStreamDefaultControllerConstructor WritableStreamDefaultController { get; }
    public CountQueuingStrategyConstructor CountQueuingStrategy { get; }
    public ByteLengthQueuingStrategyConstructor ByteLengthQueuingStrategy { get; }
    public TransformStreamConstructor TransformStream { get; }
    internal TransformStreamDefaultControllerConstructor TransformStreamDefaultController { get; }
    public CompressionStreamConstructor CompressionStream { get; }
    public DecompressionStreamConstructor DecompressionStream { get; }

    internal StorageConstructor Storage { get; }
    public StorageInstance LocalStorage { get; }
    public StorageInstance SessionStorage { get; }

    internal FetchProvider FetchProvider { get; }
    internal Base64Provider Base64Provider { get; }
    internal EventLoop EventLoop { get; }
    internal TimerProvider TimerProvider { get; }
    public ConsoleInstance Console { get; }

    public FormDataConstructor FormData { get; }
    public RequestConstructor Request { get; }
    public ResponseConstructor Response { get; }
    public HeadersConstructor Headers { get; }
    public WebSocketConstructor WebSocket { get; }
    public XMLHttpRequestConstructor XMLHttpRequest { get; }
    public XMLHttpRequestEventTargetConstructor XMLHttpRequestEventTarget { get; }
    public XMLHttpRequestUploadConstructor XMLHttpRequestUpload { get; }

    private readonly Engine _engine;

    internal WebApiIntrinsics(Engine engine, Options options)
    {
        _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        Options = options ?? throw new ArgumentNullException(nameof(options));
        Options.CancellationToken.ThrowIfCancellationRequested();

        if (!Options.CancellationToken.CanBeCanceled)
        {
            throw new ArgumentException(
                "The provided cancellation token must be cancellable.",
                nameof(options)
            );
        }

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

        Blob = new(_engine, this);
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

        LocalStorage = Storage.CreateInstance(Options.Storage.LocalStorage.DataProvider);
        SessionStorage = Storage.CreateInstance(Options.Storage.SessionStorage.DataProvider);
        Options.Storage.LocalStorage.Configurator?.Invoke(LocalStorage);
        Options.Storage.SessionStorage.Configurator?.Invoke(SessionStorage);

        ConfigureEngine();
    }

    private void ConfigureEngine()
    {
        Set(AbortController);
        Set(AbortSignal);
        Set(Blob);
        Set(File);
        Set(FileReader);
        Set(FileReaderSync);
        Set(TextEncoder);
        Set(TextDecoder);
        Set(TextEncoderStream);
        Set(TextDecoderStream);
        Set(DOMException);
        Set(Event);
        Set(ProgressEvent);
        Set(MessageEvent);
        Set(CloseEvent);
        Set(EventTarget);
        Set(URL);
        Set(URLSearchParams);
        Set(CountQueuingStrategy);
        Set(ByteLengthQueuingStrategy);
        Set(ReadableStream);
        Set(ReadableStreamDefaultController);
        Set(ReadableStreamDefaultReader);
        Set(ReadableStreamBYOBReader);
        Set(ReadableStreamBYOBRequest);
        Set(ReadableByteStreamController);
        Set(WritableStream);
        Set(WritableStreamDefaultWriter);
        Set(WritableStreamDefaultController);
        Set(TransformStream);
        Set(TransformStreamDefaultController);
        Set(CompressionStream);
        Set(DecompressionStream);
        Set(Storage);
        Set(FormData);
        Set(Request);
        Set(Response);
        Set(Headers);
        Set(WebSocket);
        Set(XMLHttpRequest);
        Set(XMLHttpRequestEventTarget);
        Set(XMLHttpRequestUpload);

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
    }

    private void Set(string key, JsValue jsValue)
    {
        _engine.SetValue(key, jsValue);
    }

    private void Set(JsValue jsValue, [CallerArgumentExpression(nameof(jsValue))] string key = "")
    {
        _engine.SetValue(key, jsValue);
    }
}
