using System;
using Jint;
using Xunit;

namespace Yilduz.Tests.Blob;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldBeGlobalConstructor()
    {
        Assert.True(Evaluate("typeof Blob === 'function'").AsBoolean());
        Assert.True(Evaluate("Blob.prototype").IsObject());
    }

    [Fact]
    public void ShouldCreateBlobWithoutAnyArguments()
    {
        Execute("const blob = new Blob();");
        Assert.Equal("Blob", Evaluate("blob.constructor.name"));
    }

    [Fact]
    public void ShouldCreateBlobWithEmptyArray()
    {
        Execute("const blob = new Blob([]);");
        Assert.Equal(0, Evaluate("blob.size").AsNumber());
        Assert.Equal("", Evaluate("blob.type").AsString());
    }

    [Fact]
    public void ShouldCreateBlobWithStringArray()
    {
        Execute("const blob = new Blob(['Hello', ' ', 'World']);");
        Assert.Equal(11, Evaluate("blob.size").AsNumber());
        Assert.Equal("", Evaluate("blob.type").AsString());
    }

    [Fact]
    public void ShouldCreateBlobWithTypeOption()
    {
        Execute("const blob = new Blob(['test'], { type: 'text/plain' });");
        Assert.Equal("text/plain", Evaluate("blob.type").AsString());
    }

    [Fact]
    public void ShouldTrimAndLowercaseTypeOption()
    {
        Execute("const blob = new Blob([], { type: '  TEXT/HTML  ' });");
        Assert.Equal("text/html", Evaluate("blob.type").AsString());
    }

    [Fact]
    public void ShouldHandleTypedArrayData()
    {
        Execute(
            @"
            const uint8Array = new Uint8Array([65, 66, 67]); // ABC
            const blob = new Blob([uint8Array]);
            "
        );
        Assert.Equal(3, Evaluate("blob.size").AsNumber());
    }

    [Fact]
    public void ShouldHandleArrayBufferData()
    {
        Execute(
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
        Assert.Equal(4, Evaluate("blob.size").AsNumber());
    }

    [Fact]
    public void ShouldCreateBlobWithOtherBlobs()
    {
        Execute(
            @"
            const blob1 = new Blob(['Hello']);
            const blob2 = new Blob([' World']);
            const combinedBlob = new Blob([blob1, blob2]);
            "
        );
        Assert.Equal(11, Evaluate("combinedBlob.size").AsNumber());
    }

    [Fact]
    public void ShouldHandleEndingsOption()
    {
        Execute(
            @"
            const blob1 = new Blob(['line1\nline2'], { endings: 'transparent' });
            const blob2 = new Blob(['line1\nline2']);
            "
        );

        Assert.Equal(Evaluate("blob1.size").AsNumber(), Evaluate("blob2.size").AsNumber());

        if (Environment.NewLine == "\r\n")
        {
            Execute(
                @"
                const blobTransparent = new Blob(['line1\nline2'], { endings: 'transparent' });
                const blobNative = new Blob(['line1\nline2'], { endings: 'native' });
                "
            );

            Assert.Equal(
                Evaluate("blobTransparent.size").AsNumber() + 1,
                Evaluate("blobNative.size").AsNumber()
            );
        }
    }

    [Fact]
    public void ShouldThrowErrorForInvalidEndingsOption()
    {
        Assert.Throws<Jint.Runtime.JavaScriptException>(
            () => Execute("new Blob([], { endings: 'invalid' });")
        );
    }

    [Fact]
    public void ShouldHaveCorrectToStringTag()
    {
        Execute("const blob = new Blob();");

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
        Execute(
            @"
            const blobWithNumber = new Blob([123]);
            const blobWithBoolean = new Blob([true]);
            const blobWithNull = new Blob([null]);
            const blobWithUndefined = new Blob([undefined]);
            "
        );

        Assert.Equal(3, Evaluate("blobWithNumber.size").AsNumber());
        Assert.Equal(4, Evaluate("blobWithBoolean.size").AsNumber());
        Assert.Equal(4, Evaluate("blobWithNull.size").AsNumber());
        Assert.Equal(9, Evaluate("blobWithUndefined.size").AsNumber());
    }

    [Fact]
    public void ShouldCreateBlobFromDataView()
    {
        Execute(
            """
            const buffer = new ArrayBuffer(4);
            const view = new DataView(buffer);
            view.setUint8(0, 1);
            view.setUint8(1, 2);
            view.setUint8(2, 3);
            view.setUint8(3, 4);
            const blob = new Blob([view]);
            """
        );

        Assert.Equal(4, Evaluate("blob.size").AsNumber());
    }

    [Fact]
    public void ShouldCombineDataViewWithOtherParts()
    {
        Execute(
            """
            const buffer = new ArrayBuffer(3);
            const view = new DataView(buffer);
            view.setUint8(0, 65); // A
            view.setUint8(1, 66); // B
            view.setUint8(2, 67); // C
            const blob = new Blob([view, 'DEF', new Uint8Array([71, 72, 73])]); // ABC + DEF + GHI
            """
        );

        Assert.Equal(9, Evaluate("blob.size").AsNumber());
    }

    [Fact]
    public void ShouldCreateBlobFromMultipleDataViews()
    {
        Execute(
            """
            const buf1 = new ArrayBuffer(2);
            const buf2 = new ArrayBuffer(3);
            const view1 = new DataView(buf1);
            const view2 = new DataView(buf2);
            view1.setUint8(0, 10);
            view1.setUint8(1, 20);
            view2.setUint8(0, 30);
            view2.setUint8(1, 40);
            view2.setUint8(2, 50);
            const blob = new Blob([view1, view2]);
            """
        );

        Assert.Equal(5, Evaluate("blob.size").AsNumber());
    }
}
