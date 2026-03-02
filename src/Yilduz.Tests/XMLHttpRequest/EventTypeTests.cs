using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.XMLHttpRequest;

public sealed class EventTypeTests : HttpRouteTestBase
{
    [Fact]
    public async Task LoadEventShouldBeInstanceOfProgressEvent()
    {
        MapGet("/xhr-load-pe", async ctx => await WriteResponseAsync(ctx, 200, "hi"));

        Execute(
            $$"""
            var isProgressEvent = false;
            var isEvent = false;
            var eventType = null;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.addEventListener('load', function(e) {
                isProgressEvent = e instanceof ProgressEvent;
                isEvent         = e instanceof Event;
                eventType       = e.type;
                done = true;
            });
            xhr.open('GET', '{{BaseUrl}}xhr-load-pe');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.True(Evaluate("isProgressEvent").AsBoolean());
        Assert.True(Evaluate("isEvent").AsBoolean());
        Assert.Equal("load", Evaluate("eventType").AsString());
    }

    [Fact]
    public async Task ProgressEventShouldBeInstanceOfProgressEvent()
    {
        MapGet(
            "/xhr-progress-pe",
            async ctx => await WriteResponseAsync(ctx, 200, "progress-body")
        );

        Execute(
            $$"""
            var progressIsProgressEvent = false;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.onprogress = function(e) {
                progressIsProgressEvent = e instanceof ProgressEvent;
            };
            xhr.onload = function() { done = true; };
            xhr.open('GET', '{{BaseUrl}}xhr-progress-pe');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.True(Evaluate("progressIsProgressEvent").AsBoolean());
    }

    [Fact]
    public async Task ReadystatechangeEventShouldBeEventNotProgressEvent()
    {
        MapGet("/xhr-rsc-event", async ctx => await WriteResponseAsync(ctx, 200, "data"));

        Execute(
            $$"""
            var isEvent = false;
            var isProgressEvent = false;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.addEventListener('readystatechange', function(e) {
                if (xhr.readyState === 4) {
                    isEvent         = e instanceof Event;
                    isProgressEvent = e instanceof ProgressEvent;
                    done = true;
                }
            });
            xhr.open('GET', '{{BaseUrl}}xhr-rsc-event');
            xhr.send();
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.True(Evaluate("isEvent").AsBoolean());
        Assert.False(Evaluate("isProgressEvent").AsBoolean());
    }
}
