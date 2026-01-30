using Jint;
using Xunit;

namespace Yilduz.Tests.TextEncoderStream;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldEncodeStringChunks()
    {
        Engine.Execute(
            """
            const stream = new TextEncoderStream();
            const writer = stream.writable.getWriter();
            const reader = stream.readable.getReader();
            var result = null;

            async function test() {
                const readPromise = reader.read();
                await writer.write('Hi');
                const chunk = await readPromise;
                result = Array.from(chunk.value);
            }
            """
        );

        Engine.Evaluate("test()").UnwrapIfPromise();

        var array = Engine.Evaluate("result").AsArray();
        Assert.Equal<uint>(2, array.Length);
        Assert.Equal(72, array[0].AsNumber());
        Assert.Equal(105, array[1].AsNumber());
    }

    [Fact]
    public void ShouldHandleMultipleWritesInOrder()
    {
        Engine.Execute(
            """
            const stream = new TextEncoderStream();
            const writer = stream.writable.getWriter();
            const reader = stream.readable.getReader();
            var results = [];

            async function test() {
                const r1 = reader.read();
                await writer.write('A');
                results.push(Array.from((await r1).value));

                const r2 = reader.read();
                await writer.write('B');
                results.push(Array.from((await r2).value));
            }
            """
        );

        Engine.Evaluate("test()").UnwrapIfPromise();

        var results = Engine.Evaluate("results").AsArray();
        Assert.Equal<uint>(2, results.Length);
        Assert.Equal(65, results[0].AsArray()[0].AsNumber());
        Assert.Equal(66, results[1].AsArray()[0].AsNumber());
    }
}
