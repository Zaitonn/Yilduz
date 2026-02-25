using System.IO;
using Jint;
using Xunit;

namespace Yilduz.Tests.Fetch;

public sealed class RequestInputTests : FetchTestBase
{
    [Fact]
    public void ShouldUseRequestMethod()
    {
        string? receivedMethod = null;

        MapRoute(
            "POST",
            "/method",
            async ctx =>
            {
                receivedMethod = ctx.Request.HttpMethod;
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            async function run() {
                const req = new Request('{{BaseUrl}}method', { method: 'POST' });
                await fetch(req);
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("POST", receivedMethod);
    }

    [Fact]
    public void ShouldCarryRequestHeaders()
    {
        string? received = null;

        MapGet(
            "/req-headers",
            async ctx =>
            {
                received = ctx.Request.Headers["X-From-Request"];
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            async function run() {
                const req = new Request('{{BaseUrl}}req-headers', {
                    headers: { 'X-From-Request': 'request-header-value' }
                });
                await fetch(req);
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("request-header-value", received);
    }

    [Fact]
    public void ShouldCarryRequestBody()
    {
        string? received = null;

        MapRoute(
            "POST",
            "/req-body",
            async ctx =>
            {
                using var reader = new StreamReader(ctx.Request.InputStream);
                received = await reader.ReadToEndAsync();
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            async function run() {
                const req = new Request('{{BaseUrl}}req-body', {
                    method: 'POST',
                    body: 'request body content'
                });
                await fetch(req);
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("request body content", received);
    }

    [Fact]
    public void ShouldOverrideRequestMethodWithInit()
    {
        string? receivedMethod = null;

        MapRoute(
            "PUT",
            "/override-method",
            async ctx =>
            {
                receivedMethod = ctx.Request.HttpMethod;
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            async function run() {
                const req = new Request('{{BaseUrl}}override-method', { method: 'POST' });
                await fetch(req, { method: 'PUT', body: 'updated' });
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("PUT", receivedMethod);
    }

    [Fact]
    public void ShouldOverrideRequestHeadersWithInit()
    {
        string? received = null;

        MapGet(
            "/override-headers",
            async ctx =>
            {
                received = ctx.Request.Headers["X-Header"];
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            async function run() {
                const req = new Request('{{BaseUrl}}override-headers', {
                    headers: { 'X-Header': 'original' }
                });
                await fetch(req, {
                    headers: { 'X-Header': 'overridden' }
                });
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("overridden", received);
    }

    [Fact]
    public void ShouldUseClonedRequestForFetch()
    {
        MapGet("/cloned", async ctx => await WriteResponseAsync(ctx, 200, "cloned response"));

        Execute(
            $$"""
            var text1, text2;
            async function run() {
                const req = new Request('{{BaseUrl}}cloned');
                const cloned = req.clone();
                const res1 = await fetch(req);
                text1 = await res1.text();
                const res2 = await fetch(cloned);
                text2 = await res2.text();
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("cloned response", Evaluate("text1").AsString());
        Assert.Equal("cloned response", Evaluate("text2").AsString());
    }

    [Fact]
    public void ShouldPreserveRequestUrl()
    {
        MapGet("/url-preserved", async ctx => await WriteResponseAsync(ctx, 200, "ok"));

        Execute(
            $$"""
            var reqUrl, resUrl;
            async function run() {
                const req = new Request('{{BaseUrl}}url-preserved');
                reqUrl = req.url;
                const res = await fetch(req);
                resUrl = res.url;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Contains("url-preserved", Evaluate("reqUrl").AsString());
        Assert.Contains("url-preserved", Evaluate("resUrl").AsString());
    }

    [Fact]
    public void ShouldCreateRequestFromAnotherRequestAndFetch()
    {
        string? received = null;

        MapRoute(
            "POST",
            "/from-request",
            async ctx =>
            {
                using var reader = new StreamReader(ctx.Request.InputStream);
                received = await reader.ReadToEndAsync();
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            async function run() {
                const original = new Request('{{BaseUrl}}from-request', {
                    method: 'POST',
                    headers: { 'Content-Type': 'text/plain' },
                    body: 'original body'
                });
                const copy = new Request(original);
                await fetch(copy);
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("original body", received);
    }

    [Fact]
    public void ShouldFetchWithRequestAndReadJsonResponse()
    {
        MapRoute(
            "POST",
            "/req-json",
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
            async function run() {
                const req = new Request('{{BaseUrl}}req-json', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ ping: 'pong' })
                });
                const res = await fetch(req);
                result = await res.json();
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("pong", Evaluate("result.ping").AsString());
    }

    [Fact]
    public void FetchWithRequestShouldReturnResponseInstance()
    {
        MapGet("/response-type", async ctx => await WriteResponseAsync(ctx, 200, "ok"));

        Execute(
            $$"""
            var isResponse, isObject;
            async function run() {
                const req = new Request('{{BaseUrl}}response-type');
                const res = await fetch(req);
                isResponse = res instanceof Response;
                isObject = res instanceof Object;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("isResponse").AsBoolean());
        Assert.True(Evaluate("isObject").AsBoolean());
    }
}
