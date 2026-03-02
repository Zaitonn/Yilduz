using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.XMLHttpRequest;

public sealed class JsonResponseTests : HttpRouteTestBase
{
    [Fact]
    public async Task ShouldReturnParsedObjectForValidJson()
    {
        MapGet(
            "/xhr-valid-json",
            async ctx =>
                await WriteResponseAsync(
                    ctx,
                    200,
                    """{"success":true,"value":99}""",
                    "application/json"
                )
        );

        Execute(
            $$"""
            var success = null;
            var value = -1;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.responseType = 'json';
            xhr.onload = function() {
                success = xhr.response.success;
                value   = xhr.response.value;
                done = true;
            };
            xhr.open('GET', '{{BaseUrl}}xhr-valid-json');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.True(Evaluate("success").AsBoolean());
        Assert.Equal(99, Evaluate("value").AsNumber());
    }

    [Fact]
    public async Task ShouldReturnNullForInvalidJson()
    {
        MapGet(
            "/xhr-invalid-json",
            async ctx =>
                await WriteResponseAsync(ctx, 200, "this is not valid json", "application/json")
        );

        Execute(
            $$"""
            var response = undefined;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.responseType = 'json';
            xhr.onload = function() {
                response = xhr.response;
                done = true;
            };
            xhr.open('GET', '{{BaseUrl}}xhr-invalid-json');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        // Per spec https://xhr.spec.whatwg.org/#the-response-attribute:
        // when JSON parsing fails, response must be null.
        Assert.True(Evaluate("response").IsNull());
    }

    [Fact]
    public async Task ShouldReturnNullForEmptyBody()
    {
        MapGet(
            "/xhr-empty-json",
            async ctx => await WriteResponseAsync(ctx, 200, "", "application/json")
        );

        Execute(
            $$"""
            var response = undefined;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.responseType = 'json';
            xhr.onload = function() {
                response = xhr.response;
                done = true;
            };
            xhr.open('GET', '{{BaseUrl}}xhr-empty-json');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.True(Evaluate("response").IsNull());
    }

    [Fact]
    public async Task ShouldReturnNullWhenBodyHasTrailingGarbage()
    {
        MapGet(
            "/xhr-garbage-json",
            async ctx =>
                await WriteResponseAsync(
                    ctx,
                    200,
                    """{"key":"value"} unexpected trailing garbage""",
                    "application/json"
                )
        );

        Execute(
            $$"""
            var response = undefined;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.responseType = 'json';
            xhr.onload = function() {
                response = xhr.response;
                done = true;
            };
            xhr.open('GET', '{{BaseUrl}}xhr-garbage-json');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.True(Evaluate("response").IsNull());
    }
}
