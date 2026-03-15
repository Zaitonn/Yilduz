using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.Fetch;

public sealed class BasicTests : HttpRouteTestBase
{
    [Fact]
    public void ShouldFetchTextResponse()
    {
        MapGet("/text", async ctx => await WriteResponseAsync(ctx, 200, "hello from server"));

        Execute(
            $$"""
            var result;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}text');
                result = await res.text();
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.Equal("hello from server", Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldFetchJsonResponse()
    {
        MapGet(
            "/json",
            async ctx =>
                await WriteResponseAsync(
                    ctx,
                    200,
                    """{"name":"Yilduz","version":1}""",
                    "application/json"
                )
        );

        Execute(
            $$"""
            var result;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}json');
                result = await res.json();
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.Equal("Yilduz", Evaluate("result.name").AsString());
        Assert.Equal(1, Evaluate("result.version").AsNumber());
    }

    [Fact]
    public void ShouldExposeStatusAndOk()
    {
        MapGet("/ok", async ctx => await WriteResponseAsync(ctx, 200, "OK"));

        Execute(
            $$"""
            var status;
            var ok;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}ok');
                status = res.status;
                ok = res.ok;
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.Equal(200, Evaluate("status").AsNumber());
        Assert.True(Evaluate("ok").AsBoolean());
    }

    [Fact]
    public void ShouldExposeNonOkStatus()
    {
        MapGet("/not-found", async ctx => await WriteResponseAsync(ctx, 404, "Not Found"));

        Execute(
            $$"""
            var status;
            var ok;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}not-found');
                status = res.status;
                ok = res.ok;
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(404, Evaluate("status").AsNumber());
        Assert.False(Evaluate("ok").AsBoolean());
    }

    [Fact]
    public void ShouldExposeResponseHeaders()
    {
        MapGet(
            "/headers",
            async ctx =>
            {
                ctx.Response.AddHeader("X-Custom-Header", "custom-value");
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            var headerValue;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}headers');
                headerValue = res.headers.get('X-Custom-Header');
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("custom-value", Evaluate("headerValue").AsString());
    }

    [Fact]
    public void ShouldReturnResponseAsArrayBuffer()
    {
        MapGet(
            "/binary",
            async ctx => await WriteResponseAsync(ctx, 200, [0x01, 0x02, 0x03, 0x04])
        );

        Execute(
            $$"""
            var byteLength;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}binary');
                const buf = await res.arrayBuffer();
                byteLength = buf.byteLength;
                done = true;
            }
            run();
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(4, Evaluate("byteLength").AsNumber());
    }

    [Fact]
    public void ShouldReturnResponseAsBlob()
    {
        MapGet("/blob", async ctx => await WriteResponseAsync(ctx, 200, "blob content"));

        Execute(
            $$"""
            var blobSize;
            var blobType;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}blob');
                const b = await res.blob();
                blobSize = b.size;
                blobType = b.type;
                done = true;
            }
            run();
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(12, Evaluate("blobSize").AsNumber());
    }

    [Fact]
    public void ShouldReturnResponseAsBytes()
    {
        MapGet("/bytes", async ctx => await WriteResponseAsync(ctx, 200, "abc"));

        Execute(
            $$"""
            var bytesResult;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}bytes');
                bytesResult = await res.bytes();
                done = true;
            }
            run();
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("bytesResult instanceof Uint8Array").AsBoolean());
        Assert.Equal(3, Evaluate("bytesResult.length").AsNumber());
        Assert.Equal(97, Evaluate("bytesResult[0]").AsNumber());
    }

    [Fact]
    public void ShouldFetchWithEmptyBody()
    {
        MapGet(
            "/empty",
            ctx =>
            {
                ctx.Response.StatusCode = 204;
                ctx.Response.Close();
                return Task.CompletedTask;
            }
        );

        Execute(
            $$"""
            var status;
            var bodyIsNull;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}empty');
                status = res.status;
                bodyIsNull = res.body === null;
                done = true;
            }
            run();
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(204, Evaluate("status").AsNumber());
        Assert.True(Evaluate("bodyIsNull").AsBoolean());
    }

    [Fact]
    public void ShouldFetchUrlEncodedFormDataResponse()
    {
        MapGet(
            "/form-data-urlencoded",
            async ctx =>
                await WriteResponseAsync(
                    ctx,
                    200,
                    "name=Yilduz&version=1&spaced=hello+world",
                    "application/x-www-form-urlencoded"
                )
        );

        Execute(
            $$"""
            var name;
            var version;
            var spaced;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}form-data-urlencoded');
                const fd = await res.formData();
                name = fd.get('name');
                version = fd.get('version');
                spaced = fd.get('spaced');
                done = true;
            }
            run();
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.Equal("Yilduz", Evaluate("name").AsString());
        Assert.Equal("1", Evaluate("version").AsString());
        Assert.Equal("hello world", Evaluate("spaced").AsString());
    }

    [Fact]
    public void ShouldFetchMultipartFormDataResponse()
    {
        var boundary = "----WebKitFormBoundary7MA4YWxkTrZu0gW";
        var body = $"""
            --{boundary}
            Content-Disposition: form-data; name="username"

            admin
            --{boundary}
            Content-Disposition: form-data; name="info"; filename="info.txt"
            Content-Type: text/plain

            binarydata
            --{boundary}--
            """.Replace("\r\n", "\n").Replace("\n", "\r\n");

        MapGet(
            "/form-data-multipart",
            async ctx =>
                await WriteResponseAsync(
                    ctx,
                    200,
                    body,
                    $"multipart/form-data; boundary={boundary}"
                )
        );

        Execute(
            $$"""
            var username;
            var infoName;
            var infoText;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}form-data-multipart');
                const fd = await res.formData();
                username = fd.get('username');
                
                const file = fd.get('info');
                infoName = file.name;
                infoText = await file.text();
                done = true;
            }
            run();
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.Equal("admin", Evaluate("username").AsString());
        Assert.Equal("info.txt", Evaluate("infoName").AsString());
        Assert.Equal("binarydata", Evaluate("infoText").AsString());
    }
}
