using Jint;
using Jint.Runtime;
using Xunit;
using Yilduz.Events.Event;

namespace Yilduz.Tests.Event;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void DefaultPreventedShouldBeFalse()
    {
        Execute("const event = new Event('test');");
        var result = Evaluate("event.defaultPrevented").AsBoolean();
        Assert.False(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CanBePrevented(bool cancelable)
    {
        Execute(
            $"const event = new Event('test', {{ cancelable: {cancelable.ToString().ToLowerInvariant()} }});"
        );
        Evaluate("event.preventDefault();");
        Assert.Equal(Evaluate("event.defaultPrevented").AsBoolean(), cancelable);
    }

    [Fact]
    public void ShouldSupportCustomEventData()
    {
        Execute(
            """
            const event = new Event('custom');
            event.customData = { value: 42, text: 'custom' };
            event.timestamp = Date.now();
            """
        );

        Assert.Equal(42, Evaluate("event.customData.value").AsNumber());
        Assert.Equal("custom", Evaluate("event.customData.text").AsString());
        Assert.Equal("number", Evaluate("typeof event.timestamp").AsString());
    }

    [Fact]
    public void ShouldHandleEventComposition()
    {
        Execute(
            """
            const target = new EventTarget();
            let events = [];

            target.addEventListener('test', (event) => {
                events.push({
                    type: event.type,
                    bubbles: event.bubbles,
                    cancelable: event.cancelable,
                    composed: event.composed || false
                });
            });

            target.dispatchEvent(new Event('test', { bubbles: true }));
            target.dispatchEvent(new Event('test', { cancelable: true }));
            target.dispatchEvent(new Event('test', { bubbles: true, cancelable: true }));
            """
        );

        Assert.Equal(3, Evaluate("events.length").AsNumber());
        Assert.True(Evaluate("events[0].bubbles").AsBoolean());
        Assert.True(Evaluate("events[1].cancelable").AsBoolean());
        Assert.True(Evaluate("events[2].bubbles && events[2].cancelable").AsBoolean());
    }

    [Fact]
    public void ShouldMaintainEventPhases()
    {
        Execute(
            """
            const event = new Event('test');
            const phases = {
                none: event.NONE,
                capturing: event.CAPTURING_PHASE,
                target: event.AT_TARGET,
                bubbling: event.BUBBLING_PHASE
            };
            """
        );

        Assert.Equal(0, Evaluate("phases.none").AsNumber());
        Assert.Equal(1, Evaluate("phases.capturing").AsNumber());
        Assert.Equal(2, Evaluate("phases.target").AsNumber());
        Assert.Equal(3, Evaluate("phases.bubbling").AsNumber());
    }

    [Fact]
    public void ShouldHandleEventPropagationStates()
    {
        Execute(
            """
            const target = new EventTarget();
            let propagationData = [];

            target.addEventListener('test', (event) => {
                propagationData.push({
                    defaultPrevented: event.defaultPrevented,
                    cancelable: event.cancelable,
                    bubbles: event.bubbles
                });
                
                if (!event.defaultPrevented && event.cancelable) {
                    event.preventDefault();
                }
            });

            target.dispatchEvent(new Event('test', { cancelable: true, bubbles: true }));
            """
        );

        Assert.Equal(1, Evaluate("propagationData.length").AsNumber());
        Assert.False(Evaluate("propagationData[0].defaultPrevented").AsBoolean());
        Assert.True(Evaluate("propagationData[0].cancelable").AsBoolean());
    }

    [Fact]
    public void ShouldSupportEventTimestamp()
    {
        Execute(
            """
            const event = new Event('test');
            const timestamp = event.timeStamp;
            """
        );

        Assert.Equal("number", Evaluate("typeof timestamp").AsString());
        Assert.True(Evaluate("timestamp").AsNumber() > 0);
    }

    [Fact]
    public void ShouldHandleEventInheritance()
    {
        Execute(
            """
            const event = new Event('test');
            """
        );

        Assert.True(Evaluate("event instanceof Event").AsBoolean());
        Assert.True(Evaluate("event instanceof Object").AsBoolean());
        Assert.Equal("[object Event]", Evaluate("event.toString()"));
    }

    [Fact]
    public void ShouldHandleEventWithInitDict()
    {
        Execute(
            """
            const event = new Event('custom', {
                bubbles: true,
                cancelable: true,
                composed: false
            });
            """
        );

        Assert.Equal("custom", Evaluate("event.type").AsString());
        Assert.True(Evaluate("event.bubbles").AsBoolean());
        Assert.True(Evaluate("event.cancelable").AsBoolean());
        Assert.False(Evaluate("event.composed || false").AsBoolean());
    }

    [Fact]
    public void ShouldHandleEventTargetRelatedProperties()
    {
        Execute(
            """
            const target = new EventTarget();
            let eventData = null;

            target.addEventListener('test', (event) => {
                eventData = {
                    target: event.target === target,
                    currentTarget: event.currentTarget === target,
                    eventPhase: event.eventPhase
                };
            });

            target.dispatchEvent(new Event('test'));
            """
        );

        Assert.True(Evaluate("eventData.target").AsBoolean());
        Assert.True(Evaluate("eventData.currentTarget").AsBoolean());
        Assert.Equal(EventPhases.AT_TARGET, Evaluate("eventData.eventPhase").AsNumber());
    }

    [Fact]
    public void ShouldThrowErrorForInvalidConstructor()
    {
        var exception = Assert.Throws<JavaScriptException>(() =>
        {
            Execute("new Event();"); // Missing required type parameter
        });

        Assert.Contains("TypeError", exception.Error.ToString());
    }
}
