using Jint;
using Xunit;

namespace Yilduz.Tests.Response;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void TextShouldResolveWithBodyString()
    {
        Execute(
            """
            var result;
            async function run() { result = await new Response('hello world').text(); }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("hello world", Evaluate("result").AsString());
    }

    [Fact]
    public void TextShouldReturnEmptyStringWhenBodyIsNull()
    {
        var result = Evaluate("new Response(null).text()").UnwrapIfPromise();

        Assert.Equal(string.Empty, result.AsString());
    }

    [Fact]
    public void TextShouldHandleNonAsciiContent()
    {
        Execute(
            """
            var result;
            async function run() {
                const encoder = new TextEncoder();
                const res = new Response(encoder.encode('你好世界'));
                result = await res.text();
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("你好世界", Evaluate("result").AsString());
    }

    [Fact]
    public void JsonShouldParseBodyAsJson()
    {
        Execute(
            """
            var result;
            async function run() {
                const res = new Response('{"key":"value","num":42}');
                result = await res.json();
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("value", Evaluate("result.key").AsString());
        Assert.Equal(42, Evaluate("result.num").AsNumber());
    }

    [Fact]
    public void JsonShouldParseArray()
    {
        Execute(
            """
            var result;
            async function run() {
                result = await new Response('[1,2,3]').json();
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(3, Evaluate("result.length").AsNumber());
        Assert.Equal(2, Evaluate("result[1]").AsNumber());
    }

    [Fact]
    public void ArrayBufferShouldResolveWithCorrectByteLength()
    {
        Execute(
            """
            var buf;
            async function run() {
                buf = await new Response('abc').arrayBuffer();
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(3, Evaluate("buf.byteLength").AsNumber());
    }

    [Fact]
    public void ArrayBufferShouldBeEmptyWhenBodyIsNull()
    {
        Execute(
            """
            var buf;
            async function run() {
                buf = await new Response(null).arrayBuffer();
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(0, Evaluate("buf.byteLength").AsNumber());
    }

    [Fact]
    public void BytesShouldResolveWithUint8Array()
    {
        Execute(
            """
            var bytes;
            async function run() {
                bytes = await new Response('abc').bytes();
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("bytes instanceof Uint8Array").AsBoolean());
        Assert.Equal(3, Evaluate("bytes.length").AsNumber());
        Assert.Equal(97, Evaluate("bytes[0]").AsNumber()); // 'a'
        Assert.Equal(98, Evaluate("bytes[1]").AsNumber()); // 'b'
        Assert.Equal(99, Evaluate("bytes[2]").AsNumber()); // 'c'
    }

    [Fact]
    public void BytesShouldBeEmptyWhenBodyIsNull()
    {
        Execute(
            """
            var bytes;
            async function run() {
                bytes = await new Response(null).bytes();
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("bytes instanceof Uint8Array").AsBoolean());
        Assert.Equal(0, Evaluate("bytes.length").AsNumber());
    }

    [Fact]
    public void BlobShouldResolveWithBlobInstance()
    {
        Execute(
            """
            var b;
            async function run() {
                b = await new Response('hello').blob();
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.True(Evaluate("b instanceof Blob").AsBoolean());
        Assert.Equal(5, Evaluate("b.size").AsNumber());
    }

    [Fact]
    public void BodyUsedShouldBeTrueAfterConsuming()
    {
        Execute(
            """
            const res = new Response('data');
            res.text();
            """
        );

        Assert.True(Evaluate("res.bodyUsed").AsBoolean());
    }

    [Fact]
    public void ShouldThrowWhenConsumedTwice()
    {
        Execute(
            """
            const res = new Response('data');
            res.text();
            """
        );

        var ex = Record.Exception(() => Evaluate("res.text()").UnwrapIfPromise());
        Assert.NotNull(ex);
    }

    [Fact]
    public void ShouldAcceptUint8ArrayAsBody()
    {
        Execute(
            """
            var result;
            async function run() {
                const encoder = new TextEncoder();
                const res = new Response(encoder.encode('typed'));
                result = await res.text();
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal("typed", Evaluate("result").AsString());
    }

    [Fact]
    public void StaticJsonBodyRoundTrip()
    {
        Execute(
            """
            var out;
            async function run() {
                const res = Response.json({ a: 1, b: [2, 3] });
                out = await res.json();
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();
        Assert.Equal(1, Evaluate("out.a").AsNumber());
        Assert.Equal(2, Evaluate("out.b[0]").AsNumber());
        Assert.Equal(3, Evaluate("out.b[1]").AsNumber());
    }

    [Fact]
    public void RedirectedShouldBeFalseWhenSingleUrl()
    {
        Execute("const res = new Response();");

        Assert.False(Evaluate("res.redirected").AsBoolean());
    }

    [Fact]
    public void UrlShouldBeEmptyStringForManuallyCreatedResponse()
    {
        Execute("const res = new Response();");

        Assert.Equal(string.Empty, Evaluate("res.url").AsString());
    }

    [Fact]
    public void TypeShouldBeDefaultForManuallyCreatedResponse()
    {
        Execute("const res = new Response();");

        Assert.Equal("default", Evaluate("res.type").AsString());
    }

    [Fact]
    public void ErrorResponseBodyShouldBeNull()
    {
        Execute("const res = Response.error();");

        Assert.True(Evaluate("res.body === null").AsBoolean());
    }

    [Fact]
    public void ShouldInjectBlobContentTypeIntoHeaders()
    {
        Execute(
            """
            const blob = new Blob(['x'], { type: 'text/html' });
            const res = new Response(blob);
            """
        );

        Assert.Equal("text/html", Evaluate("res.headers.get('content-type')").AsString());
    }
}
