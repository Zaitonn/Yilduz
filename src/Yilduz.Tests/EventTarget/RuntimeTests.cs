using Jint;
using Xunit;

namespace Yilduz.Tests.EventTarget;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldHandleErrorsInEventListeners()
    {
        Engine.Execute(
            @"
            const target = new EventTarget();
            let executed = false;
            target.addEventListener('test', () => {
                throw new Error('Test error');
            });
            target.addEventListener('test', () => {
                executed = true;
            });
            target.dispatchEvent(new Event('test'));
        "
        );

        var executed = Engine.Evaluate("executed").AsBoolean();
        Assert.True(executed); // Second listener should still execute
    }

    [Fact]
    public void ShouldMaintainEventListenerOrder()
    {
        Engine.Execute(
            @"
            const target = new EventTarget();
            let order = [];
            target.addEventListener('test', () => { order.push(1); });
            target.addEventListener('test', () => { order.push(2); });
            target.addEventListener('test', () => { order.push(3); });
            target.dispatchEvent(new Event('test'));
        "
        );

        var first = Engine.Evaluate("order[0]").AsNumber();
        var second = Engine.Evaluate("order[1]").AsNumber();
        var third = Engine.Evaluate("order[2]").AsNumber();

        Assert.Equal(1, first);
        Assert.Equal(2, second);
        Assert.Equal(3, third);
    }

    [Fact]
    public void ShouldHandleNestedEventDispatching()
    {
        Engine.Execute(
            @"
            const target = new EventTarget();
            let innerExecuted = false;
            target.addEventListener('outer', () => {
                target.addEventListener('inner', () => {
                    innerExecuted = true;
                });
                target.dispatchEvent(new Event('inner'));
            });
            target.dispatchEvent(new Event('outer'));
        "
        );

        var innerExecuted = Engine.Evaluate("innerExecuted").AsBoolean();
        Assert.True(innerExecuted);
    }

    [Fact]
    public void ShouldHandleEventStopPropagation()
    {
        Engine.Execute(
            @"
            const target = new EventTarget();
            let count = 0;
            target.addEventListener('test', (event) => {
                count++;
                event.stopPropagation();
            });
            target.addEventListener('test', () => {
                count += 10;
            });
            target.dispatchEvent(new Event('test'));
        "
        );

        var count = Engine.Evaluate("count").AsNumber();
        Assert.Equal(11, count); // stopPropagation doesn't affect same target
    }

    [Fact]
    public void ShouldHandleEventStopImmediatePropagation()
    {
        Engine.Execute(
            """
            const target = new EventTarget();
            let count = 0;
            target.addEventListener('test', (event) => {
                count++;
                event.stopImmediatePropagation();
            });
            target.addEventListener('test', () => {
                count += 10;
            });
            target.dispatchEvent(new Event('test'));
            """
        );

        var count = Engine.Evaluate("count").AsNumber();
        Assert.Equal(1, count); // Second listener should not execute
    }

    [Fact]
    public void ShouldHandleLargeNumberOfListeners()
    {
        Engine.Execute(
            @"
            const target = new EventTarget();
            let count = 0;
            for (let i = 0; i < 100; i++) {
                target.addEventListener('test', () => { count++; });
            }
            target.dispatchEvent(new Event('test'));
        "
        );

        var count = Engine.Evaluate("count").AsNumber();
        Assert.Equal(100, count);
    }

    [Fact]
    public void ShouldHandleListenerWithComplexOptions()
    {
        Engine.Execute(
            @"
            const target = new EventTarget();
            let passiveExecuted = false;
            let captureExecuted = false;
            target.addEventListener('test', () => {
                passiveExecuted = true;
            }, { passive: true });
            target.addEventListener('test', () => {
                captureExecuted = true;
            }, { capture: true });
            target.dispatchEvent(new Event('test'));
        "
        );

        var passiveExecuted = Engine.Evaluate("passiveExecuted").AsBoolean();
        var captureExecuted = Engine.Evaluate("captureExecuted").AsBoolean();

        Assert.True(passiveExecuted);
        Assert.True(captureExecuted);
    }

    [Fact]
    public void ShouldHandleEventWithCustomProperties()
    {
        Engine.Execute(
            @"
            const target = new EventTarget();
            let customData = null;
            target.addEventListener('custom', (event) => {
                event.customProperty = 'custom value';
                customData = event.customProperty;
            });
            const event = new Event('custom');
            target.dispatchEvent(event);
        "
        );

        var customData = Engine.Evaluate("customData").AsString();
        Assert.Equal("custom value", customData);
    }

    [Fact]
    public void ShouldHandleInvalidEventTypes()
    {
        Engine.Execute(
            @"
            const target = new EventTarget();
            let executed = false;
            target.addEventListener('', () => { executed = true; });
            target.dispatchEvent(new Event(''));
        "
        );

        var executed = Engine.Evaluate("executed").AsBoolean();
        Assert.True(executed); // Empty string is valid event type
    }

    [Fact]
    public void ShouldHandleNullAndUndefinedListeners()
    {
        Engine.Execute(
            @"
            const target = new EventTarget();
            target.addEventListener('test', null);
            target.addEventListener('test', undefined);
            target.removeEventListener('test', null);
            target.removeEventListener('test', undefined);
        "
        );
        // Should not throw
    }

    [Fact]
    public void ShouldHandleEventWithCircularReferences()
    {
        Engine.Execute(
            @"
            const target = new EventTarget();
            let eventReceived = null;
            target.addEventListener('test', (event) => {
                event.circularRef = event;
                eventReceived = event;
            });
            target.dispatchEvent(new Event('test'));
        "
        );

        var hasCircularRef = Engine
            .Evaluate("eventReceived.circularRef === eventReceived")
            .AsBoolean();
        Assert.True(hasCircularRef);
    }
}
