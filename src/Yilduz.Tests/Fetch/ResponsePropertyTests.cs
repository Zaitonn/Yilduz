using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.Fetch;

public sealed class ResponsePropertyTests : FetchTestBase
{
    [Theory]
    [InlineData(200, true)]
    [InlineData(201, true)]
    [InlineData(204, true)]
    [InlineData(299, true)]
    [InlineData(300, false)]
    [InlineData(400, false)]
    [InlineData(404, false)]
    [InlineData(500, false)]
    public void OkShouldReflectStatusCode(int statusCode, bool expectedOk)
    {
        MapGet(
            "/status",
            ctx =>
            {
                ctx.Response.StatusCode = statusCode;
                ctx.Response.Close();
                return Task.CompletedTask;
            }
        );

        Execute(
            $$"""
            var ok;
            var status;
            async function run() {
                const res = await fetch('{{BaseUrl}}status');
                ok = res.ok;
                status = res.status;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(statusCode, (int)Evaluate("status").AsNumber());
        Assert.Equal(expectedOk, Evaluate("ok").AsBoolean());
    }

    [Fact]
    public void ShouldExposeStatusText()
    {
        MapGet("/ok", async ctx => await WriteResponseAsync(ctx, 200, "OK"));

        Execute(
            $$"""
            var statusText;
            async function run() {
                const res = await fetch('{{BaseUrl}}ok');
                statusText = res.statusText;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("OK", Evaluate("statusText").AsString());
    }

    [Fact]
    public void ShouldExposeMultipleResponseHeaders()
    {
        MapGet(
            "/multi-headers",
            async ctx =>
            {
                ctx.Response.AddHeader("X-First", "one");
                ctx.Response.AddHeader("X-Second", "two");
                ctx.Response.AddHeader("X-Third", "three");
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            var h1, h2, h3;
            async function run() {
                const res = await fetch('{{BaseUrl}}multi-headers');
                h1 = res.headers.get('X-First');
                h2 = res.headers.get('X-Second');
                h3 = res.headers.get('X-Third');
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("one", Evaluate("h1").AsString());
        Assert.Equal("two", Evaluate("h2").AsString());
        Assert.Equal("three", Evaluate("h3").AsString());
    }

    [Fact]
    public void ShouldExposeContentTypeHeader()
    {
        MapGet("/ct", async ctx => await WriteResponseAsync(ctx, 200, "{}", "application/json"));

        Execute(
            $$"""
            var ct;
            async function run() {
                const res = await fetch('{{BaseUrl}}ct');
                ct = res.headers.get('content-type');
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Contains("application/json", Evaluate("ct").AsString());
    }

    [Fact]
    public void HeadersShouldSupportHasMethod()
    {
        MapGet(
            "/has-header",
            async ctx =>
            {
                ctx.Response.AddHeader("X-Present", "yes");
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            var hasPresent, hasMissing;
            async function run() {
                const res = await fetch('{{BaseUrl}}has-header');
                hasPresent = res.headers.has('X-Present');
                hasMissing = res.headers.has('X-Missing');
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("hasPresent").AsBoolean());
        Assert.False(Evaluate("hasMissing").AsBoolean());
    }

    [Fact]
    public void UrlShouldMatchRequestUrl()
    {
        MapGet("/url-prop", async ctx => await WriteResponseAsync(ctx, 200, "ok"));

        Execute(
            $$"""
            var url;
            async function run() {
                const res = await fetch('{{BaseUrl}}url-prop');
                url = res.url;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        var responseUrl = Evaluate("url").AsString();
        Assert.StartsWith(BaseUrl, responseUrl);
        Assert.Contains("url-prop", responseUrl);
    }

    [Fact]
    public void TypeShouldBeDefault()
    {
        MapGet("/type-check", async ctx => await WriteResponseAsync(ctx, 200, "ok"));

        Execute(
            $$"""
            var type;
            async function run() {
                const res = await fetch('{{BaseUrl}}type-check');
                type = res.type;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("default", Evaluate("type").AsString());
    }

    [Fact]
    public void RedirectedShouldBeFalseForDirectRequest()
    {
        MapGet("/direct", async ctx => await WriteResponseAsync(ctx, 200, "ok"));

        Execute(
            $$"""
            var redirected;
            async function run() {
                const res = await fetch('{{BaseUrl}}direct');
                redirected = res.redirected;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.False(Evaluate("redirected").AsBoolean());
    }

    [Fact]
    public void BodyUsedShouldBeFalseBeforeConsuming()
    {
        MapGet("/body-used", async ctx => await WriteResponseAsync(ctx, 200, "data"));

        Execute(
            $$"""
            var bodyUsedBefore, bodyUsedAfter;
            async function run() {
                const res = await fetch('{{BaseUrl}}body-used');
                bodyUsedBefore = res.bodyUsed;
                await res.text();
                bodyUsedAfter = res.bodyUsed;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.False(Evaluate("bodyUsedBefore").AsBoolean());
        Assert.True(Evaluate("bodyUsedAfter").AsBoolean());
    }

    [Fact]
    public void BodyShouldBeReadableStream()
    {
        MapGet("/readable", async ctx => await WriteResponseAsync(ctx, 200, "stream data"));

        Execute(
            $$"""
            var isReadable;
            async function run() {
                const res = await fetch('{{BaseUrl}}readable');
                isReadable = res.body instanceof ReadableStream;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("isReadable").AsBoolean());
    }

    [Fact]
    public void CloneShouldReturnIndependentResponse()
    {
        MapGet("/clone-test", async ctx => await WriteResponseAsync(ctx, 200, "clone me"));

        Execute(
            $$"""
            var text1, text2;
            async function run() {
                const res = await fetch('{{BaseUrl}}clone-test');
                const cloned = res.clone();
                text1 = await res.text();
                text2 = await cloned.text();
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("clone me", Evaluate("text1").AsString());
        Assert.Equal("clone me", Evaluate("text2").AsString());
    }

    [Fact]
    public void CloneShouldPreserveStatus()
    {
        MapGet(
            "/clone-status",
            ctx =>
            {
                ctx.Response.StatusCode = 201;
                ctx.Response.Close();
                return Task.CompletedTask;
            }
        );

        Execute(
            $$"""
            var originalStatus, clonedStatus;
            async function run() {
                const res = await fetch('{{BaseUrl}}clone-status');
                const cloned = res.clone();
                originalStatus = res.status;
                clonedStatus = cloned.status;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(201, (int)Evaluate("originalStatus").AsNumber());
        Assert.Equal(201, (int)Evaluate("clonedStatus").AsNumber());
    }

    [Fact]
    public void ShouldThrowWhenBodyConsumedTwice()
    {
        MapGet("/double-read", async ctx => await WriteResponseAsync(ctx, 200, "once"));

        Execute(
            $$"""
            var errorCaught = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}double-read');
                await res.text();
                try {
                    await res.text();
                } catch (e) {
                    errorCaught = true;
                }
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("errorCaught").AsBoolean());
    }

    [Fact]
    public void ResponseShouldBeInstanceOfResponse()
    {
        MapGet("/instanceof", async ctx => await WriteResponseAsync(ctx, 200, "ok"));

        Execute(
            $$"""
            var isResponse;
            async function run() {
                const res = await fetch('{{BaseUrl}}instanceof');
                isResponse = res instanceof Response;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("isResponse").AsBoolean());
    }

    [Fact]
    public void ShouldConsumeBodyAsJson()
    {
        MapGet(
            "/json-response",
            async ctx =>
                await WriteResponseAsync(
                    ctx,
                    200,
                    """{"items":[1,2,3],"nested":{"key":"val"}}""",
                    "application/json"
                )
        );

        Execute(
            $$"""
            var result;
            async function run() {
                const res = await fetch('{{BaseUrl}}json-response');
                result = await res.json();
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(3, Evaluate("result.items.length").AsNumber());
        Assert.Equal(1, Evaluate("result.items[0]").AsNumber());
        Assert.Equal("val", Evaluate("result.nested.key").AsString());
    }

    [Fact]
    public void ShouldConsumeBodyAsArrayBuffer()
    {
        MapGet("/ab-response", async ctx => await WriteResponseAsync(ctx, 200, "hello"));

        Execute(
            $$"""
            var byteLength;
            var firstByte;
            async function run() {
                const res = await fetch('{{BaseUrl}}ab-response');
                const buf = await res.arrayBuffer();
                byteLength = buf.byteLength;
                firstByte = new Uint8Array(buf)[0];
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(5, Evaluate("byteLength").AsNumber());
        Assert.Equal(104, Evaluate("firstByte").AsNumber()); // 'h'
    }

    [Fact]
    public void ShouldConsumeBodyAsBlob()
    {
        MapGet("/blob-response", async ctx => await WriteResponseAsync(ctx, 200, "blob!"));

        Execute(
            $$"""
            var blobSize;
            async function run() {
                const res = await fetch('{{BaseUrl}}blob-response');
                const b = await res.blob();
                blobSize = b.size;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(5, Evaluate("blobSize").AsNumber());
    }

    [Fact]
    public void ShouldConsumeBodyAsBytes()
    {
        MapGet("/bytes-response", async ctx => await WriteResponseAsync(ctx, 200, "AB"));

        Execute(
            $$"""
            var len, first, second;
            async function run() {
                const res = await fetch('{{BaseUrl}}bytes-response');
                const b = await res.bytes();
                len = b.length;
                first = b[0];
                second = b[1];
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(2, Evaluate("len").AsNumber());
        Assert.Equal(65, Evaluate("first").AsNumber()); // 'A'
        Assert.Equal(66, Evaluate("second").AsNumber()); // 'B'
    }
}
