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
        Engine.Execute("const event = new Event('test');");
        var result = Engine.Evaluate("event.defaultPrevented").AsBoolean();
        Assert.False(result);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CanBePrevented(bool cancelable)
    {
        Engine.Execute(
            $"const event = new Event('test', {{ cancelable: {cancelable.ToString().ToLowerInvariant()} }});"
        );
        Engine.Evaluate("event.preventDefault();");
        Assert.Equal(Engine.Evaluate("event.defaultPrevented").AsBoolean(), cancelable);
    }

    [Fact]
    public void ShouldSupportCustomEventData()
    {
        Engine.Execute(
            """
            const event = new Event('custom');
            event.customData = { value: 42, text: 'custom' };
            event.timestamp = Date.now();
            """
        );

        Assert.Equal(42, Engine.Evaluate("event.customData.value").AsNumber());
        Assert.Equal("custom", Engine.Evaluate("event.customData.text").AsString());
        Assert.Equal("number", Engine.Evaluate("typeof event.timestamp").AsString());
    }

    [Fact]
    public void ShouldHandleEventComposition()
    {
        Engine.Execute(
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

        Assert.Equal(3, Engine.Evaluate("events.length").AsNumber());
        Assert.True(Engine.Evaluate("events[0].bubbles").AsBoolean());
        Assert.True(Engine.Evaluate("events[1].cancelable").AsBoolean());
        Assert.True(Engine.Evaluate("events[2].bubbles && events[2].cancelable").AsBoolean());
    }

    [Fact]
    public void ShouldMaintainEventPhases()
    {
        Engine.Execute(
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

        Assert.Equal(0, Engine.Evaluate("phases.none").AsNumber());
        Assert.Equal(1, Engine.Evaluate("phases.capturing").AsNumber());
        Assert.Equal(2, Engine.Evaluate("phases.target").AsNumber());
        Assert.Equal(3, Engine.Evaluate("phases.bubbling").AsNumber());
    }

    [Fact]
    public void ShouldHandleEventPropagationStates()
    {
        Engine.Execute(
            @"
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
        "
        );

        Assert.Equal(1, Engine.Evaluate("propagationData.length").AsNumber());
        Assert.False(Engine.Evaluate("propagationData[0].defaultPrevented").AsBoolean());
        Assert.True(Engine.Evaluate("propagationData[0].cancelable").AsBoolean());
    }

    [Fact]
    public void ShouldSupportEventTimestamp()
    {
        Engine.Execute(
            @"
            const event = new Event('test');
            const timestamp = event.timeStamp;
        "
        );

        Assert.Equal("number", Engine.Evaluate("typeof timestamp").AsString());
        Assert.True(Engine.Evaluate("timestamp").AsNumber() > 0);
    }

    [Fact]
    public void ShouldHandleEventInheritance()
    {
        Engine.Execute(
            @"
            const event = new Event('test');
        "
        );

        Assert.True(Engine.Evaluate("event instanceof Event").AsBoolean());
        Assert.True(Engine.Evaluate("event instanceof Object").AsBoolean());
        Assert.Equal("[object Event]", Engine.Evaluate("event.toString()"));
    }

    [Fact]
    public void ShouldHandleEventWithInitDict()
    {
        Engine.Execute(
            @"
            const event = new Event('custom', {
                bubbles: true,
                cancelable: true,
                composed: false
            });
        "
        );

        Assert.Equal("custom", Engine.Evaluate("event.type").AsString());
        Assert.True(Engine.Evaluate("event.bubbles").AsBoolean());
        Assert.True(Engine.Evaluate("event.cancelable").AsBoolean());
        Assert.False(Engine.Evaluate("event.composed || false").AsBoolean());
    }

    [Fact]
    public void ShouldHandleEventTargetRelatedProperties()
    {
        Engine.Execute(
            @"
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
        "
        );

        Assert.True(Engine.Evaluate("eventData.target").AsBoolean());
        Assert.True(Engine.Evaluate("eventData.currentTarget").AsBoolean());
        Assert.Equal(EventPhases.AT_TARGET, Engine.Evaluate("eventData.eventPhase").AsNumber());
    }

    [Fact]
    public void ShouldThrowErrorForInvalidConstructor()
    {
        var exception = Assert.Throws<JavaScriptException>(() =>
        {
            Engine.Execute("new Event();"); // Missing required type parameter
        });

        Assert.Contains("TypeError", exception.Error.ToString());
    }
}
