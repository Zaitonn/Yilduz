using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.XMLHttpRequest;

public sealed class RuntimeTests : HttpRouteTestBase
{
    [Fact]
    public async Task ShouldCompleteGetRequestAndPopulateResponse()
    {
        MapGet("/xhr-basic", async ctx => await WriteResponseAsync(ctx, 200, "hello world"));

        Execute(
            $$"""
            var body = null;
            var status = -1;
            var responseUrl = null;
            var finalState = -1;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function() {
                if (xhr.readyState === 4) {
                    body = xhr.responseText;
                    status = xhr.status;
                    responseUrl = xhr.responseURL;
                    finalState = xhr.readyState;
                    done = true;
                }
            };
            xhr.open('GET', '{{BaseUrl}}xhr-basic');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.Equal("hello world", Evaluate("body").AsString());
        Assert.Equal(200, Evaluate("status"));

        var url = Evaluate("responseUrl").AsString();
        Assert.Contains("xhr-basic", url);
        Assert.Equal(4, Evaluate("finalState"));
    }

    [Fact]
    public async Task ShouldSendHeadersAndReadResponseHeaders()
    {
        string? receivedHeader = null;

        MapRoute(
            "POST",
            "/xhr-headers",
            async ctx =>
            {
                receivedHeader = ctx.Request.Headers["X-Test"];
                ctx.Response.AddHeader("X-Resp", "test");
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            var respHeader = null;
            var allHeaders = null;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.onload = function() {
                respHeader = xhr.getResponseHeader('X-Resp');
                allHeaders = xhr.getAllResponseHeaders();
                done = true;
            };
            xhr.open('POST', '{{BaseUrl}}xhr-headers');
            xhr.setRequestHeader('X-Test', 'client-header');
            xhr.send('payload');
            """
        );

        await WaitForJsConditionAsync("done === true");
        Assert.Equal("client-header", receivedHeader);
        Assert.Equal("test", Evaluate("respHeader").AsString());
        Assert.Contains("x-resp: test", Evaluate("allHeaders").AsString());
    }

    [Fact]
    public async Task ShouldTrackReadyStateTransitions()
    {
        MapGet("/xhr-states", async ctx => await WriteResponseAsync(ctx, 200, "stateful"));

        Execute(
            $$"""
            var states = [];
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function() {
                states.push(xhr.readyState);
                if (xhr.readyState === 4) {
                    done = true;
                }
            };
            xhr.open('GET', '{{BaseUrl}}xhr-states');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        var statesArray = Evaluate("states").AsArray();
        Assert.Equal([1, 2, 3, 4], statesArray);
    }

    [Fact]
    public async Task ShouldAbortPendingRequest()
    {
        MapGet(
            "/xhr-slow",
            async ctx =>
            {
                await Task.Delay(200, Token);
                await WriteResponseAsync(ctx, 200, "slow response");
            }
        );

        Execute(
            $$"""
            var aborted = false;
            var readyStateAfterAbort = -1;
            const xhr = new XMLHttpRequest();
            xhr.onabort = function() {
                aborted = true;
                readyStateAfterAbort = xhr.readyState;
            };
            xhr.open('GET', '{{BaseUrl}}xhr-slow');
            xhr.send();
            setTimeout(() => xhr.abort(), 10);
            """
        );

        await WaitForJsConditionAsync("aborted === true");
        Assert.True(Evaluate("aborted").AsBoolean());
        Assert.Equal(4, Evaluate("readyStateAfterAbort"));
        Assert.Equal(0, Evaluate("xhr.status"));
    }

    [Fact]
    public async Task ShouldFireErrorOnNetworkFailure()
    {
        MapGet(
            "/xhr-connection-abort",
            ctx =>
            {
                ctx.Response.Abort();
                return Task.CompletedTask;
            }
        );

        Execute(
            $$"""
            var errorFired = false;
            var loadFired = false;
            var ready = -1;
            const xhr = new XMLHttpRequest();
            xhr.onerror = function() {
                errorFired = true;
                ready = xhr.readyState;
            };
            xhr.onload = function() { loadFired = true; };
            xhr.open('GET', '{{BaseUrl}}xhr-connection-abort');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("errorFired === true");

        Assert.True(Evaluate("errorFired").AsBoolean());
        Assert.False(Evaluate("loadFired").AsBoolean());
        Assert.Equal(4, Evaluate("ready"));
        Assert.Equal(0, Evaluate("xhr.status"));
    }

    [Fact]
    public async Task ShouldFollowRedirectsAndExposeFinalResponse()
    {
        MapGet("/xhr-final", async ctx => await WriteResponseAsync(ctx, 200, "redirected"));
        MapGet(
            "/xhr-redirect",
            ctx =>
            {
                ctx.Response.StatusCode = 302;
                ctx.Response.RedirectLocation = BaseUrl + "xhr-final";
                ctx.Response.Close();
                return Task.CompletedTask;
            }
        );

        Execute(
            $$"""
            var body = null;
            var status = -1;
            var url = null;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function() {
                if (xhr.readyState === 4) {
                    body = xhr.responseText;
                    status = xhr.status;
                    url = xhr.responseURL;
                    done = true;
                }
            };
            xhr.open('GET', '{{BaseUrl}}xhr-redirect');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.Equal("redirected", Evaluate("body").AsString());
        Assert.Equal(200, Evaluate("status"));
        Assert.Contains("xhr-final", Evaluate("url").AsString());
    }

    [Fact]
    public async Task ShouldHonorOverrideMimeTypeBeforeSend()
    {
        MapGet(
            "/xhr-ovmt",
            async ctx => await WriteResponseAsync(ctx, 200, "override-body", "application/json")
        );

        Execute(
            $$"""
            var text = null;
            var status = -1;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.onreadystatechange = function() {
                if (xhr.readyState === 4) {
                    text = xhr.responseText;
                    status = xhr.status;
                    done = true;
                }
            };
            xhr.open('GET', '{{BaseUrl}}xhr-ovmt');
            xhr.overrideMimeType('text/plain');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.Equal("override-body", Evaluate("text").AsString());
        Assert.Equal(200, Evaluate("status"));
    }

    [Fact]
    public void ShouldThrowOverrideMimeTypeAfterSend()
    {
        MapGet("/xhr-ovmt-after", async ctx => await WriteResponseAsync(ctx, 200, "ok"));

        Execute(
            $$"""
            var errorCaught = false;
            async function run() {
                const xhr = new XMLHttpRequest();
                xhr.open('GET', '{{BaseUrl}}xhr-ovmt-after');
                xhr.send();
                try {
                    xhr.overrideMimeType('text/plain');
                } catch (e) {
                    errorCaught = true;
                }
            }
            """
        );

        Evaluate("run()\n").UnwrapIfPromise();
        Assert.True(Evaluate("errorCaught").AsBoolean());
    }

    [Fact]
    public async Task ShouldExposeArrayBufferResponseWhenResponseTypeIsArrayBuffer()
    {
        MapGet(
            "/xhr-ab",
            async ctx => await WriteResponseAsync(ctx, 200, "Hello", "application/octet-stream")
        );

        Execute(
            $$"""
            var isArrayBuffer = false;
            var byteLength = -1;
            var firstByte = -1;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.responseType = 'arraybuffer';
            xhr.onreadystatechange = function() {
                if (xhr.readyState === 4) {
                    const ab = xhr.response;
                    isArrayBuffer = ab instanceof ArrayBuffer;
                    byteLength = ab.byteLength;
                    firstByte = new Uint8Array(ab)[0];
                    done = true;
                }
            };
            xhr.open('GET', '{{BaseUrl}}xhr-ab');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.True(Evaluate("isArrayBuffer").AsBoolean());
        Assert.Equal(5, Evaluate("byteLength")); // "Hello"
        Assert.Equal(72, Evaluate("firstByte")); // 'H'
    }

    [Fact]
    public async Task ShouldExposeBlobResponseWhenResponseTypeIsBlob()
    {
        MapGet(
            "/xhr-blob",
            async ctx => await WriteResponseAsync(ctx, 200, "blob content", "text/plain")
        );

        Execute(
            $$"""
            var isBlob = false;
            var blobSize = -1;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.responseType = 'blob';
            xhr.onreadystatechange = function() {
                if (xhr.readyState === 4) {
                    const b = xhr.response;
                    isBlob = b instanceof Blob;
                    blobSize = b.size;
                    done = true;
                }
            };
            xhr.open('GET', '{{BaseUrl}}xhr-blob');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.True(Evaluate("isBlob").AsBoolean());
        Assert.Equal(12, Evaluate("blobSize")); // "blob content"
    }

    [Fact]
    public async Task ShouldExposeParsedJsonWhenResponseTypeIsJson()
    {
        MapGet(
            "/xhr-json",
            async ctx =>
                await WriteResponseAsync(
                    ctx,
                    200,
                    """{"name":"Yilduz","count":42}""",
                    "application/json"
                )
        );

        Execute(
            $$"""
            var name = null;
            var count = -1;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.responseType = 'json';
            xhr.onreadystatechange = function() {
                if (xhr.readyState === 4) {
                    const json = xhr.response;
                    name = json.name;
                    count = json.count;
                    done = true;
                }
            };
            xhr.open('GET', '{{BaseUrl}}xhr-json');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.Equal("Yilduz", Evaluate("name").AsString());
        Assert.Equal(42, Evaluate("count"));
    }

    [Fact]
    public async Task ShouldThrowWhenAccessingResponseTextWithNonTextResponseType()
    {
        MapGet(
            "/xhr-rt-guard",
            async ctx => await WriteResponseAsync(ctx, 200, "data", "application/octet-stream")
        );

        Execute(
            $$"""
            var errorCaught = false;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.responseType = 'arraybuffer';
            xhr.onreadystatechange = function() {
                if (xhr.readyState === 4) {
                    try {
                        const _ = xhr.responseText;
                    } catch (e) {
                        errorCaught = true;
                    }
                    done = true;
                }
            };
            xhr.open('GET', '{{BaseUrl}}xhr-rt-guard');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.True(Evaluate("errorCaught").AsBoolean());
    }
}
