using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultController;

public sealed class BackpressureTests : TestBase
{
    [Fact]
    public void ShouldHandleBackpressureWithHighWaterMark()
    {
        Execute(
            """
            let controller = null;
            const stream = new WritableStream({
                start(ctrl) {
                    controller = ctrl;
                }
            }, {
                highWaterMark: 1
            });

            const writer = stream.getWriter();
            """
        );

        // Initially, desired size should be positive (1 with HWM=1 and 0 queued)
        Assert.True(Evaluate("writer.desiredSize > 0").AsBoolean());
    }

    [Fact]
    public void ShouldRespectCustomSizeAlgorithm()
    {
        Execute(
            """
            const stream = new WritableStream({
                write(chunk) {
                    return Promise.resolve();
                }
            }, {
                highWaterMark: 10,
                size(chunk) {
                    return chunk.length;
                }
            });

            const writer = stream.getWriter();
            """
        );

        // With HWM=10 and no queued chunks, desired size should be 10
        Assert.Equal(10, Evaluate("writer.desiredSize").AsNumber());
    }

    [Fact]
    public void ShouldCalculateDesiredSizeCorrectly()
    {
        Execute(
            """
            const writeResolvers = [];
            const stream = new WritableStream({
                write(chunk) {
                    return new Promise(resolve => {
                        writeResolvers.push(resolve);
                    });
                }
            }, {
                highWaterMark: 5,
                size(chunk) {
                    return typeof chunk === 'string' ? chunk.length : 1;
                }
            });

            const writer = stream.getWriter();
            writer.write('ab'); // size 2
            writer.write('cd'); // size 2
            """
        );

        // HWM=5, queued size=4, so desired size should be 1
        Assert.Equal(1, Evaluate("writer.desiredSize").AsNumber());
    }

    [Fact]
    public void ShouldHandleNegativeDesiredSize()
    {
        Execute(
            """
            const stream = new WritableStream({
            }, {
                highWaterMark: 3,
                size(chunk) {
                    return chunk.length;
                }
            });

            const writer = stream.getWriter();
            writer.write('abcd'); // size 4, exceeds HWM of 3
            """
        );

        // HWM=3, queued size=4, so desired size should be -1
        Assert.Equal(-1, Evaluate("writer.desiredSize"));
    }
}
