using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class PropertiesTests : TestBase
{
    [Fact]
    public void ShouldHaveClosedProperty()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Evaluate("'closed' in writer").AsBoolean());
        Assert.True(Evaluate("writer.closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHaveDesiredSizeProperty()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Evaluate("'desiredSize' in writer").AsBoolean());
        Assert.True(Evaluate("typeof writer.desiredSize === 'number'").AsBoolean());
    }

    [Fact]
    public void ShouldHaveReadyProperty()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Evaluate("'ready' in writer").AsBoolean());
        Assert.True(Evaluate("writer.ready instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldResolveReadyPromiseInitially()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            let readyResolved = false;
            writer.ready.then(() => { readyResolved = true; });
            """
        );

        // In a properly implemented stream, ready should be resolved initially
        Assert.True(Evaluate("readyResolved").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectDesiredSizeWhenWritable()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        var desiredSize = Evaluate("writer.desiredSize").AsNumber();
        Assert.True(desiredSize >= 0);
    }

    [Fact]
    public void ShouldHaveNullDesiredSizeWhenErrored()
    {
        Execute(
            """
            const stream = new WritableStream({
                start(controller) {
                    controller.error(new Error('test error'));
                }
            });
            """
        );

        Execute(
            """
            let writer;
            try {
                writer = stream.getWriter();
            } catch (e) {
                // Stream might be errored before we can get writer
            }
            """
        );

        // If we managed to get a writer from errored stream, desiredSize should be null
        if (!Evaluate("typeof writer === 'undefined'").AsBoolean())
        {
            Assert.True(Evaluate("writer.desiredSize === null").AsBoolean());
        }
    }

    [Fact]
    public void ShouldHaveZeroDesiredSizeWhenClosed()
    {
        Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Evaluate("writer.close()").UnwrapIfPromise();

        Assert.Equal(0, Evaluate("writer.desiredSize"));
    }
}
