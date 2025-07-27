# Yilduz

Web api and nodejs module api implementation ([`AbortController`](https://developer.mozilla.org/en-US/docs/Web/API/AbortController), [`fetch`](https://developer.mozilla.org/en-US/docs/Web/API/Window/fetch), [`setTimeout`](https://developer.mozilla.org/en-US/docs/Web/API/Window/setTimeout), etc.) for [Jint](https://github.com/sebastienros/jint)

## Usage

```sh
dotnet add package Yilduz
```

```cs
using Jint;
using Yilduz;

var engine = new Engine().AddWebAPIs();

engine.Execute(
    """
    console.log('Hello world!');
    setTimeout(()=> console.log('I can use `setTimeout`!'), 2000);
    """
)
```

## Development progress & Project structure

- [x] Aborting
  - [x] `AbortController`
  - [x] `AbortSignal`
- [x] Console
  - [x] `console`
- [ ] Data
  - [ ] `Blob`
  - [ ] `ReadableStream`
  - [x] `URL`
  - [x] `URLSearchParams` (`entries()`, `keys()` and `values()` are not implemented yet)
- [x] Events
  - [x] `Event`
  - [x] `EventTarget`
- [x] Storages
  - [x] `localStorage`
  - [x] `sessionStorage`
- [x] Timers
  - [x] `setTimeout`
  - [x] `setInterval`
  - [x] `clearTimeout`
  - [x] `clearInterval`

## Origin of the name

~~It was chosen arbitrarily.~~

The name comes from a little-known alternative name for [the North Star](https://en.wikipedia.org/wiki/Polaris).
