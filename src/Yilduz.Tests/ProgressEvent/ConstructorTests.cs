using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.ProgressEvent;

public sealed class ConstructorTests : TestBase
{
    [Fact]
    public void ShouldCreateProgressEventWithTypeOnly()
    {
        Execute("const event = new ProgressEvent('progress');");

        Assert.Equal("ProgressEvent", Evaluate("event.constructor.name").AsString());
        Assert.Equal("progress", Evaluate("event.type").AsString());
        Assert.False(Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(0, Evaluate("event.loaded").AsNumber());
        Assert.Equal(0, Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldCreateProgressEventWithOptions()
    {
        Execute(
            """
            const event = new ProgressEvent('progress', {
                lengthComputable: true,
                loaded: 50,
                total: 100
            });
            """
        );

        Assert.Equal("progress", Evaluate("event.type").AsString());
        Assert.True(Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(50, Evaluate("event.loaded").AsNumber());
        Assert.Equal(100, Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldInheritFromEvent()
    {
        Execute("const event = new ProgressEvent('test');");

        Assert.True(Evaluate("event instanceof ProgressEvent").AsBoolean());
        Assert.True(Evaluate("event instanceof Event").AsBoolean());
        Assert.True(Evaluate("event instanceof Object").AsBoolean());
    }

    [Fact]
    public void ShouldHaveCorrectStringTag()
    {
        Execute("const event = new ProgressEvent('test');");

        Assert.Equal(
            "[object ProgressEvent]",
            Evaluate("Object.prototype.toString.call(event)").AsString()
        );
    }

    [Fact]
    public void ShouldHandleEventInitOptions()
    {
        Execute(
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

        Assert.Equal("custom", Evaluate("event.type").AsString());
        Assert.True(Evaluate("event.bubbles").AsBoolean());
        Assert.True(Evaluate("event.cancelable").AsBoolean());

        Assert.True(Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(75, Evaluate("event.loaded").AsNumber());
        Assert.Equal(150, Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldHandlePartialOptions()
    {
        Execute(
            """
            const event = new ProgressEvent('loadstart', {
                lengthComputable: true,
                loaded: 25
            });
            """
        );

        Assert.True(Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(25, Evaluate("event.loaded").AsNumber());
        Assert.Equal(0, Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldHandleNonObjectOptions()
    {
        Execute("const event = new ProgressEvent('load', null);");

        Assert.False(Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(0, Evaluate("event.loaded").AsNumber());
        Assert.Equal(0, Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldHandleInvalidNumberValues()
    {
        Execute(
            """
            const event = new ProgressEvent('progress', {
                lengthComputable: true,
                loaded: 'invalid',
                total: null
            });
            """
        );

        Assert.True(Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(0, Evaluate("event.loaded").AsNumber());
        Assert.Equal(0, Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldHandleLargeNumbers()
    {
        Execute(
            """
            const event = new ProgressEvent('progress', {
                lengthComputable: true,
                loaded: Number.MAX_SAFE_INTEGER,
                total: Number.MAX_SAFE_INTEGER
            });
            """
        );

        Assert.True(Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(9007199254740991, Evaluate("event.loaded").AsNumber());
        Assert.Equal(9007199254740991, Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldHandleZeroValues()
    {
        Execute(
            """
            const event = new ProgressEvent('loadend', {
                lengthComputable: false,
                loaded: 0,
                total: 0
            });
            """
        );

        Assert.False(Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(0, Evaluate("event.loaded").AsNumber());
        Assert.Equal(0, Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldThrowWithoutRequiredType()
    {
        Assert.Throws<JavaScriptException>(() => Execute("new ProgressEvent();"));
    }

    [Fact]
    public void ShouldHandleCommonProgressEventTypes()
    {
        var eventTypes = new[] { "loadstart", "progress", "load", "loadend", "error", "abort" };

        foreach (var eventType in eventTypes)
        {
            Execute($"const event_{eventType} = new ProgressEvent('{eventType}');");
            Assert.Equal(eventType, Evaluate($"event_{eventType}.type").AsString());
        }
    }

    [Fact]
    public void ShouldHaveReadOnlyProperties()
    {
        Execute(
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

        Assert.Equal("progress", Evaluate("event.type").AsString());
        Assert.True(Evaluate("event.lengthComputable").AsBoolean());
        Assert.Equal(50, Evaluate("event.loaded").AsNumber());
        Assert.Equal(100, Evaluate("event.total").AsNumber());
    }

    [Fact]
    public void ShouldSupportEventPreventDefault()
    {
        Execute(
            """
            const event = new ProgressEvent('progress', {
                cancelable: true,
                lengthComputable: true,
                loaded: 50,
                total: 100
            });
            """
        );

        Assert.False(Evaluate("event.defaultPrevented").AsBoolean());

        Execute("event.preventDefault();");

        Assert.True(Evaluate("event.defaultPrevented").AsBoolean());
    }

    [Fact]
    public void ShouldWorkWithEventTarget()
    {
        Execute(
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

        Assert.True(Evaluate("capturedEvent !== null").AsBoolean());
        Assert.Equal("progress", Evaluate("capturedEvent.type").AsString());
        Assert.True(Evaluate("capturedEvent.lengthComputable").AsBoolean());
        Assert.Equal(75, Evaluate("capturedEvent.loaded").AsNumber());
        Assert.Equal(100, Evaluate("capturedEvent.total").AsNumber());
    }
}
