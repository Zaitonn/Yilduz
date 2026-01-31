using Jint;
using Xunit;

namespace Yilduz.Tests.EventTarget;

public sealed class MethodTests : TestBase
{
    [Fact]
    public void AddEventListenerShouldRegisterListener()
    {
        Execute(
            """
            const target = new EventTarget();
            let executed = false;
            target.addEventListener('test', () => { executed = true; });
            target.dispatchEvent(new Event('test'));
            """
        );

        var executed = Evaluate("executed").AsBoolean();
        Assert.True(executed);
    }

    [Fact]
    public void RemoveEventListenerShouldUnregisterListener()
    {
        Execute(
            """
            const target = new EventTarget();
            let executed = false;
            const handler = () => { executed = true; };
            target.addEventListener('test', handler);
            target.removeEventListener('test', handler);
            target.dispatchEvent(new Event('test'));
            """
        );

        var executed = Evaluate("executed").AsBoolean();
        Assert.False(executed);
    }

    [Fact]
    public void ShouldSupportMultipleListenersForSameEvent()
    {
        Execute(
            """
            const target = new EventTarget();
            let count = 0;
            target.addEventListener('test', () => { count++; });
            target.addEventListener('test', () => { count++; });
            target.addEventListener('test', () => { count++; });
            target.dispatchEvent(new Event('test'));
            """
        );

        var count = Evaluate("count").AsNumber();
        Assert.Equal(3, count);
    }

    [Fact]
    public void ShouldSupportDifferentEventTypes()
    {
        Execute(
            """
            const target = new EventTarget();
            let testExecuted = false;
            let customExecuted = false;
            target.addEventListener('test', () => { testExecuted = true; });
            target.addEventListener('custom', () => { customExecuted = true; });
            target.dispatchEvent(new Event('test'));
            """
        );

        var testExecuted = Evaluate("testExecuted").AsBoolean();
        var customExecuted = Evaluate("customExecuted").AsBoolean();

        Assert.True(testExecuted);
        Assert.False(customExecuted);
    }

    [Fact]
    public void ShouldPassEventToListener()
    {
        Execute(
            """
            const target = new EventTarget();
            let eventType = null;
            let eventTarget = null;
            target.addEventListener('test', (event) => {
                eventType = event.type;
                eventTarget = event.target;
            });
            target.dispatchEvent(new Event('test'));
            """
        );

        var eventType = Evaluate("eventType").AsString();

        Assert.Equal("test", eventType);
        Assert.Equal(Evaluate("target"), Evaluate("eventTarget"));
    }

    [Fact]
    public void ShouldSupportOnceOption()
    {
        Execute(
            """
            const target = new EventTarget();
            let count = 0;
            target.addEventListener('test', () => { count++; }, { once: true });
            target.dispatchEvent(new Event('test'));
            target.dispatchEvent(new Event('test'));
            target.dispatchEvent(new Event('test'));
            """
        );

        var count = Evaluate("count").AsNumber();
        Assert.Equal(1, count);
    }

    [Fact]
    public void ShouldAllowNullEventInAddEventListener()
    {
        Execute(
            """
            const target = new EventTarget();
            let listenerExecuted = false;

            // addEventListener should accept null as event type
            target.addEventListener(null, () => {
                listenerExecuted = true;
            });

            // Dispatching null event should trigger the listener
            target.dispatchEvent(new Event(null));
            """
        );

        var listenerExecuted = Evaluate("listenerExecuted").AsBoolean();
        Assert.True(listenerExecuted);
    }

    [Fact]
    public void ShouldNotAddDuplicateListeners()
    {
        Execute(
            """
            const target = new EventTarget();
            let count = 0;
            const handler = () => { count++; };
            target.addEventListener('test', handler);
            target.addEventListener('test', handler);
            target.addEventListener('test', handler);
            target.dispatchEvent(new Event('test'));
            """
        );

        var count = Evaluate("count").AsNumber();
        Assert.Equal(1, count);
    }

    [Fact]
    public void DispatchEventShouldReturnBoolean()
    {
        Execute(
            """
            const target = new EventTarget();
            const normalResult = target.dispatchEvent(new Event('test'));
            const cancelableResult = target.dispatchEvent(new Event('test', { cancelable: true }));
            """
        );

        var normalResult = Evaluate("normalResult").AsBoolean();
        var cancelableResult = Evaluate("cancelableResult").AsBoolean();

        Assert.True(normalResult);
        Assert.True(cancelableResult);
    }

    [Fact]
    public void ShouldHandlePreventDefault()
    {
        Execute(
            """
            const target = new EventTarget();
            let result = null;
            target.addEventListener('test', (event) => {
                event.preventDefault();
            });
            result = target.dispatchEvent(new Event('test', { cancelable: true }));
            """
        );

        var result = Evaluate("result").AsBoolean();
        Assert.False(result);
    }

    [Fact]
    public void ShouldRemoveSpecificListener()
    {
        Execute(
            """
            const target = new EventTarget();
            let count1 = 0, count2 = 0;
            const handler1 = () => { count1++; };
            const handler2 = () => { count2++; };
            target.addEventListener('test', handler1);
            target.addEventListener('test', handler2);
            target.removeEventListener('test', handler1);
            target.dispatchEvent(new Event('test'));
            """
        );

        var count1 = Evaluate("count1").AsNumber();
        var count2 = Evaluate("count2").AsNumber();

        Assert.Equal(0, count1);
        Assert.Equal(1, count2);
    }

    [Fact]
    public void ShouldHandleOnEventProperties()
    {
        Execute(
            """
                const target = new EventTarget();
                let executed = false;
                target.ontest = () => { executed = true; };
                target.dispatchEvent(new Event('test'));
            """
        );

        var executed = Evaluate("executed").AsBoolean();
        Assert.True(executed);
    }

    [Fact]
    public void ShouldHandleComplexEventData()
    {
        Execute(
            """
            const target = new EventTarget();
            let receivedData = null;
            target.addEventListener('custom', (event) => {
                receivedData = {
                    type: event.type,
                    bubbles: event.bubbles,
                    cancelable: event.cancelable,
                    timeStamp: typeof event.timeStamp
                };
            });
            target.dispatchEvent(new Event('custom', { bubbles: true, cancelable: true }));
            """
        );

        var type = Evaluate("receivedData.type").AsString();
        var bubbles = Evaluate("receivedData.bubbles").AsBoolean();
        var cancelable = Evaluate("receivedData.cancelable").AsBoolean();
        var timeStampType = Evaluate("receivedData.timeStamp").AsString();

        Assert.Equal("custom", type);
        Assert.True(bubbles);
        Assert.True(cancelable);
        Assert.Equal("number", timeStampType);
    }
}
