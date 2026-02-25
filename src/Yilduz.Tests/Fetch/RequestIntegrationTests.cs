using System.IO;
using Jint;
using Xunit;

namespace Yilduz.Tests.Fetch;

public sealed class RequestIntegrationTests : FetchTestBase
{
    [Fact]
    public void ShouldAcceptRequestObjectAsInput()
    {
        MapGet("/req-obj", async ctx => await WriteResponseAsync(ctx, 200, "from request object"));

        Execute(
            $$"""
            var result;
            var done = false;
            async function run() {
                const req = new Request('{{BaseUrl}}req-obj');
                const res = await fetch(req);
                result = await res.text();
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("from request object", Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldAcceptRequestObjectWithInit()
    {
        string? receivedMethod = null;

        MapRoute(
            "POST",
            "/req-init",
            async ctx =>
            {
                receivedMethod = ctx.Request.HttpMethod;
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            var done = false;
            async function run() {
                const req = new Request('{{BaseUrl}}req-init');
                const res = await fetch(req, { method: 'POST', body: 'data' });
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("POST", receivedMethod);
    }

    [Fact]
    public void ShouldEchoJsonRoundTrip()
    {
        MapRoute(
            "POST",
            "/echo-json",
            async ctx =>
            {
                using var reader = new StreamReader(ctx.Request.InputStream);
                var body = await reader.ReadToEndAsync();
                await WriteResponseAsync(ctx, 200, body, "application/json");
            }
        );

        Execute(
            $$"""
            var result;
            var done = false;
            async function run() {
                const payload = { message: 'hello', items: [1, 2, 3] };
                const res = await fetch('{{BaseUrl}}echo-json', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });
                result = await res.json();
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.Equal("hello", Evaluate("result.message").AsString());
        Assert.Equal(3, Evaluate("result.items.length").AsNumber());
        Assert.Equal(2, Evaluate("result.items[1]").AsNumber());
    }

    [Fact]
    public void ShouldExposeResponseUrl()
    {
        MapGet("/url-check", async ctx => await WriteResponseAsync(ctx, 200, "ok"));

        Execute(
            $$"""
            var responseUrl;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}url-check');
                responseUrl = res.url;
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        var url = Evaluate("responseUrl").AsString();
        Assert.Contains("url-check", url);
    }

    [Fact]
    public void ShouldSupportBlobBody()
    {
        string? receivedBody = null;

        MapRoute(
            "POST",
            "/blob-body",
            async ctx =>
            {
                using var reader = new StreamReader(ctx.Request.InputStream);
                receivedBody = await reader.ReadToEndAsync();
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            var done = false;
            async function run() {
                const blob = new Blob(['blob data'], { type: 'text/plain' });
                await fetch('{{BaseUrl}}blob-body', {
                    method: 'POST',
                    body: blob
                });
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("blob data", receivedBody);
    }

    [Fact]
    public void ShouldSupportUint8ArrayBody()
    {
        byte[]? receivedBytes = null;

        MapRoute(
            "POST",
            "/typed-array",
            async ctx =>
            {
                using var ms = new MemoryStream();
                await ctx.Request.InputStream.CopyToAsync(ms);
                receivedBytes = ms.ToArray();
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            var done = false;
            async function run() {
                const data = new Uint8Array([72, 101, 108, 108, 111]);
                await fetch('{{BaseUrl}}typed-array', {
                    method: 'POST',
                    body: data
                });
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.NotNull(receivedBytes);
        Assert.Equal("Hello", System.Text.Encoding.UTF8.GetString(receivedBytes!));
    }
}
