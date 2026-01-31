using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.TextDecoderStream;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldDecodeByteChunks()
    {
        Execute(
            """
            const stream = new TextDecoderStream();
            const writer = stream.writable.getWriter();
            const reader = stream.readable.getReader();
            var result = null;

            async function test() {
                const readPromise = reader.read();
                await writer.write(new Uint8Array([72, 101, 108, 108, 111]));
                const chunk = await readPromise;
                result = chunk.value;
            }
            """
        );

        Evaluate("test()").UnwrapIfPromise();
        Assert.Equal("Hello", Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldRejectNonByteChunk()
    {
        Execute(
            """
            const stream = new TextDecoderStream();
            const writer = stream.writable.getWriter();
            const reader = stream.readable.getReader();
            """
        );

        Assert.Throws<PromiseRejectedException>(
            () =>
                Engine
                    .Evaluate(
                        """
                        const readPromise = reader.read();
                        writer.write('not-bytes')
                        """
                    )
                    .UnwrapIfPromise()
        );
    }

    [Fact]
    public void ShouldFlushOnCloseAndComplete()
    {
        Execute(
            """
            const stream = new TextDecoderStream();
            const writer = stream.writable.getWriter();
            const reader = stream.readable.getReader();
            var result = null;
            var doneFlag = null;

            async function test() {
                const readPromise = reader.read();
                await writer.write(new Uint8Array([65]));
                const chunk = await readPromise;
                result = chunk.value;
                await writer.close();
                const done = await reader.read();
                doneFlag = done.done;
            }
            """
        );

        Evaluate("test()").UnwrapIfPromise();
        Assert.Equal("A", Evaluate("result").AsString());
        Assert.True(Evaluate("doneFlag").AsBoolean());
    }

    [Fact]
    public void ShouldRemoveBomByDefault()
    {
        Execute(
            """
            const stream = new TextDecoderStream();
            const writer = stream.writable.getWriter();
            const reader = stream.readable.getReader();
            var result = null;

            async function test() {
                const readPromise = reader.read();
                await writer.write(new Uint8Array([0xEF, 0xBB, 0xBF, 0x41]));
                const chunk = await readPromise;
                result = chunk.value;
            }
            """
        );

        Evaluate("test()").UnwrapIfPromise();
        Assert.Equal("A", Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldPreserveBomWhenIgnoreBomTrue()
    {
        Execute(
            """
            const stream = new TextDecoderStream('utf-8', { ignoreBOM: true });
            const writer = stream.writable.getWriter();
            const reader = stream.readable.getReader();
            var result = null;

            async function test() {
                const readPromise = reader.read();
                await writer.write(new Uint8Array([0xEF, 0xBB, 0xBF, 0x41]));
                const chunk = await readPromise;
                result = chunk.value;
            }
            """
        );

        Evaluate("test()").UnwrapIfPromise();
        Assert.Equal("\uFEFFA", Evaluate("result").AsString());
    }
}
