# Yilduz

[![wakatime](https://wakatime.com/badge/github/Zaitonn/Yilduz.svg)](https://wakatime.com/badge/github/Zaitonn/Yilduz)
[![NuGet Version](https://img.shields.io/nuget/v/Yilduz)](https://www.nuget.org/packages/Yilduz)

An extension library for Jint that adds [Web API](https://developer.mozilla.org/en-US/docs/Web/API) implementations such as [`ReadableStream`](https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream), [`localStorage`](https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage), [`fetch`](https://developer.mozilla.org/en-US/docs/Web/API/Window/fetch), [`setTimeout`](https://developer.mozilla.org/en-US/docs/Web/API/Window/setTimeout), etc.

## Usage

```sh
dotnet add package Yilduz
```

```cs
using Jint;
using Yilduz;

var cts = new CancellationTokenSource();
var engine = new Engine((o) => o.CancellationToken(cts.Token)).InitializeWebApi(
    new() { CancellationToken = cts.Token }
);

engine.Execute(
    """
    console.log('Hello world!');
    setTimeout(() => console.log('I can use `setTimeout`!'), 2000);
    """
);

engine.Dispose();
cts.Dispose();
```

## Development Progress

<details>
<summary>Click to expand</summary>

- Aborting
  - [x] `AbortController`
  - [x] `AbortSignal`
- Base64
  - [x] `atob()`
  - [x] `btoa()`
- Compression
  - [ ] `CompressionStream`
  - [ ] `DecompressionStream`
- Console
  - [x] `console`
- Data
  - [x] `Blob`
  - [x] `File`
  - [x] `FileReader`
  - [x] `FileReaderSync`
  - [x] `FormData`
- DOM
  - [ ] `DOMError`
  - [x] `DOMException`
- Encoding
  - [x] `TextDecoder`
  - [x] `TextDecoderStream`
  - [x] `TextEncoder`
  - [x] `TextEncoderStream`
- Events
  - [x] `Event`
  - [x] `EventTarget`
  - [x] `ProgressEvent`
- Network
  - [x] `fetch()`
  - [x] `Headers`
  - [x] `Request`
  - [x] `Response`
  - [ ] `WebSocket`
  - [ ] `XMLHttpRequest`
  - [ ] `XMLHttpRequestEventTarget`
  - [ ] `XMLHttpRequestUpload`
- Streams
  - [x] `ByteLengthQueuingStrategy`
  - [x] `CountQueuingStrategy`
  - [ ] `ReadableByteStreamController`
  - [x] `ReadableStream`
    - Async iteration is not implemented yet in Jint
  - [ ] `ReadableStreamBYOBReader`
  - [x] `ReadableStreamBYOBRequest`
  - [x] `ReadableStreamDefaultReader`
  - [x] `ReadableStreamDefaultController`
  - [x] `WritableStream`
  - [x] `WritableStreamDefaultWriter`
  - [x] `WritableStreamDefaultController`
  - [x] `TransformStream`
  - [x] `TransformStreamDefaultController`
- Storages
  - [x] `localStorage`
  - [x] `sessionStorage`
- Timers
  - [x] `setTimeout()`
  - [x] `setInterval()`
  - [x] `clearTimeout()`
  - [x] `clearInterval()`
- URLs
  - [x] `URL`
  - [x] `URLSearchParams`

</details>

## Known Issues

### Encoding Support

The `TextDecoder` implementation supports common character encodings including UTF-8, UTF-16, ASCII, and ISO-8859-1.

If you need to use additional character encodings beyond the common ones, you'll need to install the [`System.Text.Encoding.CodePages` NuGet package](https://www.nuget.org/packages/System.Text.Encoding.CodePages/) and register the encoding providers:

```cs
using System.Text;

var engine = new Engine().InitializeWebApi(new() { CancellationToken = CancellationToken.None });
// engine.Evaluate("new TextDecoder('gb_2312').encoding"); // throws an error

// Register additional encoding providers
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

engine.Evaluate("new TextDecoder('gb_2312').encoding"); // = 'gbk'
```

This enables support for legacy encodings such as Windows-1252, Shift-JIS, and other code page encodings.

### Spec Deviations

Some behaviors may differ slightly from Web specs because certain features wrap .NET types; we're working through these gaps. For example:

- `new URL("about:blank").origin` should be `'null'` instead of `'about:blank'`
- `new URL("http://example.com").port` should be `''` instead of `'80'`

## Origin of the name

~~It was chosen arbitrarily.~~

The name comes from a little-known alternative name for [the North Star](https://en.wikipedia.org/wiki/Polaris).
