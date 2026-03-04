using System.Threading.Tasks;
using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Blob;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldThrowWithInvalidArguments()
    {
        Assert.Throws<JavaScriptException>(() => Execute("new Blob('not an array');"));
        Assert.Throws<JavaScriptException>(() => Execute("new Blob([], 'not an object');"));
        Assert.Throws<JavaScriptException>(() => Execute("new Blob([], { endings: 'invalid' });"));
    }

    [Fact]
    public void ShouldHandleArrayLike()
    {
        Execute(
            """
            const arrayLike = { 
                0: 'Hello',
                1: ' ',
                2: 'World',
                length: 3
            };
            """
        );

        Assert.Throws<JavaScriptException>(() => Execute("new Blob(arrayLike)"));
    }

    [Fact]
    public void ShouldImplementStreams()
    {
        Execute(
            """
            const blob = new Blob(['Stream Test']);
            const stream = blob.stream();
            """
        );

        // Must be a ReadableStream instance, not just any object
        Assert.True(Evaluate("stream instanceof ReadableStream").AsBoolean());
    }

    [Fact]
    public async Task ShouldStreamContainCorrectData()
    {
        Execute(
            """
            let streamText = null;
            const blob = new Blob(['Hello Stream']);
            const reader = blob.stream().getReader();
            reader.read().then(({ value }) => {
                streamText = new TextDecoder().decode(value);
            });
            """
        );

        await WaitForJsConditionAsync("streamText !== null");
        Assert.Equal("Hello Stream", Evaluate("streamText").AsString());
    }

    [Fact]
    public void ShouldHaveReadOnlyProperties()
    {
        Execute(
            """
            const blob = new Blob(['test'], {type: 'text/plain'});
            const originalSize = blob.size;
            const originalType = blob.type;

            blob.size = 100;
            blob.type = 'application/json';
            """
        );

        Assert.Equal(4, Evaluate("blob.size"));
        Assert.Equal("text/plain", Evaluate("blob.type"));
    }

    [Fact]
    public void ShouldSliceCorrectly()
    {
        Execute(
            """
            const originalBlob = new Blob(['Hello, World!'], {type: 'text/plain'});
            const slice1 = originalBlob.slice(0, 5);
            const slice2 = originalBlob.slice(7);
            const slice3 = originalBlob.slice(-6);
            const slice4 = originalBlob.slice(0, 5, 'text/html');
            """
        );

        Assert.Equal(5, Evaluate("slice1.size"));
        Assert.Equal(6, Evaluate("slice2.size"));
        Assert.Equal(6, Evaluate("slice3.size"));
        Assert.Empty(Evaluate("slice1.type").AsString());
        Assert.Equal("text/html", Evaluate("slice4.type"));
    }

    [Fact]
    public void ShouldHandleNegativeSliceIndices()
    {
        Execute(
            """
            const blob = new Blob(['Hello, World!']);
            const slice = blob.slice(-5, -2);
            """
        );

        Assert.Equal(3, Evaluate("slice.size"));
    }

    [Fact]
    public void ShouldHandleSliceOutOfBounds()
    {
        Execute(
            """
            const blob = new Blob(['Hello, World!']);
            const sliceStart = blob.slice(20, 25); 
            const sliceEnd = blob.slice(0, 100);
            """
        );

        Assert.Equal(0, Evaluate("sliceStart.size"));
        Assert.Equal(13, Evaluate("sliceEnd.size"));
    }

    [Fact]
    public void ShouldHandleSliceWithDefaultEndAndContentType()
    {
        Execute(
            """
            const blob = new Blob(['Hello, World!'], {type: 'text/plain'});
            const slice = blob.slice(7);
            """
        );

        Assert.Equal(6, Evaluate("slice.size"));
        Assert.Empty(Evaluate("slice.type").AsString());
    }

    [Fact]
    public void ShouldCreatePromiseFromTextMethod()
    {
        Execute(
            """
            const blob = new Blob(['Hello, Promise!'], {type: 'text/plain'});
            const textPromise = blob.text();
            """
        );

        Assert.True(Evaluate("textPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldCreatePromiseFromArrayBufferMethod()
    {
        Execute(
            """
            const blob = new Blob(['ArrayBuffer Test']);
            const bufferPromise = blob.arrayBuffer();
            """
        );

        Assert.True(Evaluate("bufferPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public async Task ShouldResolveTextPromiseWithCorrectContent()
    {
        Execute(
            """
            let result = null;
            const blob = new Blob(['Hello, Async!'], { type: 'text/plain' });
            blob.text().then(t => { result = t; });
            """
        );

        await WaitForJsConditionAsync("result !== null");
        Assert.Equal("Hello, Async!", Evaluate("result").AsString());
    }

    [Fact]
    public async Task ShouldResolveArrayBufferPromiseWithCorrectBytes()
    {
        Execute(
            """
            let result = null;
            const blob = new Blob([new Uint8Array([1, 2, 3, 4])]);
            blob.arrayBuffer().then(ab => { result = new Uint8Array(ab); });
            """
        );

        await WaitForJsConditionAsync("result !== null");
        Assert.Equal(4, Evaluate("result.length"));
        Assert.Equal(1, Evaluate("result[0]"));
        Assert.Equal(4, Evaluate("result[3]"));
    }

    [Fact]
    public async Task ShouldReadDataViewContentViaText()
    {
        Execute(
            """
            const buffer = new ArrayBuffer(5);
            const view = new DataView(buffer);
            view.setUint8(0, 72);  // H
            view.setUint8(1, 101); // e
            view.setUint8(2, 108); // l
            view.setUint8(3, 108); // l
            view.setUint8(4, 111); // o
            const blob = new Blob([view]);
            let result = null;
            blob.text().then(t => { result = t; });
            """
        );

        await WaitForJsConditionAsync("result !== null");
        Assert.Equal("Hello", Evaluate("result").AsString());
    }

    [Fact]
    public async Task ShouldReadDataViewContentViaArrayBuffer()
    {
        Execute(
            """
            const buffer = new ArrayBuffer(3);
            const view = new DataView(buffer);
            view.setUint8(0, 10);
            view.setUint8(1, 20);
            view.setUint8(2, 30);
            const blob = new Blob([view]);
            let result = null;
            blob.arrayBuffer().then(ab => { result = new Uint8Array(ab); });
            """
        );

        await WaitForJsConditionAsync("result !== null");
        Assert.Equal(3, Evaluate("result.length"));
        Assert.Equal(10, Evaluate("result[0]"));
        Assert.Equal(20, Evaluate("result[1]"));
        Assert.Equal(30, Evaluate("result[2]"));
    }

    [Fact]
    public async Task ShouldCombineDataViewAndStringInBlob()
    {
        Execute(
            """
            const buffer = new ArrayBuffer(3);
            const view = new DataView(buffer);
            view.setUint8(0, 70);  // F
            view.setUint8(1, 111); // o
            view.setUint8(2, 111); // o
            const blob = new Blob([view, 'Bar']);
            let result = null;
            blob.text().then(t => { result = t; });
            """
        );

        await WaitForJsConditionAsync("result !== null");
        Assert.Equal("FooBar", Evaluate("result").AsString());
    }

    [Fact]
    public async Task ShouldRoundtripBlobViaDataView()
    {
        // Blob → arrayBuffer() → DataView → read bytes back
        Execute(
            """
            const blob = new Blob([new Uint8Array([255, 0, 128])]);
            let r0 = null, r1 = null, r2 = null;
            blob.arrayBuffer().then(ab => {
                const dv = new DataView(ab);
                r0 = dv.getUint8(0);
                r1 = dv.getUint8(1);
                r2 = dv.getUint8(2);
            });
            """
        );

        await WaitForJsConditionAsync("r0 !== null");
        Assert.Equal(255, Evaluate("r0"));
        Assert.Equal(0, Evaluate("r1"));
        Assert.Equal(128, Evaluate("r2"));
    }

    [Fact]
    public void ShouldReturnPromiseFromBytesMethod()
    {
        Execute("const blob = new Blob(['Hi']);");

        Assert.True(Evaluate("blob.bytes() instanceof Promise").AsBoolean());
    }

    [Fact]
    public async Task ShouldResolveBytesPromiseAsUint8Array()
    {
        Execute(
            """
            let result = null;
            const blob = new Blob(['Hi']);
            blob.bytes().then(b => { result = b; });
            """
        );

        await WaitForJsConditionAsync("result !== null");
        Assert.True(Evaluate("result instanceof Uint8Array").AsBoolean());
    }

    [Fact]
    public async Task ShouldResolveBytesWithCorrectValues()
    {
        Execute(
            """
            let result = null;
            const blob = new Blob([new Uint8Array([10, 20, 30])]);
            blob.bytes().then(b => { result = b; });
            """
        );

        await WaitForJsConditionAsync("result !== null");
        Assert.Equal(3, Evaluate("result.length"));
        Assert.Equal(10, Evaluate("result[0]"));
        Assert.Equal(20, Evaluate("result[1]"));
        Assert.Equal(30, Evaluate("result[2]"));
    }

    [Fact]
    public async Task ShouldResolveBytesFromStringContent()
    {
        Execute(
            """
            let result = null;
            const blob = new Blob(['ABC']);
            blob.bytes().then(b => { result = b; });
            """
        );

        await WaitForJsConditionAsync("result !== null");
        Assert.Equal(3, Evaluate("result.length"));
        Assert.Equal(65, Evaluate("result[0]")); // A
        Assert.Equal(66, Evaluate("result[1]")); // B
        Assert.Equal(67, Evaluate("result[2]")); // C
    }

    [Fact]
    public async Task ShouldResolveBytesFromDataView()
    {
        Execute(
            """
            const buffer = new ArrayBuffer(3);
            const view = new DataView(buffer);
            view.setUint8(0, 100);
            view.setUint8(1, 200);
            view.setUint8(2, 50);
            let result = null;
            const blob = new Blob([view]);
            blob.bytes().then(b => { result = b; });
            """
        );

        await WaitForJsConditionAsync("result !== null");
        Assert.Equal(3, Evaluate("result.length"));
        Assert.Equal(100, Evaluate("result[0]"));
        Assert.Equal(200, Evaluate("result[1]"));
        Assert.Equal(50, Evaluate("result[2]"));
    }

    [Fact]
    public async Task ShouldResolveBytesFromEmptyBlob()
    {
        Execute(
            """
            let result = null;
            const blob = new Blob([]);
            blob.bytes().then(b => { result = b; });
            """
        );

        await WaitForJsConditionAsync("result !== null");
        Assert.Equal(0, Evaluate("result.length"));
    }

    [Fact]
    public async Task ShouldRoundtripBytesAndText()
    {
        // bytes() → Uint8Array → new Blob([...]) → text() should give back the original string
        Execute(
            """
            let finalText = null;
            const original = new Blob(['RoundTrip']);
            original.bytes().then(bytes => {
                const rebuilt = new Blob([bytes]);
                rebuilt.text().then(t => { finalText = t; });
            });
            """
        );

        await WaitForJsConditionAsync("finalText !== null");
        Assert.Equal("RoundTrip", Evaluate("finalText"));
    }

    [Fact]
    public async Task ShouldMatchBytesAndArrayBufferContent()
    {
        Execute(
            """
            let bytesResult = null;
            let abResult = null;
            const blob = new Blob([new Uint8Array([7, 14, 21, 28])]);
            blob.bytes().then(b => { bytesResult = b; });
            blob.arrayBuffer().then(ab => { abResult = new Uint8Array(ab); });
            """
        );

        await WaitForJsConditionAsync("bytesResult !== null && abResult !== null");
        Assert.Equal(Evaluate("bytesResult.length"), Evaluate("abResult.length"));
        Assert.Equal(Evaluate("bytesResult[0]"), Evaluate("abResult[0]"));
        Assert.Equal(Evaluate("bytesResult[3]"), Evaluate("abResult[3]"));
    }
}
