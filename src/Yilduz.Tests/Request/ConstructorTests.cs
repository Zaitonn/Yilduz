using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Request;

public sealed class ConstructorTests : TestBase
{
    [Theory]
    [InlineData("not a url")]
    [InlineData("://missing-scheme")]
    [InlineData("")]
    public void ShouldThrowWhenUrlStringIsMalformed(string url)
    {
        Assert.Throws<JavaScriptException>(() => Execute($"new Request('{url}');"));
    }

    [Theory]
    [InlineData("https://user@example.com")]
    [InlineData("https://user:pass@example.com")]
    public void ShouldThrowWhenUrlContainsCredentials(string url)
    {
        Assert.Throws<JavaScriptException>(() => Execute($"new Request('{url}');"));
    }

    [Fact]
    public void ShouldCreateFromAnotherRequest()
    {
        Execute(
            """
            const original = new Request('https://example.com/resource', {
                method: 'POST',
                headers: { 'X-Src': 'yes' },
                duplex: 'half',
                body: 'hello'
            });
            const copy = new Request(original);
            """
        );

        Assert.Equal("POST", Evaluate("copy.method").AsString());
        Assert.Equal("https://example.com/resource", Evaluate("copy.url").AsString());
        Assert.Equal("yes", Evaluate("copy.headers.get('x-src')").AsString());
    }

    [Fact]
    public void ShouldThrowWhenInputIsNeitherStringNorRequest()
    {
        Assert.Throws<JavaScriptException>(() => Execute("new Request(42);"));
        Assert.Throws<JavaScriptException>(() => Execute("new Request({});"));
        Assert.Throws<JavaScriptException>(() => Execute("new Request(null);"));
    }

    [Fact]
    public void ShouldThrowWhenInitModeIsNavigate()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("new Request('https://example.com', { mode: 'navigate' });")
        );
    }

    [Fact]
    public void ShouldSetReferrerToNoReferrerWhenReferrerIsEmptyString()
    {
        Execute("const req = new Request('https://example.com', { referrer: '' });");

        Assert.Equal(string.Empty, Evaluate("req.referrer").AsString());
    }

    [Fact]
    public void ShouldSetReferrerWhenValidAbsoluteUrl()
    {
        Execute(
            "const req = new Request('https://example.com', { referrer: 'https://referrer.example.com/path' });"
        );

        var referrer = Evaluate("req.referrer").AsString();
        Assert.Equal("https://referrer.example.com/path", referrer);
    }

    [Fact]
    public void ShouldSetReferrerToClientWhenReferrerIsAboutClient()
    {
        Execute("const req = new Request('https://example.com', { referrer: 'about:client' });");

        Assert.Equal("about:client", Evaluate("req.referrer").AsString());
    }

    [Fact]
    public void ShouldThrowWhenReferrerUrlIsMalformed()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("new Request('https://example.com', { referrer: 'not a valid url' });")
        );
    }

    [Fact]
    public void ShouldThrowWhenOnlyIfCachedWithNonSameOriginMode()
    {
        Assert.Throws<JavaScriptException>(
            () =>
                Execute(
                    "new Request('https://example.com', { cache: 'only-if-cached', mode: 'cors' });"
                )
        );
        Assert.Throws<JavaScriptException>(
            () =>
                Execute(
                    "new Request('https://example.com', { cache: 'only-if-cached', mode: 'no-cors' });"
                )
        );
    }

    [Fact]
    public void ShouldAllowOnlyIfCachedWithSameOriginMode()
    {
        Execute(
            "const req = new Request('https://example.com', { cache: 'only-if-cached', mode: 'same-origin' });"
        );

        Assert.Equal("only-if-cached", Evaluate("req.cache").AsString());
        Assert.Equal("same-origin", Evaluate("req.mode").AsString());
    }

    [Theory]
    [InlineData("GET method")]
    [InlineData("POST\tmethod")]
    [InlineData("DELETE\nmethod")]
    [InlineData(" ")]
    public void ShouldThrowWhenMethodContainsInvalidCharacters(string method)
    {
        Assert.Throws<JavaScriptException>(
            () => Execute($"new Request('https://example.com', {{ method: '{method}' }});")
        );
    }

    [Theory]
    [InlineData("CONNECT")]
    [InlineData("TRACE")]
    [InlineData("TRACK")]
    [InlineData("connect")]
    [InlineData("trace")]
    public void ShouldThrowWhenMethodIsForbidden(string method)
    {
        Assert.Throws<JavaScriptException>(
            () => Execute($"new Request('https://example.com', {{ method: '{method}' }});")
        );
    }

    [Theory]
    [InlineData("get", "GET")]
    [InlineData("post", "POST")]
    [InlineData("delete", "DELETE")]
    [InlineData("PUT", "PUT")]
    [InlineData("Patch", "PATCH")]
    public void ShouldNormalizeMethodToUpperCase(string input, string expected)
    {
        Execute($"const req = new Request('https://example.com', {{ method: '{input}' }});");

        Assert.Equal(expected, Evaluate("req.method").AsString());
    }

    [Fact]
    public void ShouldCreateWithHeadersInstanceInInit()
    {
        Execute(
            """
            const h = new Headers();
            h.set('X-Custom', 'value1');
            h.set('Accept', 'application/json');
            const req = new Request('https://example.com', { headers: h });
            """
        );

        Assert.Equal("value1", Evaluate("req.headers.get('x-custom')").AsString());
        Assert.Equal("application/json", Evaluate("req.headers.get('accept')").AsString());
    }

    [Fact]
    public void ShouldOverrideInputRequestHeadersWithInitHeaders()
    {
        Execute(
            """
            const original = new Request('https://example.com', { headers: { 'X-A': 'original' } });
            const modified = new Request(original, { headers: { 'X-A': 'overridden', 'X-B': 'new' } });
            """
        );

        Assert.Equal("overridden", Evaluate("modified.headers.get('x-a')").AsString());
        Assert.Equal("new", Evaluate("modified.headers.get('x-b')").AsString());
    }

    [Fact]
    public void ShouldThrowWhenInitWindowIsNonNull()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("new Request('https://example.com', { window: 'client' });")
        );
        Assert.Throws<JavaScriptException>(
            () => Execute("new Request('https://example.com', { window: {} });")
        );
    }

    [Fact]
    public void ShouldHaveNullBodyAndFalseBodyUsedWhenNoBody()
    {
        Execute("const req = new Request('https://example.com');");

        Assert.True(Evaluate("req.body === null").AsBoolean());
        Assert.False(Evaluate("req.bodyUsed").AsBoolean());
    }

    [Fact]
    public void ShouldExposeBodyAsReadableStream()
    {
        Execute(
            """
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'hello world'
            });
            """
        );

        Assert.True(Evaluate("req.body instanceof ReadableStream").AsBoolean());
        Assert.False(Evaluate("req.bodyUsed").AsBoolean());
    }

    [Fact]
    public void ShouldLockBodyStreamAfterConsuming()
    {
        Execute(
            """
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'consume me'
            });
            """
        );

        Assert.Equal("consume me", Evaluate("req.text()").UnwrapIfPromise());
    }

    [Fact]
    public void ShouldThrowWhenDuplexMissingForStringBodyWithNullSource()
    {
        Execute(
            """
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'data'
            });
            """
        );

        Assert.Equal("half", Evaluate("req.duplex").AsString());
    }

    [Fact]
    public void ShouldCreateWithDefaults()
    {
        Execute("const req = new Request('https://example.com');");

        Assert.Equal("Request", Evaluate("req.constructor.name"));
        Assert.Equal("[object Request]", Evaluate("Object.prototype.toString.call(req)"));
        Assert.Equal("GET", Evaluate("req.method"));
        Assert.Equal("https://example.com/", Evaluate("req.url"));
        Assert.Equal("cors", Evaluate("req.mode"));
        Assert.Equal("same-origin", Evaluate("req.credentials"));
        Assert.Equal("default", Evaluate("req.cache"));
        Assert.Equal("follow", Evaluate("req.redirect"));
        Assert.Equal("about:client", Evaluate("req.referrer"));
        Assert.True(Evaluate("req.headers instanceof Headers").AsBoolean());
        Assert.False(Evaluate("req.keepalive").AsBoolean());
        Assert.False(Evaluate("req.bodyUsed").AsBoolean());
    }

    [Fact]
    public void ShouldApplyInitOptions()
    {
        Execute(
            """
            const controller = new AbortController();
            const req = new Request('https://example.com/data', {
                method: 'POST',
                headers: { 'X-Test': 'abc' },
                referrer: '',
                referrerPolicy: 'no-referrer',
                mode: 'same-origin',
                credentials: 'include',
                cache: 'reload',
                redirect: 'error',
                integrity: 'sha256-abc',
                keepalive: true,
                signal: controller.signal,
                duplex: 'half',
                body: 'payload'
            });
            """
        );

        Assert.Equal("POST", Evaluate("req.method"));
        Assert.Equal("https://example.com/data", Evaluate("req.url"));
        Assert.Equal(string.Empty, Evaluate("req.referrer"));
        Assert.Equal("no-referrer", Evaluate("req.referrerPolicy"));
        Assert.Equal("same-origin", Evaluate("req.mode"));
        Assert.Equal("include", Evaluate("req.credentials"));
        Assert.Equal("reload", Evaluate("req.cache"));
        Assert.Equal("error", Evaluate("req.redirect"));
        Assert.Equal("sha256-abc", Evaluate("req.integrity"));
        Assert.True(Evaluate("req.keepalive").AsBoolean());
        Assert.Equal("half", Evaluate("req.duplex"));
        Assert.True(Evaluate("req.body instanceof ReadableStream").AsBoolean());
        Assert.Equal("abc", Evaluate("req.headers.get('x-test')"));
        Assert.True(Evaluate("req.signal instanceof AbortSignal").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenNoCorsUnsafeMethod()
    {
        Assert.Throws<JavaScriptException>(
            () =>
                Execute(
                    "new Request('https://example.com', { mode: 'no-cors', method: 'DELETE' });"
                )
        );
    }

    [Fact]
    public void ShouldThrowWhenBodyWithGetOrHead()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("new Request('https://example.com', { method: 'GET', body: 'x' });")
        );

        Assert.Throws<JavaScriptException>(
            () => Execute("new Request('https://example.com', { method: 'HEAD', body: 'x' });")
        );
    }

    [Fact]
    public void ShouldThrowWhenSignalIsInvalid()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("new Request('https://example.com', { signal: {} });")
        );
    }

    [Fact]
    public void CloneShouldCopyAndLinkSignal()
    {
        Execute(
            """
            const controller = new AbortController();
            const req = new Request('https://example.com', {
                method: 'POST',
                headers: { 'X-A': '1' },
                signal: controller.signal,
                body: 'data'
            });
            const clone = req.clone();
            const sameInstance = clone === req;
            const headerCopy = clone.headers.get('x-a');
            controller.abort('boom');
            const cloneAborted = clone.signal.aborted;
            const cloneReason = clone.signal.reason;
            const originalAborted = req.signal.aborted;
            """
        );

        Assert.False(Evaluate("sameInstance").AsBoolean());
        Assert.Equal("1", Evaluate("headerCopy"));
        Assert.True(Evaluate("cloneAborted").AsBoolean());
        Assert.True(Evaluate("originalAborted").AsBoolean());
        Assert.Equal("boom", Evaluate("cloneReason"));
        Assert.False(Evaluate("clone.bodyUsed").AsBoolean());
        Assert.False(Evaluate("req.bodyUsed").AsBoolean());
    }
}
