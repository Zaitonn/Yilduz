using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.XMLHttpRequest;

public sealed class MethodTests : HttpRouteTestBase
{
    [Fact]
    public async Task ShouldSendBodyWithPostRequest()
    {
        string? receivedBody = null;

        MapRoute(
            "POST",
            "/xhr-post-body",
            async ctx =>
            {
                using var reader = new System.IO.StreamReader(ctx.Request.InputStream);
                receivedBody = await reader.ReadToEndAsync();
                await WriteResponseAsync(ctx, 200, "received");
            }
        );

        Execute(
            $$"""
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.onload = function() { done = true; };
            xhr.open('POST', '{{BaseUrl}}xhr-post-body');
            xhr.send('request-payload');
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.Equal("request-payload", receivedBody);
        Assert.Equal("received", Evaluate("xhr.responseText").AsString());
    }

    [Fact]
    public async Task ShouldSendPutRequest()
    {
        string? receivedMethod = null;

        MapRoute(
            "PUT",
            "/xhr-put",
            async ctx =>
            {
                receivedMethod = ctx.Request.HttpMethod;
                await WriteResponseAsync(ctx, 200, "put-ok");
            }
        );

        Execute(
            $$"""
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.onload = function() { done = true; };
            xhr.open('PUT', '{{BaseUrl}}xhr-put');
            xhr.send('put-data');
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.Equal("PUT", receivedMethod);
        Assert.Equal("put-ok", Evaluate("xhr.responseText").AsString());
    }

    [Fact]
    public async Task ShouldSendDeleteRequest()
    {
        string? receivedMethod = null;

        MapRoute(
            "DELETE",
            "/xhr-delete",
            async ctx =>
            {
                receivedMethod = ctx.Request.HttpMethod;
                await WriteResponseAsync(ctx, 204, "");
            }
        );

        Execute(
            $$"""
            var status = -1;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.onload = function() {
                status = xhr.status;
                done = true;
            };
            xhr.open('DELETE', '{{BaseUrl}}xhr-delete');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.Equal("DELETE", receivedMethod);
        Assert.Equal(204, Evaluate("status").AsNumber());
    }

    [Fact]
    public async Task ShouldSendPatchRequest()
    {
        string? receivedMethod = null;
        string? receivedBody = null;

        MapRoute(
            "PATCH",
            "/xhr-patch",
            async ctx =>
            {
                receivedMethod = ctx.Request.HttpMethod;
                using var reader = new System.IO.StreamReader(ctx.Request.InputStream);
                receivedBody = await reader.ReadToEndAsync();
                await WriteResponseAsync(ctx, 200, "patched");
            }
        );

        Execute(
            $$"""
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.onload = function() { done = true; };
            xhr.open('PATCH', '{{BaseUrl}}xhr-patch');
            xhr.send('patch-data');
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.Equal("PATCH", receivedMethod);
        Assert.Equal("patch-data", receivedBody);
        Assert.Equal("patched", Evaluate("xhr.responseText").AsString());
    }

    [Fact]
    public async Task ShouldSendHeadRequestAndReceiveHeaders()
    {
        MapRoute(
            "HEAD",
            "/xhr-head",
            ctx =>
            {
                ctx.Response.StatusCode = 200;
                ctx.Response.Headers.Add("X-Head-Test", "yes");
                ctx.Response.Close();
                return Task.CompletedTask;
            }
        );

        Execute(
            $$"""
            var status = -1;
            var headHeader = null;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.onload = function() {
                status = xhr.status;
                headHeader = xhr.getResponseHeader('X-Head-Test');
                done = true;
            };
            xhr.open('HEAD', '{{BaseUrl}}xhr-head');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.Equal(200, Evaluate("status").AsNumber());
        Assert.Equal("yes", Evaluate("headHeader").AsString());
    }

    [Fact]
    public async Task ShouldNormalizeMethodNameToUppercase()
    {
        string? receivedMethod = null;

        MapRoute(
            "POST",
            "/xhr-method-case",
            async ctx =>
            {
                receivedMethod = ctx.Request.HttpMethod;
                await WriteResponseAsync(ctx, 200, "ok");
            }
        );

        Execute(
            $$"""
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.onload = function() { done = true; };
            xhr.open('post', '{{BaseUrl}}xhr-method-case');
            xhr.send('data');
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.Equal("POST", receivedMethod);
    }
}
