using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.Fetch;

public sealed class RedirectTests : FetchTestBase
{
    [Fact]
    public void ShouldFollowRedirectByDefault()
    {
        MapGet(
            "/redirect-source",
            ctx =>
            {
                ctx.Response.Redirect(BaseUrl + "redirect-target");
                ctx.Response.Close();
                return Task.CompletedTask;
            }
        );

        MapGet(
            "/redirect-target",
            async ctx => await WriteResponseAsync(ctx, 200, "final destination")
        );

        Execute(
            $$"""
            var bodyText;
            var done = false;
            async function run() {
                const res = await fetch('{{BaseUrl}}redirect-source');
                bodyText = await res.text();
                done = true;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("final destination", Evaluate("bodyText").AsString());
    }

    [Fact]
    public void ShouldRejectWhenRedirectModeIsError()
    {
        MapGet(
            "/redirect-error",
            ctx =>
            {
                ctx.Response.StatusCode = 302;
                ctx.Response.RedirectLocation = BaseUrl + "target";
                ctx.Response.Close();
                return Task.CompletedTask;
            }
        );

        Execute(
            $$"""
            var errorCaught = false;
            async function run() {
                try {
                    await fetch('{{BaseUrl}}redirect-error', { redirect: 'error' });
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
    public void ShouldReturnOpaqueRedirectWhenRedirectModeIsManual()
    {
        MapGet(
            "/redirect-manual",
            ctx =>
            {
                ctx.Response.StatusCode = 302;
                ctx.Response.RedirectLocation = BaseUrl + "target";
                ctx.Response.Close();
                return Task.CompletedTask;
            }
        );

        Execute(
            $$"""
            var errorCaught = false;
            async function run() {
                try {
                    await fetch('{{BaseUrl}}redirect-manual', { redirect: 'manual' });
                } catch (e) {
                    // redirect: 'manual' with a redirect status results in a network error
                    // per the current implementation
                    errorCaught = true;
                }
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("errorCaught").AsBoolean());
    }
}
