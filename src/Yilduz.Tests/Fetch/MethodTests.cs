using System.IO;
using Jint;
using Xunit;

namespace Yilduz.Tests.Fetch;

public sealed class MethodTests : FetchTestBase
{
    [Fact]
    public void ShouldSendPostRequestWithStringBody()
    {
        string? receivedBody = null;
        string? receivedMethod = null;

        MapRoute(
            "POST",
            "/echo",
            async ctx =>
            {
                receivedMethod = ctx.Request.HttpMethod;
                using var reader = new StreamReader(ctx.Request.InputStream);
                receivedBody = await reader.ReadToEndAsync();
                await WriteResponseAsync(ctx, 200, receivedBody);
            }
        );

        Execute(
            $$"""
            var responseText;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}echo', {
                    method: 'POST',
                    body: 'hello server'
                });
                responseText = await res.text();
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.Equal("POST", receivedMethod);
        Assert.Equal("hello server", receivedBody);
        Assert.Equal("hello server", Evaluate("responseText").AsString());
    }

    [Fact]
    public void ShouldSendPutRequest()
    {
        string? receivedMethod = null;

        MapRoute(
            "PUT",
            "/put",
            async ctx =>
            {
                receivedMethod = ctx.Request.HttpMethod;
                await WriteResponseAsync(ctx, 200, "updated");
            }
        );

        Execute(
            $$"""
            var responseText;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}put', {
                    method: 'PUT',
                    body: 'update payload'
                });
                responseText = await res.text();
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.Equal("PUT", receivedMethod);
        Assert.Equal("updated", Evaluate("responseText").AsString());
    }

    [Fact]
    public void ShouldSendDeleteRequest()
    {
        string? receivedMethod = null;

        MapRoute(
            "DELETE",
            "/resource",
            async ctx =>
            {
                receivedMethod = ctx.Request.HttpMethod;
                await WriteResponseAsync(ctx, 200, "deleted");
            }
        );

        Execute(
            $$"""
            var responseText;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}resource', {
                    method: 'DELETE'
                });
                responseText = await res.text();
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.Equal("DELETE", receivedMethod);
        Assert.Equal("deleted", Evaluate("responseText").AsString());
    }

    [Fact]
    public void ShouldSendPatchRequest()
    {
        string? receivedMethod = null;
        string? receivedBody = null;

        MapRoute(
            "PATCH",
            "/patch",
            async ctx =>
            {
                receivedMethod = ctx.Request.HttpMethod;
                using var reader = new StreamReader(ctx.Request.InputStream);
                receivedBody = await reader.ReadToEndAsync();
                await WriteResponseAsync(ctx, 200, "patched");
            }
        );

        Execute(
            $$"""
            var done = false;
            async function run() {
                await fetch('{{BaseUrl}}patch', {
                    method: 'PATCH',
                    body: '{"op":"replace"}'
                });
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.Equal("PATCH", receivedMethod);
        Assert.Equal("{\"op\":\"replace\"}", receivedBody);
    }

    [Fact]
    public void ShouldSendCustomHeaders()
    {
        string? receivedAuth = null;
        string? receivedCustom = null;

        MapGet(
            "/with-headers",
            async ctx =>
            {
                receivedAuth = ctx.Request.Headers["Authorization"];
                receivedCustom = ctx.Request.Headers["X-Custom"];
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            var done = false;
            async function run() {
                await fetch('{{BaseUrl}}with-headers', {
                    headers: {
                        'Authorization': 'Bearer token123',
                        'X-Custom': 'my-value'
                    }
                });
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.Equal("Bearer token123", receivedAuth);
        Assert.Equal("my-value", receivedCustom);
    }

    [Fact]
    public void ShouldSendJsonBodyWithContentType()
    {
        string? receivedContentType = null;
        string? receivedBody = null;

        MapRoute(
            "POST",
            "/json-body",
            async ctx =>
            {
                receivedContentType = ctx.Request.ContentType;
                using var reader = new StreamReader(ctx.Request.InputStream);
                receivedBody = await reader.ReadToEndAsync();
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            var done = false;
            async function run() {
                await fetch('{{BaseUrl}}json-body', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ key: 'value' })
                });
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.Equal("{\"key\":\"value\"}", receivedBody);
        Assert.Contains("application/json", receivedContentType);
    }
}
