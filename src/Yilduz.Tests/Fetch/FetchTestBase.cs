using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Yilduz.Tests.Fetch;

public abstract class FetchTestBase : HttpServerTestBase
{
    private readonly Dictionary<string, Func<HttpListenerContext, Task>> _routes = [];

    protected void MapGet(string path, Func<HttpListenerContext, Task> handler)
    {
        _routes[$"GET:{path}"] = handler;
    }

    protected void MapRoute(string method, string path, Func<HttpListenerContext, Task> handler)
    {
        _routes[$"{method.ToUpperInvariant()}:{path}"] = handler;
    }

    protected void MapAny(string path, Func<HttpListenerContext, Task> handler)
    {
        _routes[$"*:{path}"] = handler;
    }

    protected override async Task HandleRequestAsync(HttpListenerContext context)
    {
        var method = context.Request.HttpMethod;
        var path = context.Request.Url?.AbsolutePath ?? "/";

        if (
            _routes.TryGetValue($"{method}:{path}", out var handler)
            || _routes.TryGetValue($"*:{path}", out handler)
        )
        {
            await handler(context).ConfigureAwait(false);
            return;
        }

        // Fallback: 404
        await WriteResponseAsync(context, 404, "Not Found").ConfigureAwait(false);
    }
}
