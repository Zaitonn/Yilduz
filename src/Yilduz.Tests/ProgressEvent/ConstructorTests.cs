using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.ProgressEvent;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateProgressEventWithTypeOnly()
    {
        Engine.Execute("const event = new ProgressEvent('progress');");

        Assert.Equal("ProgressEvent", Engine.Evaluate("event.constructor.name").AsString());
        Assert.Equal("progress", Engine.Evaluate("event.type").AsString());
        Assert.False(Engine.Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(0, Engine.Evaluate("event.loaded").AsNumber());
        Assert.Equal(0, Engine.Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldCreateProgressEventWithOptions()
    {
        Engine.Execute(
            """
            const event = new ProgressEvent('progress', {
                lengthComputable: true,
                loaded: 50,
                total: 100
            });
            """
        );

        Assert.Equal("progress", Engine.Evaluate("event.type").AsString());
        Assert.True(Engine.Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(50, Engine.Evaluate("event.loaded").AsNumber());
        Assert.Equal(100, Engine.Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldInheritFromEvent()
    {
        Engine.Execute("const event = new ProgressEvent('test');");

        Assert.True(Engine.Evaluate("event instanceof ProgressEvent").AsBoolean());
        Assert.True(Engine.Evaluate("event instanceof Event").AsBoolean());
        Assert.True(Engine.Evaluate("event instanceof Object").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectStringTag()
    {
        Engine.Execute("const event = new ProgressEvent('test');");

        Assert.Equal(
            "[object ProgressEvent]",
            Engine.Evaluate("Object.prototype.toString.call(event)").AsString()
        );
    }

    [Fact]
    public void ShouldHandleEventInitOptions()
    {
        Engine.Execute(
            """
            const event = new ProgressEvent('custom', {
                bubbles: true,
                cancelable: true,
                lengthComputable: true,
                loaded: 75,
                total: 150
            });
            """
        );

        Assert.Equal("custom", Engine.Evaluate("event.type").AsString());
        Assert.True(Engine.Evaluate("event.bubbles").AsBoolean());
        Assert.True(Engine.Evaluate("event.cancelable").AsBoolean());

        Assert.True(Engine.Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(75, Engine.Evaluate("event.loaded").AsNumber());
        Assert.Equal(150, Engine.Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldHandlePartialOptions()
    {
        Engine.Execute(
            """
            const event = new ProgressEvent('loadstart', {
                lengthComputable: true,
                loaded: 25
            });
            """
        );

        Assert.True(Engine.Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(25, Engine.Evaluate("event.loaded").AsNumber());
        Assert.Equal(0, Engine.Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldHandleNonObjectOptions()
    {
        Engine.Execute("const event = new ProgressEvent('load', null);");

        Assert.False(Engine.Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(0, Engine.Evaluate("event.loaded").AsNumber());
        Assert.Equal(0, Engine.Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldHandleInvalidNumberValues()
    {
        Engine.Execute(
            """
            const event = new ProgressEvent('progress', {
                lengthComputable: true,
                loaded: 'invalid',
                total: null
            });
            """
        );

        Assert.True(Engine.Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(0, Engine.Evaluate("event.loaded").AsNumber());
        Assert.Equal(0, Engine.Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldHandleLargeNumbers()
    {
        Engine.Execute(
            """
            const event = new ProgressEvent('progress', {
                lengthComputable: true,
                loaded: Number.MAX_SAFE_INTEGER,
                total: Number.MAX_SAFE_INTEGER
            });
            """
        );

        Assert.True(Engine.Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(9007199254740991, Engine.Evaluate("event.loaded").AsNumber());
        Assert.Equal(9007199254740991, Engine.Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldHandleZeroValues()
    {
        Engine.Execute(
            """
            const event = new ProgressEvent('loadend', {
                lengthComputable: false,
                loaded: 0,
                total: 0
            });
            """
        );

        Assert.False(Engine.Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(0, Engine.Evaluate("event.loaded").AsNumber());
        Assert.Equal(0, Engine.Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldThrowWithoutRequiredType()
    {
        Assert.Throws<JavaScriptException>(() => Engine.Execute("new ProgressEvent();"));
    }

    [Fact]
    public void ShouldHandleCommonProgressEventTypes()
    {
        var eventTypes = new[] { "loadstart", "progress", "load", "loadend", "error", "abort" };

        foreach (var eventType in eventTypes)
        {
            Engine.Execute($"const event_{eventType} = new ProgressEvent('{eventType}');");
            Assert.Equal(eventType, Engine.Evaluate($"event_{eventType}.type").AsString());
        }
    }

    [Fact]
    public void ShouldHaveReadOnlyProperties()
    {
        Engine.Execute(
            """
            const event = new ProgressEvent('progress', {
                lengthComputable: true,
                loaded: 50,
                total: 100
            });

            event.type = 'modified';
            event.lengthComputable = false;
            event.loaded = 999;
            event.total = 999;
            """
        );

        Assert.Equal("progress", Engine.Evaluate("event.type").AsString());
        Assert.True(Engine.Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(50, Engine.Evaluate("event.loaded").AsNumber());
        Assert.Equal(100, Engine.Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldSupportEventPreventDefault()
    {
        Engine.Execute(
            """
            const event = new ProgressEvent('progress', {
                cancelable: true,
                lengthComputable: true,
                loaded: 50,
                total: 100
            });
            """
        );

        Assert.False(Engine.Evaluate("event.defaultPrevented").AsBoolean());

        Engine.Execute("event.preventDefault();");

        Assert.True(Engine.Evaluate("event.defaultPrevented").AsBoolean());
    }

    [Fact]
    public void ShouldWorkWithEventTarget()
    {
        Engine.Execute(
            """
            const target = new EventTarget();
            let capturedEvent = null;

            target.addEventListener('progress', (event) => {
                capturedEvent = event;
            });

            const progressEvent = new ProgressEvent('progress', {
                lengthComputable: true,
                loaded: 75,
                total: 100
            });

            target.dispatchEvent(progressEvent);
            """
        );

        Assert.True(Engine.Evaluate("capturedEvent !== null").AsBoolean());
        Assert.Equal("progress", Engine.Evaluate("capturedEvent.type").AsString());
        Assert.True(Engine.Evaluate("capturedEvent.lengthComputable").AsBoolean());
        Assert.Equal(75, Engine.Evaluate("capturedEvent.loaded").AsNumber());
        Assert.Equal(100, Engine.Evaluate("capturedEvent.total").AsNumber());
    }
}
