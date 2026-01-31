using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.FileReader;

public sealed class EventTests : TestBase
{
    [Fact]
    public async Task ShouldHandleEventListenerRemoval()
    {
        Execute(
            """
            const blob = new Blob(['Test removal']);
            const reader = new FileReader();
            let eventCount = 0;

            function eventHandler() {
                eventCount++;
            }

            reader.addEventListener('load', eventHandler);
            reader.addEventListener('load', eventHandler);
            reader.removeEventListener('load', eventHandler);

            reader.readAsText(blob);
            """
        );

        await Task.Delay(100);
        Assert.Equal(0, Evaluate("eventCount").AsNumber());
    }

    [Fact]
    public async Task ShouldMaintainEventOrder()
    {
        Execute(
            """
            const blob = new Blob(['Event order test']);
            const reader = new FileReader();
            let eventOrder = [];

            reader.addEventListener('loadstart', () => { eventOrder.push('loadstart'); });
            reader.addEventListener('progress', () => { eventOrder.push('progress'); });
            reader.addEventListener('load', () => { eventOrder.push('load'); });
            reader.addEventListener('loadend', () => { eventOrder.push('loadend'); });

            reader.readAsText(blob);
            """
        );

        await Task.Delay(100);
        Assert.Equal("loadstart", Evaluate("eventOrder[0]").AsString());
        Assert.Equal("loadend", Evaluate("eventOrder[eventOrder.length - 1]").AsString());
    }

    [Fact]
    public async Task ShouldSupportOnEventProperties()
    {
        Execute(
            """
            const blob = new Blob(['OnEvent test']);
            const reader = new FileReader();
            let eventsFired = {
                loadstart: false,
                progress: false,
                load: false,
                loadend: false,
                error: false,
                abort: false
            };

            reader.onloadstart = () => { eventsFired.loadstart = true; };
            reader.onprogress = () => { eventsFired.progress = true; };
            reader.onload = () => { eventsFired.load = true; };
            reader.onloadend = () => { eventsFired.loadend = true; };

            reader.readAsText(blob);
            """
        );

        await Task.Delay(100);

        Assert.True(Evaluate("eventsFired.load").AsBoolean());
        Assert.True(Evaluate("eventsFired.loadend").AsBoolean());
        Assert.True(Evaluate("eventsFired.loadstart").AsBoolean());
        Assert.True(Evaluate("eventsFired.progress").AsBoolean());
    }

    [Fact]
    public void ShouldSupportEventListeners()
    {
        Execute(
            """
            const reader = new FileReader();
            let eventsFired = {
                loadstart: false,
                progress: false,
                load: false,
                loadend: false,
                error: false,
                abort: false
            };

            reader.addEventListener('loadstart', () => { eventsFired.loadstart = true; });
            reader.addEventListener('progress', () => { eventsFired.progress = true; });
            reader.addEventListener('load', () => { eventsFired.load = true; });
            reader.addEventListener('loadend', () => { eventsFired.loadend = true; });
            reader.addEventListener('error', () => { eventsFired.error = true; });
            reader.addEventListener('abort', () => { eventsFired.abort = true; });
            """
        );

        Assert.True(Evaluate("typeof reader.addEventListener === 'function'").AsBoolean());
        Assert.True(Evaluate("typeof reader.removeEventListener === 'function'").AsBoolean());
    }

    [Fact]
    public async Task ShouldFireLoadStartEvent()
    {
        Execute(
            """
            const blob = new Blob(['Test content']);
            const reader = new FileReader();
            let loadStartFired = false;

            reader.addEventListener('loadstart', () => {
                loadStartFired = true;
            });

            reader.readAsText(blob);
            """
        );

        await Task.Delay(50);
        Assert.True(Evaluate("loadStartFired").AsBoolean());
    }

    [Fact]
    public async Task ShouldFireLoadEndEvent()
    {
        Execute(
            """
            const blob = new Blob(['Test content']);
            const reader = new FileReader();
            let loadEndFired = false;

            reader.addEventListener('loadend', () => {
                loadEndFired = true;
            });
            reader.readAsText(blob);
            """
        );

        await WaitForJsConditionAsync("loadEndFired === true");

        // Explicit assertion for test clarity and documentation
        Assert.True(Evaluate("loadEndFired").AsBoolean());
    }

    [Fact]
    public async Task ShouldHandleProgressEvents()
    {
        Execute(
            """
            const blob = new Blob(['Test content for progress']);
            const reader = new FileReader();
            let progressEventData = null;

            reader.addEventListener('progress', (event) => {
                progressEventData = {
                    lengthComputable: event.lengthComputable,
                    loaded: event.loaded,
                    total: event.total,
                    type: event.type
                };
            });

            reader.readAsText(blob);
            """
        );

        await Task.Delay(100);

        Assert.True(Evaluate("progressEventData !== null").AsBoolean());
        Assert.Equal("progress", Evaluate("progressEventData.type").AsString());
    }
}
