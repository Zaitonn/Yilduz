using Jint;
using Xunit;

namespace Yilduz.Tests.Fetch;

public sealed class InvalidInputTests : FetchTestBase
{
    [Theory]
    [InlineData("not-a-url")]
    [InlineData("://missing-scheme")]
    [InlineData("")]
    public void ShouldRejectWhenUrlIsMalformed(string url)
    {
        Execute(
            $$"""
            var caught = false;
            var errorType;
            async function run() {
                try {
                    await fetch('{{url}}');
                } catch (e) {
                    caught = true;
                    errorType = e.constructor.name;
                }
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("caught").AsBoolean());
        Assert.Equal("TypeError", Evaluate("errorType").AsString());
    }

    [Theory]
    [InlineData("CONNECT")]
    [InlineData("TRACE")]
    [InlineData("TRACK")]
    public void ShouldRejectWhenMethodIsForbidden(string method)
    {
        Execute(
            $$"""
            var caught = false;
            var errorType;
            async function run() {
                try {
                    await fetch('https://example.com', { method: '{{method}}' });
                } catch (e) {
                    caught = true;
                    errorType = e.constructor.name;
                }
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("caught").AsBoolean());
        Assert.Equal("TypeError", Evaluate("errorType").AsString());
    }

    [Fact]
    public void ShouldRejectWhenBodyIsUsedWithGet()
    {
        Execute(
            $$"""
            var caught = false;
            async function run() {
                try {
                    await fetch('{{BaseUrl}}', { method: 'GET', body: 'data' });
                } catch (e) {
                    caught = true;
                }
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("caught").AsBoolean());
    }

    [Fact]
    public void ShouldRejectWhenBodyIsUsedWithHead()
    {
        Execute(
            $$"""
            var caught = false;
            async function run() {
                try {
                    await fetch('{{BaseUrl}}', { method: 'HEAD', body: 'data' });
                } catch (e) {
                    caught = true;
                }
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("caught").AsBoolean());
    }

    [Fact]
    public void ShouldRejectWhenUrlContainsCredentials()
    {
        Execute(
            """
            var caught = false;
            async function run() {
                try {
                    await fetch('https://user:pass@example.com');
                } catch (e) {
                    caught = true;
                }
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("caught").AsBoolean());
    }

    [Fact]
    public void ShouldRejectWhenServerIsUnreachable()
    {
        Execute(
            """
            var caught = false;
            var isTypeError = false;
            async function run() {
                try {
                    await fetch('http://localhost:1/unreachable');
                } catch (e) {
                    caught = true;
                    isTypeError = e instanceof TypeError;
                }
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("caught").AsBoolean());
        Assert.True(Evaluate("isTypeError").AsBoolean());
    }

    [Fact]
    public void ShouldRejectWhenRedirectModeIsErrorAndServerRedirects()
    {
        MapGet(
            "/redir",
            ctx =>
            {
                ctx.Response.StatusCode = 301;
                ctx.Response.RedirectLocation = BaseUrl + "target";
                ctx.Response.Close();
                return System.Threading.Tasks.Task.CompletedTask;
            }
        );

        Execute(
            $$"""
            var caught = false;
            async function run() {
                try {
                    await fetch('{{BaseUrl}}redir', { redirect: 'error' });
                } catch (e) {
                    caught = true;
                }
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("caught").AsBoolean());
    }
}
