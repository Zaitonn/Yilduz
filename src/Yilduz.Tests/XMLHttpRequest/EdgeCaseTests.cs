using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.XMLHttpRequest;

public sealed class EdgeCaseTests : HttpRouteTestBase
{
    [Fact]
    public void ShouldThrowWhenSendBeforeOpen()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("const xhr = new XMLHttpRequest(); xhr.send();")
        );
    }

    [Fact]
    public void ShouldThrowWhenSettingHeaderBeforeOpen()
    {
        Assert.Throws<JavaScriptException>(
            () =>
                Execute(
                    "const xhr = new XMLHttpRequest(); xhr.setRequestHeader('X-Test', 'value');"
                )
        );
    }

    [Fact]
    public void ShouldThrowForForbiddenMethod()
    {
        Assert.Throws<JavaScriptException>(
            () =>
                Execute(
                    $$"""const xhr = new XMLHttpRequest(); xhr.open('CONNECT', '{{BaseUrl}}forbidden');"""
                )
        );
    }

    [Fact]
    public void ShouldThrowForInvalidHeaderNameAfterOpen()
    {
        Assert.Throws<JavaScriptException>(
            () =>
                Execute(
                    $$"""
                    const xhr = new XMLHttpRequest();
                    xhr.open('GET', '{{BaseUrl}}bad-header');
                    xhr.setRequestHeader('bad name', 'value');
                    """
                )
        );
    }

    [Fact]
    public void ShouldThrowWhenSettingTimeoutOnSynchronousRequest()
    {
        Assert.Throws<JavaScriptException>(
            () =>
                Execute(
                    $$"""
                    const xhr = new XMLHttpRequest(); 
                    xhr.open('GET', '{{BaseUrl}}timeout', false);
                    xhr.timeout = 100;
                    """
                )
        );
    }

    [Fact]
    public void ShouldThrowWhenSettingWithCredentialsAfterSend()
    {
        MapGet("/with-cred", async ctx => await WriteResponseAsync(ctx, 200, "ok"));

        Assert.Throws<JavaScriptException>(
            () =>
                Execute(
                    $$"""
                    const xhr = new XMLHttpRequest();
                    xhr.open('GET', '{{BaseUrl}}with-cred');
                    xhr.send();
                    xhr.withCredentials = true;
                    """
                )
        );
    }

    [Fact]
    public void ShouldThrowWhenOverrideMimeTypeAfterSend()
    {
        MapGet("/ovmt", async ctx => await WriteResponseAsync(ctx, 200, "ok"));

        Assert.Throws<JavaScriptException>(
            () =>
                Execute(
                    $$"""
                    const xhr = new XMLHttpRequest();
                    xhr.open('GET', '{{BaseUrl}}ovmt');
                    xhr.send();
                    xhr.overrideMimeType('text/plain');
                    """
                )
        );
    }

    [Fact]
    public void ShouldThrowWhenUrlIsMalformed()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("const xhr = new XMLHttpRequest(); xhr.open('GET', 'not a url');")
        );
    }
}
