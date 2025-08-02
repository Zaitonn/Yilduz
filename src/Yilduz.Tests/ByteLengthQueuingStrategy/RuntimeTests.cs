using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.ByteLengthQueuingStrategy;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldReturnByteLengthOfArrayBuffer()
    {
        Engine.Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 16 });
            const buffer = new ArrayBuffer(8);
            const size = strategy.size(buffer);
            """
        );
        Assert.Equal(8, Engine.Evaluate("size").AsNumber());
    }

    [Fact]
    public void ShouldReturnByteLengthOfTypedArray()
    {
        Engine.Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 16 });
            const uint8Array = new Uint8Array(12);
            const size = strategy.size(uint8Array);
            """
        );
        Assert.Equal(12, Engine.Evaluate("size").AsNumber());
    }

    [Fact]
    public void ShouldReturnByteLengthOfDataView()
    {
        Engine.Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 16 });
            const buffer = new ArrayBuffer(20);
            const view = new DataView(buffer);
            const size = strategy.size(view);
            """
        );
        Assert.Equal(20, Engine.Evaluate("size").AsNumber());
    }

    [Fact]
    public void ShouldReturnUndefinedForObjectWithoutByteLength()
    {
        Engine.Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 16 });
            const obj = { length: 10 };
            const size = strategy.size(obj);
            """
        );
        Assert.True(Engine.Evaluate("size === undefined").AsBoolean());
    }

    [Fact]
    public void ShouldReturnCustomByteLengthProperty()
    {
        Engine.Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 16 });
            const obj = { byteLength: 42 };
            const size = strategy.size(obj);
            """
        );
        Assert.Equal(42, Engine.Evaluate("size").AsNumber());
    }

    [Fact]
    public void ShouldReturnZeroForEmptyArrayBuffer()
    {
        Engine.Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 16 });
            const buffer = new ArrayBuffer(0);
            const size = strategy.size(buffer);
            """
        );
        Assert.Equal(0, Engine.Evaluate("size").AsNumber());
    }

    [Fact]
    public void ShouldThrowWhenSizeCalledWithoutArguments()
    {
        Engine.Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 16 });
            let caughtError;
            try {
                strategy.size();
            } catch (e) {
                caughtError = e;
            }
            """
        );
        Assert.True(Engine.Evaluate("caughtError instanceof TypeError").AsBoolean());
    }

    [Fact]
    public void ShouldHandleNullAndUndefinedChunks()
    {
        Engine.Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 16 });
            const nullSize = strategy.size(null);
            const undefinedSize = strategy.size(undefined);
            """
        );
        Assert.True(Engine.Evaluate("nullSize === undefined").AsBoolean());
        Assert.True(Engine.Evaluate("undefinedSize === undefined").AsBoolean());
    }

    [Fact]
    public void ShouldWorkWithReadableStreamDefault()
    {
        Engine.Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 1024 });
            const stream = new ReadableStream({
                start(controller) {
                    const buffer = new ArrayBuffer(100);
                    controller.enqueue(buffer);
                    controller.close();
                }
            }, strategy);
            """
        );
        Assert.Equal("ReadableStream", Engine.Evaluate("stream.constructor.name"));
    }

    [Fact]
    public void ShouldWorkWithWritableStream()
    {
        Engine.Execute(
            """
            const strategy = new ByteLengthQueuingStrategy({ highWaterMark: 512 });
            const stream = new WritableStream({
                write(chunk) {
                    // Write chunk
                }
            }, strategy);
            """
        );
        Assert.Equal("WritableStream", Engine.Evaluate("stream.constructor.name"));
    }
}
