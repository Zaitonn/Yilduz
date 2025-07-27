using System;
using Jint;
using Xunit;

namespace Yilduz.Tests.Blob;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateBlobWithoutAnyArguments()
    {
        Engine.Execute("const blob = new Blob();");
        Assert.Equal("Blob", Engine.Evaluate("blob.constructor.name"));
    }

    [Fact]
    public void ShouldCreateBlobWithEmptyArray()
    {
        Engine.Execute("const blob = new Blob([]);");
        Assert.Equal(0, Engine.Evaluate("blob.size").AsNumber());
        Assert.Equal("", Engine.Evaluate("blob.type").AsString());
    }

    [Fact]
    public void ShouldCreateBlobWithStringArray()
    {
        Engine.Execute("const blob = new Blob(['Hello', ' ', 'World']);");
        Assert.Equal(11, Engine.Evaluate("blob.size").AsNumber());
        Assert.Equal("", Engine.Evaluate("blob.type").AsString());
    }

    [Fact]
    public void ShouldCreateBlobWithTypeOption()
    {
        Engine.Execute("const blob = new Blob(['test'], { type: 'text/plain' });");
        Assert.Equal("text/plain", Engine.Evaluate("blob.type").AsString());
    }

    [Fact]
    public void ShouldTrimAndLowercaseTypeOption()
    {
        Engine.Execute("const blob = new Blob([], { type: '  TEXT/HTML  ' });");
        Assert.Equal("text/html", Engine.Evaluate("blob.type").AsString());
    }

    [Fact]
    public void ShouldHandleTypedArrayData()
    {
        Engine.Execute(
            @"
            const uint8Array = new Uint8Array([65, 66, 67]); // ABC
            const blob = new Blob([uint8Array]);
            "
        );
        Assert.Equal(3, Engine.Evaluate("blob.size").AsNumber());
    }

    [Fact]
    public void ShouldHandleArrayBufferData()
    {
        Engine.Execute(
            @"
            const buffer = new ArrayBuffer(4);
            const view = new Uint8Array(buffer);
            view[0] = 68; // D
            view[1] = 69; // E
            view[2] = 70; // F
            view[3] = 71; // G
            const blob = new Blob([buffer]);
            "
        );
        Assert.Equal(4, Engine.Evaluate("blob.size").AsNumber());
    }

    [Fact]
    public void ShouldCreateBlobWithOtherBlobs()
    {
        Engine.Execute(
            @"
            const blob1 = new Blob(['Hello']);
            const blob2 = new Blob([' World']);
            const combinedBlob = new Blob([blob1, blob2]);
            "
        );
        Assert.Equal(11, Engine.Evaluate("combinedBlob.size").AsNumber());
    }

    [Fact]
    public void ShouldHandleEndingsOption()
    {
        Engine.Execute(
            @"
            const blob1 = new Blob(['line1\nline2'], { endings: 'transparent' });
            const blob2 = new Blob(['line1\nline2']);
            "
        );

        Assert.Equal(
            Engine.Evaluate("blob1.size").AsNumber(),
            Engine.Evaluate("blob2.size").AsNumber()
        );

        if (Environment.NewLine == "\r\n")
        {
            Engine.Execute(
                @"
                const blobTransparent = new Blob(['line1\nline2'], { endings: 'transparent' });
                const blobNative = new Blob(['line1\nline2'], { endings: 'native' });
                "
            );

            Assert.Equal(
                Engine.Evaluate("blobTransparent.size").AsNumber() + 1,
                Engine.Evaluate("blobNative.size").AsNumber()
            );
        }
    }

    [Fact]
    public void ShouldThrowErrorForInvalidEndingsOption()
    {
        Assert.Throws<Jint.Runtime.JavaScriptException>(
            () => Engine.Execute("new Blob([], { endings: 'invalid' });")
        );
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Engine.Execute("const blob = new Blob();");

        Assert.Equal(
            "Blob",
            Engine
                .Evaluate("Object.prototype.toString.call(blob)")
                .AsString()
                .Replace("[object ", "")
                .Replace("]", "")
        );
    }

    [Fact]
    public void ShouldConvertNonStringBlobPartsToString()
    {
        Engine.Execute(
            @"
            const blobWithNumber = new Blob([123]);
            const blobWithBoolean = new Blob([true]);
            const blobWithNull = new Blob([null]);
            const blobWithUndefined = new Blob([undefined]);
            "
        );

        Assert.Equal(3, Engine.Evaluate("blobWithNumber.size").AsNumber());
        Assert.Equal(4, Engine.Evaluate("blobWithBoolean.size").AsNumber());
        Assert.Equal(4, Engine.Evaluate("blobWithNull.size").AsNumber());
        Assert.Equal(9, Engine.Evaluate("blobWithUndefined.size").AsNumber());
    }
}
