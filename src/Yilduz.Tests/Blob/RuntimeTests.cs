using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Blob;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldBeGlobalConstructor()
    {
        Assert.True(Engine.Evaluate("typeof Blob === 'function'").AsBoolean());
        Assert.True(Engine.Evaluate("Blob.prototype").IsObject());
    }

    [Fact]
    public void ShouldThrowWithInvalidArguments()
    {
        Assert.Throws<JavaScriptException>(() => Engine.Execute("new Blob('not an array');"));
        Assert.Throws<JavaScriptException>(() => Engine.Execute("new Blob([], 'not an object');"));
        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("new Blob([], { endings: 'invalid' });")
        );
    }

    [Fact]
    public void ShouldHandleArrayLike()
    {
        Engine.Execute(
            """
            const arrayLike = { 
                0: 'Hello',
                1: ' ',
                2: 'World',
                length: 3
            };
            """
        );

        Assert.Throws<JavaScriptException>(() => Engine.Execute("new Blob(arrayLike)"));
    }

    [Fact(Skip = "ReadableStream is not implemented yet")]
    public void ShouldImplementStreams()
    {
        Engine.Execute(
            """
            const blob = new Blob(['Stream Test']);
            const stream = blob.stream();
            const streamExists = stream !== null && typeof stream === 'object';
            """
        );

        Assert.True(Engine.Evaluate("streamExists").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectInstanceProperties()
    {
        Engine.Execute(
            """
            const blob = new Blob(['Test']);

            const sizeDescriptor = Object.getOwnPropertyDescriptor(Blob.prototype, 'size');
            const typeDescriptor = Object.getOwnPropertyDescriptor(Blob.prototype, 'type');

            const isSizeGetter = sizeDescriptor && typeof sizeDescriptor.get === 'function';
            const isTypeGetter = typeDescriptor && typeof typeDescriptor.get === 'function';
            """
        );
        Assert.True(Engine.Evaluate("isSizeGetter").AsBoolean());
        Assert.True(Engine.Evaluate("isTypeGetter").AsBoolean());
    }

    [Fact]
    public void ShouldHaveReadOnlyProperties()
    {
        Engine.Execute(
            """
            const blob = new Blob(['test'], {type: 'text/plain'});
            const originalSize = blob.size;
            const originalType = blob.type;

            blob.size = 100;
            blob.type = 'application/json';
            """
        );

        Assert.Equal(4, Engine.Evaluate("blob.size").AsNumber());
        Assert.Equal("text/plain", Engine.Evaluate("blob.type").AsString());
    }

    [Fact]
    public void ShouldSliceCorrectly()
    {
        Engine.Execute(
            """
            const originalBlob = new Blob(['Hello, World!'], {type: 'text/plain'});
            const slice1 = originalBlob.slice(0, 5);
            const slice2 = originalBlob.slice(7);
            const slice3 = originalBlob.slice(-6);
            const slice4 = originalBlob.slice(0, 5, 'text/html');
            """
        );

        Assert.Equal(5, Engine.Evaluate("slice1.size").AsNumber());
        Assert.Equal(6, Engine.Evaluate("slice2.size").AsNumber());
        Assert.Equal(6, Engine.Evaluate("slice3.size").AsNumber());
        Assert.Empty(Engine.Evaluate("slice1.type").AsString());
        Assert.Equal("text/html", Engine.Evaluate("slice4.type").AsString());
    }

    [Fact]
    public void ShouldHandleNegativeSliceIndices()
    {
        Engine.Execute(
            """
            const blob = new Blob(['Hello, World!']);
            const slice = blob.slice(-5, -2);
            """
        );

        Assert.Equal(3, Engine.Evaluate("slice.size").AsNumber());
    }

    [Fact]
    public void ShouldHandleSliceOutOfBounds()
    {
        Engine.Execute(
            """
            const blob = new Blob(['Hello, World!']);
            const sliceStart = blob.slice(20, 25); 
            const sliceEnd = blob.slice(0, 100);
            """
        );

        Assert.Equal(0, Engine.Evaluate("sliceStart.size").AsNumber());
        Assert.Equal(13, Engine.Evaluate("sliceEnd.size").AsNumber());
    }

    [Fact]
    public void ShouldHandleSliceWithDefaultEndAndContentType()
    {
        Engine.Execute(
            """
            const blob = new Blob(['Hello, World!'], {type: 'text/plain'});
            const slice = blob.slice(7);
            """
        );

        Assert.Equal(6, Engine.Evaluate("slice.size").AsNumber());
        Assert.Empty(Engine.Evaluate("slice.type").AsString());
    }

    [Fact]
    public void ShouldCreatePromiseFromTextMethod()
    {
        Engine.Execute(
            """
            const blob = new Blob(['Hello, Promise!'], {type: 'text/plain'});
            const textPromise = blob.text();
            """
        );

        Assert.True(Engine.Evaluate("textPromise instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldCreatePromiseFromArrayBufferMethod()
    {
        Engine.Execute(
            """
            const blob = new Blob(['ArrayBuffer Test']);
            const bufferPromise = blob.arrayBuffer();
            """
        );

        Assert.True(Engine.Evaluate("bufferPromise instanceof Promise").AsBoolean());
    }
}
