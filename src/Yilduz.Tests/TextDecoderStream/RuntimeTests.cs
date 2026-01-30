using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.TextDecoderStream;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldDecodeByteChunks()
    {
        Engine.Execute(
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

        Engine.Evaluate("test()").UnwrapIfPromise();
        Assert.Equal("Hello", Engine.Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldRejectNonByteChunk()
    {
        Engine.Execute(
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
        Engine.Execute(
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

        Engine.Evaluate("test()").UnwrapIfPromise();
        Assert.Equal("A", Engine.Evaluate("result").AsString());
        Assert.True(Engine.Evaluate("doneFlag").AsBoolean());
    }

    [Fact]
    public void ShouldRemoveBomByDefault()
    {
        Engine.Execute(
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

        Engine.Evaluate("test()").UnwrapIfPromise();
        Assert.Equal("A", Engine.Evaluate("result").AsString());
    }

    [Fact]
    public void ShouldPreserveBomWhenIgnoreBomTrue()
    {
        Engine.Execute(
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

        Engine.Evaluate("test()").UnwrapIfPromise();
        Assert.Equal("\uFEFFA", Engine.Evaluate("result").AsString());
    }
}
