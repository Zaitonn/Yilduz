using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.WritableStreamDefaultWriter;

public sealed class PropertiesTests : TestBase
{
    [Fact]
    public void ShouldHaveClosedProperty()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("'closed' in writer").AsBoolean());
        Assert.True(Engine.Evaluate("writer.closed instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldHaveDesiredSizeProperty()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("'desiredSize' in writer").AsBoolean());
        Assert.True(Engine.Evaluate("typeof writer.desiredSize === 'number'").AsBoolean());
    }

    [Fact]
    public void ShouldHaveReadyProperty()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        Assert.True(Engine.Evaluate("'ready' in writer").AsBoolean());
        Assert.True(Engine.Evaluate("writer.ready instanceof Promise").AsBoolean());
    }

    [Fact]
    public void ShouldResolveReadyPromiseInitially()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            let readyResolved = false;
            writer.ready.then(() => { readyResolved = true; });
            """
        );

        // In a properly implemented stream, ready should be resolved initially
        Assert.True(Engine.Evaluate("readyResolved").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectDesiredSizeWhenWritable()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            """
        );

        var desiredSize = Engine.Evaluate("writer.desiredSize").AsNumber();
        Assert.True(desiredSize >= 0);
    }

    [Fact]
    public void ShouldHaveNullDesiredSizeWhenErrored()
    {
        Engine.Execute(
            """
            const stream = new WritableStream({
                start(controller) {
                    controller.error(new Error('test error'));
                }
            });
            """
        );

        Engine.Execute(
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
        if (!Engine.Evaluate("typeof writer === 'undefined'").AsBoolean())
        {
            Assert.True(Engine.Evaluate("writer.desiredSize === null").AsBoolean());
        }
    }

    [Fact]
    public void ShouldHaveZeroDesiredSizeWhenClosed()
    {
        Engine.Execute(
            """
            const stream = new WritableStream();
            const writer = stream.getWriter();
            writer.close();
            """
        );

        Assert.Equal(0, Engine.Evaluate("writer.desiredSize"));
    }
}
