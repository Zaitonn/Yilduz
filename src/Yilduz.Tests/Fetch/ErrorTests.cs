using Jint;
using Xunit;

namespace Yilduz.Tests.Fetch;

public sealed class ErrorTests : FetchTestBase
{
    [Fact]
    public void ShouldRejectOnNetworkError()
    {
        // Use a port that is definitely not listening.
        Execute(
            """
            var errorCaught = false;
            var errorMessage;
            async function run() {
                try {
                    await fetch('http://localhost:1/does-not-exist');
                } catch (e) {
                    errorCaught = true;
                    errorMessage = e.message;
                }
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("errorCaught").AsBoolean());
    }

    [Fact]
    public void ShouldRejectWithTypeErrorOnNetworkError()
    {
        Execute(
            """
            var isTypeError = false;
            async function run() {
                try {
                    await fetch('http://localhost:1/fail');
                } catch (e) {
                    isTypeError = e instanceof TypeError;
                }
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("isTypeError").AsBoolean());
    }

    [Fact]
    public void ShouldHandle500ServerError()
    {
        MapGet("/error", async ctx => await WriteResponseAsync(ctx, 500, "Internal Server Error"));

        Execute(
            $$"""
            var status;
            var ok;
            var bodyText;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}error');
                status = res.status;
                ok = res.ok;
                bodyText = await res.text();
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(500, Evaluate("status").AsNumber());
        Assert.False(Evaluate("ok").AsBoolean());
        Assert.Equal("Internal Server Error", Evaluate("bodyText").AsString());
    }

    [Fact]
    public void ShouldRejectOnInvalidUrl()
    {
        Execute(
            """
            var errorCaught = false;
            async function run() {
                try {
                    await fetch('not-a-valid-url');
                } catch (e) {
                    errorCaught = true;
                }
            }

            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("errorCaught").AsBoolean());
    }
}
