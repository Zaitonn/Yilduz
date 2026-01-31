using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.TextEncoder;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldEncodeIntoUint8Array()
    {
        Execute(
            """
            const encoder = new TextEncoder();
            const buffer = new Uint8Array(10);
            const result = encoder.encodeInto('Hello', buffer);
            """
        );

        var read = Evaluate("result.read").AsNumber();
        var written = Evaluate("result.written").AsNumber();

        Assert.Equal(5, read);
        Assert.Equal(5, written);

        // Verify the buffer contains the correct bytes
        var bufferValue0 = Evaluate("buffer[0]").AsNumber();
        var bufferValue1 = Evaluate("buffer[1]").AsNumber();
        var bufferValue2 = Evaluate("buffer[2]").AsNumber();
        var bufferValue3 = Evaluate("buffer[3]").AsNumber();
        var bufferValue4 = Evaluate("buffer[4]").AsNumber();

        Assert.Equal(72, bufferValue0); // H
        Assert.Equal(101, bufferValue1); // e
        Assert.Equal(108, bufferValue2); // l
        Assert.Equal(108, bufferValue3); // l
        Assert.Equal(111, bufferValue4); // o
    }

    [Fact]
    public void ShouldEncodeIntoLimitedBuffer()
    {
        Execute(
            """
            const encoder = new TextEncoder();
            const buffer = new Uint8Array(3);
            const result = encoder.encodeInto('Hello', buffer);
            """
        );

        var read = Evaluate("result.read").AsNumber();
        var written = Evaluate("result.written").AsNumber();

        // Should only write what fits
        Assert.Equal(3, read);
        Assert.Equal(3, written);
    }

    [Fact]
    public void ShouldEncodeIntoWithUnicodeString()
    {
        Execute(
            """
            const encoder = new TextEncoder();
            const buffer = new Uint8Array(10);
            const result = encoder.encodeInto('€', buffer);
            """
        );

        var read = Evaluate("result.read").AsNumber();
        var written = Evaluate("result.written").AsNumber();

        Assert.Equal(1, read); // 1 UTF-16 code unit
        Assert.Equal(3, written); // 3 UTF-8 bytes

        // Verify the buffer contains the correct bytes for '€'
        var bufferValue0 = Evaluate("buffer[0]").AsNumber();
        var bufferValue1 = Evaluate("buffer[1]").AsNumber();
        var bufferValue2 = Evaluate("buffer[2]").AsNumber();

        Assert.Equal(226, bufferValue0);
        Assert.Equal(130, bufferValue1);
        Assert.Equal(172, bufferValue2);
    }

    [Fact]
    public void ShouldThrowForInvalidDestination()
    {
        Assert.Throws<JavaScriptException>(
            () =>
                Execute(
                    """
                    const encoder = new TextEncoder();
                    encoder.encodeInto('test', {});
                    """
                )
        );
    }

    [Fact]
    public void ShouldHandleEmptyString()
    {
        Execute(
            """
            const encoder = new TextEncoder();
            const buffer = new Uint8Array(5);
            const result = encoder.encodeInto('', buffer);
            """
        );

        var read = Evaluate("result.read").AsNumber();
        var written = Evaluate("result.written").AsNumber();

        Assert.Equal(0, read);
        Assert.Equal(0, written);
    }
}
