# Yilduz

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

## Development progress

- Aborting
  - [x] `AbortController`
  - [x] `AbortSignal`
- Base64
  - [x] `atob()`
  - [x] `btoa()`
- Console
  - [x] `console`
- Encoding
  - [ ] `TextDecoder`
  - [ ] `TextDecoderStream`
  - [ ] `TextEncoder`
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
  - [ ] `WritableStream`
  - [ ] `WritableStreamDefaultReader`
  - [ ] `WritableStreamDefaultController`
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

## Origin of the name

~~It was chosen arbitrarily.~~

The name comes from a little-known alternative name for [the North Star](https://en.wikipedia.org/wiki/Polaris).
