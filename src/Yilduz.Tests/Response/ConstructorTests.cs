using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Response;

public sealed class ConstructorTests : TestBase
{
    // -----------------------------------------------------------------------
    // new Response() — default construction
    // -----------------------------------------------------------------------

    [Fact]
    public void ShouldCreateWithDefaults()
    {
        Execute("const res = new Response();");

        Assert.Equal("default", Evaluate("res.type").AsString());
        Assert.Equal(200, Evaluate("res.status").AsNumber());
        Assert.Equal(string.Empty, Evaluate("res.statusText").AsString());
        Assert.True(Evaluate("res.ok").AsBoolean());
        Assert.Equal(string.Empty, Evaluate("res.url").AsString());
        Assert.False(Evaluate("res.redirected").AsBoolean());
        Assert.True(Evaluate("res.headers instanceof Headers").AsBoolean());
        Assert.True(Evaluate("res.body === null").AsBoolean());
        Assert.False(Evaluate("res.bodyUsed").AsBoolean());
    }

    // -----------------------------------------------------------------------
    // init.status validation
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(200)]
    [InlineData(204)]
    [InlineData(301)]
    [InlineData(404)]
    [InlineData(500)]
    [InlineData(599)]
    public void ShouldAcceptValidStatus(int status)
    {
        Execute($"const res = new Response(null, {{ status: {status} }});");
        Assert.Equal(status, Evaluate("res.status").AsNumber());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    [InlineData(199)]
    [InlineData(600)]
    [InlineData(999)]
    public void ShouldThrowWhenStatusOutOfRange(int status)
    {
        Assert.Throws<JavaScriptException>(
            () => Execute($"new Response(null, {{ status: {status} }});")
        );
    }

    // -----------------------------------------------------------------------
    // init.statusText validation
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData("OK")]
    [InlineData("Not Found")]
    [InlineData("Internal Server Error")]
    [InlineData("")] // empty is always valid
    public void ShouldAcceptValidStatusText(string text)
    {
        Execute($"const res = new Response(null, {{ statusText: '{text}' }});");
        Assert.Equal(text, Evaluate("res.statusText").AsString());
    }

    [Theory]
    [InlineData("OK\r\n")]
    [InlineData("Bad\x00Value")]
    public void ShouldThrowWhenStatusTextIsInvalid(string text)
    {
        Assert.Throws<JavaScriptException>(
            () => Execute($"new Response(null, {{ statusText: '{text}' }});")
        );
    }

    // -----------------------------------------------------------------------
    // ok getter
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(200, true)]
    [InlineData(201, true)]
    [InlineData(299, true)]
    [InlineData(300, false)]
    [InlineData(400, false)]
    [InlineData(500, false)]
    public void ShouldReturnCorrectOkValue(int status, bool expectedOk)
    {
        Execute($"const res = new Response(null, {{ status: {status} }});");
        Assert.Equal(expectedOk, Evaluate("res.ok").AsBoolean());
    }

    // -----------------------------------------------------------------------
    // init.headers
    // -----------------------------------------------------------------------

    [Fact]
    public void ShouldApplyInitHeadersFromObject()
    {
        Execute(
            """
            const res = new Response(null, {
                headers: { 'X-Custom': 'hello', 'Content-Type': 'text/plain' }
            });
            """
        );

        Assert.Equal("hello", Evaluate("res.headers.get('x-custom')").AsString());
        Assert.Equal("text/plain", Evaluate("res.headers.get('content-type')").AsString());
    }

    [Fact]
    public void ShouldApplyInitHeadersFromHeadersInstance()
    {
        Execute(
            """
            const h = new Headers();
            h.set('X-Token', 'abc123');
            const res = new Response(null, { headers: h });
            """
        );

        Assert.Equal("abc123", Evaluate("res.headers.get('x-token')").AsString());
    }

    // -----------------------------------------------------------------------
    // body + Content-Type auto-injection
    // -----------------------------------------------------------------------

    [Fact]
    public void ShouldExposeBodyAsReadableStreamWhenStringProvided()
    {
        Execute("const res = new Response('hello');");

        Assert.True(Evaluate("res.body instanceof ReadableStream").AsBoolean());
        Assert.False(Evaluate("res.bodyUsed").AsBoolean());
        Assert.Equal(
            "text/plain;charset=UTF-8",
            Evaluate("res.headers.get('content-type')").AsString()
        );
    }

    [Fact]
    public void ShouldNotOverrideExistingContentTypeHeader()
    {
        Execute(
            """
            const res = new Response('{}', {
                headers: { 'Content-Type': 'application/json' }
            });
            """
        );

        Assert.Equal("application/json", Evaluate("res.headers.get('content-type')").AsString());
    }

    [Fact]
    public void ShouldHaveNullBodyWhenNoBodyProvided()
    {
        Execute("const res = new Response();");

        Assert.True(Evaluate("res.body === null").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenBodyIsSetOnNullBodyStatus()
    {
        // 204 No Content is a null-body status
        Assert.Throws<JavaScriptException>(
            () => Execute("new Response('payload', { status: 204 });")
        );
        // 304 Not Modified
        Assert.Throws<JavaScriptException>(
            () => Execute("new Response('payload', { status: 304 });")
        );
    }

    // -----------------------------------------------------------------------
    // Response.error()
    // -----------------------------------------------------------------------

    [Fact]
    public void StaticErrorShouldReturnNetworkError()
    {
        Execute("const err = Response.error();");

        Assert.Equal("error", Evaluate("err.type").AsString());
        Assert.Equal(0, Evaluate("err.status").AsNumber());
        Assert.Equal(string.Empty, Evaluate("err.statusText").AsString());
        Assert.False(Evaluate("err.ok").AsBoolean());
        Assert.True(Evaluate("err.body === null").AsBoolean());
    }

    [Fact]
    public void StaticErrorHeadersShouldBeImmutable()
    {
        Execute("const err = Response.error();");

        // headers guard is 'immutable', so any mutation must throw
        Assert.Throws<JavaScriptException>(() => Execute("err.headers.set('X-Test', 'value');"));
    }

    // -----------------------------------------------------------------------
    // Response.redirect()
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(301)]
    [InlineData(302)]
    [InlineData(303)]
    [InlineData(307)]
    [InlineData(308)]
    public void StaticRedirectShouldCreateRedirectResponse(int status)
    {
        Execute($"const r = Response.redirect('https://example.com/new', {status});");

        Assert.Equal(status, Evaluate("r.status").AsNumber());
        Assert.Equal("https://example.com/new", Evaluate("r.headers.get('location')").AsString());
    }

    [Fact]
    public void StaticRedirectShouldDefaultTo302()
    {
        Execute("const r = Response.redirect('https://example.com/');");

        Assert.Equal(302, Evaluate("r.status").AsNumber());
    }

    [Theory]
    [InlineData(200)]
    [InlineData(204)]
    [InlineData(400)]
    [InlineData(500)]
    public void StaticRedirectShouldThrowWhenStatusIsNotRedirectStatus(int status)
    {
        Assert.Throws<JavaScriptException>(
            () => Execute($"Response.redirect('https://example.com/', {status});")
        );
    }

    [Fact]
    public void StaticRedirectShouldThrowWhenUrlIsInvalid()
    {
        Assert.Throws<JavaScriptException>(() => Execute("Response.redirect('not a valid url');"));
    }

    [Fact]
    public void StaticRedirectHeadersShouldBeImmutable()
    {
        Execute("const r = Response.redirect('https://example.com/');");

        Assert.Throws<JavaScriptException>(() => Execute("r.headers.set('X-Test', 'value');"));
    }

    // -----------------------------------------------------------------------
    // Response.json()
    // -----------------------------------------------------------------------

    [Fact]
    public void StaticJsonShouldSerializeDataAndSetContentType()
    {
        Execute("const res = Response.json({ key: 'value' });");

        Assert.Equal(200, Evaluate("res.status").AsNumber());
        Assert.Equal("application/json", Evaluate("res.headers.get('content-type')").AsString());
        Assert.True(Evaluate("res.body instanceof ReadableStream").AsBoolean());
    }

    [Fact]
    public void StaticJsonShouldRespectInitOptions()
    {
        Execute(
            """
            const res = Response.json({ ok: true }, {
                status: 201,
                headers: { 'X-Created': 'yes' }
            });
            """
        );

        Assert.Equal(201, Evaluate("res.status").AsNumber());
        Assert.Equal("yes", Evaluate("res.headers.get('x-created')").AsString());
    }

    [Fact]
    public void StaticJsonBodyShouldDeserializeCorrectly()
    {
        Execute(
            """
            var parsed;
            async function run() {
                const res = Response.json({ x: 42 });
                const text = await res.text();
                parsed = JSON.parse(text);
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(42, Evaluate("parsed.x").AsNumber());
    }

    // -----------------------------------------------------------------------
    // All init options together
    // -----------------------------------------------------------------------

    [Fact]
    public void ShouldApplyAllInitOptions()
    {
        Execute(
            """
            const res = new Response('data', {
                status: 201,
                statusText: 'Created',
                headers: { 'X-Id': '99' }
            });
            """
        );

        Assert.Equal(201, Evaluate("res.status").AsNumber());
        Assert.Equal("Created", Evaluate("res.statusText").AsString());
        Assert.Equal("99", Evaluate("res.headers.get('x-id')").AsString());
        Assert.True(Evaluate("res.ok").AsBoolean());
    }
}
