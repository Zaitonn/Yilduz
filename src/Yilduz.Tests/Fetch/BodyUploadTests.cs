using System.IO;
using Jint;
using Xunit;

namespace Yilduz.Tests.Fetch;

public sealed class BodyUploadTests : FetchTestBase
{
    [Fact]
    public void ShouldUploadStringBody()
    {
        string? received = null;

        MapRoute(
            "POST",
            "/string",
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
                await fetch('{{BaseUrl}}string', {
                    method: 'POST',
                    body: 'plain text body'
                });
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("plain text body", received);
    }

    [Fact]
    public void ShouldUploadBlobBody()
    {
        string? received = null;

        MapRoute(
            "POST",
            "/blob",
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
                const blob = new Blob(['hello', ' ', 'blob'], { type: 'text/plain' });
                await fetch('{{BaseUrl}}blob', {
                    method: 'POST',
                    body: blob
                });
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("hello blob", received);
    }

    [Fact]
    public void ShouldUploadUint8ArrayBody()
    {
        byte[]? received = null;

        MapRoute(
            "POST",
            "/uint8",
            async ctx =>
            {
                using var ms = new MemoryStream();
                await ctx.Request.InputStream.CopyToAsync(ms);
                received = ms.ToArray();
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            async function run() {
                const data = new Uint8Array([0x48, 0x69, 0x21]);
                await fetch('{{BaseUrl}}uint8', {
                    method: 'POST',
                    body: data
                });
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.NotNull(received);
        Assert.Equal("Hi!"u8.ToArray(), received);
    }

    [Fact]
    public void ShouldUploadArrayBufferBody()
    {
        byte[]? received = null;

        MapRoute(
            "POST",
            "/arraybuffer",
            async ctx =>
            {
                using var ms = new MemoryStream();
                await ctx.Request.InputStream.CopyToAsync(ms);
                received = ms.ToArray();
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            async function run() {
                const buf = new Uint8Array([1, 2, 3, 4, 5]).buffer;
                await fetch('{{BaseUrl}}arraybuffer', {
                    method: 'POST',
                    body: buf
                });
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.NotNull(received);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5 }, received);
    }

    [Fact]
    public void ShouldUploadURLSearchParamsBody()
    {
        string? received = null;
        string? contentType = null;

        MapRoute(
            "POST",
            "/search-params",
            async ctx =>
            {
                contentType = ctx.Request.ContentType;
                using var reader = new StreamReader(ctx.Request.InputStream);
                received = await reader.ReadToEndAsync();
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            async function run() {
                const params = new URLSearchParams();
                params.set('foo', 'bar');
                params.set('baz', 'qux');
                await fetch('{{BaseUrl}}search-params', {
                    method: 'POST',
                    body: params
                });
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.NotNull(received);
        Assert.Contains("foo=bar", received);
        Assert.Contains("baz=qux", received);
    }

    [Fact]
    public void ShouldUploadJsonStringifiedBody()
    {
        string? received = null;

        MapRoute(
            "POST",
            "/json",
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
                await fetch('{{BaseUrl}}json', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ a: 1, b: [2, 3] })
                });
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("{\"a\":1,\"b\":[2,3]}", received);
    }

    [Fact]
    public void ShouldUploadEmptyStringBody()
    {
        long? receivedLength = null;

        MapRoute(
            "POST",
            "/empty-string",
            async ctx =>
            {
                receivedLength = ctx.Request.ContentLength64;
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            async function run() {
                await fetch('{{BaseUrl}}empty-string', {
                    method: 'POST',
                    body: ''
                });
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(0, receivedLength);
    }

    [Fact]
    public void ShouldUploadNonAsciiStringBody()
    {
        string? received = null;

        MapRoute(
            "POST",
            "/unicode",
            async ctx =>
            {
                using var reader = new StreamReader(
                    ctx.Request.InputStream,
                    System.Text.Encoding.UTF8
                );
                received = await reader.ReadToEndAsync();
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            async function run() {
                await fetch('{{BaseUrl}}unicode', {
                    method: 'POST',
                    body: '你好世界🌍'
                });
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("你好世界🌍", received);
    }

    [Fact]
    public void ShouldSendGetWithoutBody()
    {
        long? receivedLength = null;

        MapGet(
            "/no-body",
            async ctx =>
            {
                receivedLength = ctx.Request.ContentLength64;
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            async function run() {
                await fetch('{{BaseUrl}}no-body');
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        // GET requests should have no body (content-length = -1 means not sent)
        Assert.True(receivedLength is -1 or 0);
    }

    [Fact]
    public void ShouldEchoLargeBody()
    {
        string? received = null;

        MapRoute(
            "POST",
            "/large",
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
                const large = 'x'.repeat(10000);
                await fetch('{{BaseUrl}}large', {
                    method: 'POST',
                    body: large
                });
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.NotNull(received);
        Assert.Equal(10000, received.Length);
    }
}
