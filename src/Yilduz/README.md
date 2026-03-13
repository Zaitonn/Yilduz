# Yilduz

An extension library for [Jint](https://github.com/sebastienros/jint) that adds [Web API](https://developer.mozilla.org/en-US/docs/Web/API) implementations such as [`ReadableStream`](https://developer.mozilla.org/en-US/docs/Web/API/ReadableStream), [`localStorage`](https://developer.mozilla.org/en-US/docs/Web/API/Window/localStorage), [`fetch`](https://developer.mozilla.org/en-US/docs/Web/API/Window/fetch), [`setTimeout`](https://developer.mozilla.org/en-US/docs/Web/API/Window/setTimeout), etc.

## Usage

```sh
dotnet add package Yilduz
```

```cs
using Jint;
using Yilduz;

using var cts = new CancellationTokenSource();
using var engine = new Engine((o) => o.CancellationToken(cts.Token)).InitializeWebApi(
    new() { CancellationToken = cts.Token }
);

engine.Execute(
    """
    console.log('Hello world!');
    setTimeout(() => console.log('I can use `setTimeout`!'), 2000);
    """
);

cts.Cancel();
```

>For more information, visit [Zaitonn/Yilduz](https://github.com/Zaitonn/Yilduz) plz :D
