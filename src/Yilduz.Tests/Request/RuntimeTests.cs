using Jint;
using Xunit;

namespace Yilduz.Tests.Request;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void TextShouldResolveWithBodyString()
    {
        Execute(
            """
            var textResult;
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'hello world'
            });
            async function readText() { textResult = await req.text(); }
            """
        );

        Evaluate("readText()").UnwrapIfPromise();
        Assert.Equal("hello world", Evaluate("textResult").AsString());
    }

    [Fact]
    public void TextShouldResolveWithEmptyStringWhenBodyIsNull()
    {
        Execute("const req = new Request('https://example.com');");

        var result = Evaluate("req.text()").UnwrapIfPromise();
        Assert.Equal(string.Empty, result.AsString());
    }

    [Fact]
    public void TextShouldHandleNonAsciiContent()
    {
        Execute(
            """
            var textResult;
            const encoder = new TextEncoder();
            const bytes = encoder.encode('咕咕嘎嘎');
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: bytes
            });
            async function readText() { textResult = await req.text(); }
            """
        );

        Evaluate("readText()").UnwrapIfPromise();
        Assert.Equal("咕咕嘎嘎", Evaluate("textResult").AsString());
    }

    [Fact]
    public void BytesShouldResolveWithUint8Array()
    {
        Execute(
            """
            var bytesResult;
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'abc'
            });
            async function readBytes() { bytesResult = await req.bytes(); }
            """
        );

        Evaluate("readBytes()").UnwrapIfPromise();

        Assert.True(Evaluate("bytesResult instanceof Uint8Array").AsBoolean());
        Assert.Equal(3, Evaluate("bytesResult.length").AsNumber());
        // 'a'=97, 'b'=98, 'c'=99
        Assert.Equal(97, Evaluate("bytesResult[0]").AsNumber());
        Assert.Equal(98, Evaluate("bytesResult[1]").AsNumber());
        Assert.Equal(99, Evaluate("bytesResult[2]").AsNumber());
    }

    [Fact]
    public void BytesShouldResolveWithEmptyUint8ArrayWhenBodyIsNull()
    {
        Execute(
            """
            var bytesResult;
            const req = new Request('https://example.com');
            async function readBytes() { bytesResult = await req.bytes(); }
            """
        );

        Evaluate("readBytes()").UnwrapIfPromise();

        Assert.True(Evaluate("bytesResult instanceof Uint8Array").AsBoolean());
        Assert.Equal(0, Evaluate("bytesResult.length").AsNumber());
    }

    [Fact]
    public void ArrayBufferShouldResolveWithCorrectByteLength()
    {
        Execute(
            """
            var abResult;
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'hello'
            });
            async function readAb() { abResult = await req.arrayBuffer(); }
            """
        );

        Evaluate("readAb()").UnwrapIfPromise();

        Assert.True(Evaluate("abResult instanceof ArrayBuffer").AsBoolean());
        Assert.Equal(5, Evaluate("abResult.byteLength").AsNumber());
    }

    [Fact]
    public void ArrayBufferShouldResolveWithZeroByteLengthWhenBodyIsNull()
    {
        Execute(
            """
            var abResult;
            const req = new Request('https://example.com');
            async function readAb() { abResult = await req.arrayBuffer(); }
            """
        );

        Evaluate("readAb()").UnwrapIfPromise();

        Assert.True(Evaluate("abResult instanceof ArrayBuffer").AsBoolean());
        Assert.Equal(0, Evaluate("abResult.byteLength").AsNumber());
    }

    [Fact]
    public void JsonShouldParseObjectBody()
    {
        Execute(
            """
            var jsonResult;
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: '{"key":"value","num":42}'
            });
            async function readJson() { jsonResult = await req.json(); }
            """
        );

        Evaluate("readJson()").UnwrapIfPromise();
        Assert.Equal("value", Evaluate("jsonResult.key").AsString());
        Assert.Equal(42, Evaluate("jsonResult.num").AsNumber());
    }

    [Fact]
    public void JsonShouldParseArrayBody()
    {
        Execute(
            """
            var jsonResult;
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: '[1,2,3]'
            });
            async function readJson() { jsonResult = await req.json(); }
            """
        );

        Evaluate("readJson()").UnwrapIfPromise();
        var arr = Evaluate("jsonResult").AsArray();
        Assert.Equal<uint>(3, arr.Length);
        Assert.Equal(1, arr[0].AsNumber());
        Assert.Equal(2, arr[1].AsNumber());
        Assert.Equal(3, arr[2].AsNumber());
    }

    [Fact]
    public void JsonShouldRejectOnInvalidJson()
    {
        Execute(
            """
            var jsonError;
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'not json'
            });
            async function readJson() {
                try { await req.json(); }
                catch (e) { jsonError = e.message; }
            }
            """
        );

        Evaluate("readJson()").UnwrapIfPromise();
        // If a JSON parse error occurred it will be stored in jsonError.
        Assert.False(Evaluate("jsonError === undefined").AsBoolean());
    }

    [Fact]
    public void BlobShouldResolveWithBlobInstance()
    {
        Execute(
            """
            var blobResult;
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'blob content'
            });
            async function readBlob() { blobResult = await req.blob(); }
            """
        );

        Evaluate("readBlob()").UnwrapIfPromise();

        Assert.True(Evaluate("blobResult instanceof Blob").AsBoolean());
        Assert.Equal(12, Evaluate("blobResult.size").AsNumber());
    }

    [Fact]
    public void BodyUsedShouldBeFalseBeforeConsuming()
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

        Assert.False(Evaluate("req.bodyUsed").AsBoolean());
    }

    [Fact]
    public void SecondConsumptionShouldRejectWithTypeError()
    {
        Execute(
            """
            var secondError;
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'data'
            });
            async function consumeTwice() {
                await req.text();   // first: ok
                try {
                    await req.text(); // second: should throw
                } catch (e) {
                    secondError = e.message;
                }
            }
            """
        );

        Evaluate("consumeTwice()").UnwrapIfPromise();
        Assert.False(Evaluate("secondError === undefined").AsBoolean());
    }

    [Fact]
    public void ShouldInferContentTypeHeaderFromStringBody()
    {
        Execute(
            """
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: 'plain text'
            });
            """
        );

        Assert.Equal(
            "text/plain;charset=UTF-8",
            Evaluate("req.headers.get('content-type')").AsString()
        );
    }

    [Fact]
    public void ShouldInferContentTypeHeaderFromBlobBody()
    {
        Execute(
            """
            const blob = new Blob(['{}'], { type: 'application/json' });
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                body: blob
            });
            """
        );

        Assert.Equal("application/json", Evaluate("req.headers.get('content-type')").AsString());
    }

    [Fact]
    public void ExplicitContentTypeHeaderShouldNotBeOverriddenByBodyExtraction()
    {
        Execute(
            """
            const req = new Request('https://example.com', {
                method: 'POST',
                duplex: 'half',
                headers: { 'Content-Type': 'application/octet-stream' },
                body: 'text data'
            });
            """
        );

        Assert.Equal(
            "application/octet-stream",
            Evaluate("req.headers.get('content-type')").AsString()
        );
    }
}
