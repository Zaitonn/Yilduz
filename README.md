# Yilduz

[![wakatime](https://wakatime.com/badge/github/Zaitonn/Yilduz.svg)](https://wakatime.com/badge/github/Zaitonn/Yilduz)
[![NuGet Version](https://img.shields.io/nuget/v/Yilduz)](https://www.nuget.org/packages/Yilduz)

Web api implementation ([`AbortController`](https://developer.mozilla.org/en-US/docs/Web/API/AbortController), [`fetch`](https://developer.mozilla.org/en-US/docs/Web/API/Window/fetch), [`setTimeout`](https://developer.mozilla.org/en-US/docs/Web/API/Window/setTimeout), etc.) for [Jint](https://github.com/sebastienros/jint)

## Usage

```sh
dotnet add package Yilduz
```

```cs
using Jint;
using Yilduz;

var engine = new Engine().InitializeWebApi(new());

engine.Execute(
    """
    console.log('Hello world!');
    setTimeout(() => console.log('I can use `setTimeout`!'), 2000);
    """
)
```

## Development Progress

- Aborting
  - [x] `AbortController`
  - [x] `AbortSignal`
- Base64
  - [x] `atob()`
  - [x] `btoa()`
- Console
  - [x] `console`
- DOM
  - [ ] `DOMError`
  - [x] `DOMException`
- Encoding
  - [x] `TextDecoder`
  - [ ] `TextDecoderStream`
  - [x] `TextEncoder`
  - [ ] `TextEncoderStream`
- Files
  - [x] `Blob`
  - [x] `File`
  - [x] `FileReader`
  - [x] `FileReaderSync`
- Events
  - [x] `Event`
  - [x] `EventTarget`
  - [x] `ProgressEvent`
- Network
  - [ ] `FormData`
  - [ ] `XMLHttpRequest`
  - [ ] `XMLHttpRequestEventTarget`
  - [ ] `XMLHttpRequestUpload`
  - [ ] `fetch()`
  - [ ] `Headers`
  - [ ] `Request`
  - [ ] `RequestInit`
  - [ ] `Response`
- Streams
  - [ ] `ReadableStream`
  - [ ] `ReadableStreamDefaultReader`
  - [ ] `ReadableStreamDefaultController`
  - [x] `WritableStream`
  - [x] `WritableStreamDefaultWriter`
  - [x] `WritableStreamDefaultController`
  - [ ] `TransformStream`
  - [ ] `TransformStreamDefaultController`
- Storages
  - [x] `localStorage`
  - [x] `sessionStorage`
- Timers
  - [x] `setTimeout`
  - [x] `setInterval`
  - [x] `clearTimeout`
  - [x] `clearInterval`
- URLs
  - [x] `URL`
  - [x] `URLSearchParams`

## Known Issues

### Encoding Support

The `TextDecoder` implementation supports common character encodings including UTF-8, UTF-16, ASCII, and ISO-8859-1.

If you need to use additional character encodings beyond the common ones, you'll need to install the [`System.Text.Encoding.CodePages` NuGet package](https://www.nuget.org/packages/System.Text.Encoding.CodePages/) and register the encoding providers:

```cs
using System.Text;

var engine = new Engine().InitializeWebApi(new());
// engine.Evaluate("new TextDecoder('gb_2312').encoding"); // throws an error

// Register additional encoding providers
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

engine.Evaluate("new TextDecoder('gb_2312').encoding"); // = 'gbk'
```

This enables support for legacy encodings such as Windows-1252, Shift-JIS, and other code page encodings.

## Origin of the name

~~It was chosen arbitrarily.~~

The name comes from a little-known alternative name for [the North Star](https://en.wikipedia.org/wiki/Polaris).
