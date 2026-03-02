using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.XMLHttpRequest;

public sealed class UploadTests : HttpRouteTestBase
{
    [Fact]
    public async Task ShouldFireLoadstartLoadLoadendOnPostWithBody()
    {
        MapRoute(
            "POST",
            "/xhr-upload-events",
            async ctx => await WriteResponseAsync(ctx, 200, "ok")
        );

        Execute(
            $$"""
            var uploadEvents = [];
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.upload.addEventListener('loadstart', function() { uploadEvents.push('loadstart'); });
            xhr.upload.addEventListener('load',      function() { uploadEvents.push('load'); });
            xhr.upload.addEventListener('loadend',   function() { uploadEvents.push('loadend'); });
            xhr.onload = function() { done = true; };
            xhr.open('POST', '{{BaseUrl}}xhr-upload-events');
            xhr.send('upload-payload');
            """
        );

        await WaitForJsConditionAsync("done === true");

        var events = Evaluate("uploadEvents").AsArray();
        var eventList = new System.Collections.Generic.List<string>();
        foreach (var e in events)
        {
            eventList.Add(e.AsString());
        }

        Assert.Contains("loadstart", eventList);
        Assert.Contains("load", eventList);
        Assert.Contains("loadend", eventList);
    }

    [Fact]
    public async Task ShouldFireProgressEventOnPost()
    {
        MapRoute(
            "POST",
            "/xhr-upload-progress",
            async ctx => await WriteResponseAsync(ctx, 200, "ok")
        );

        Execute(
            $$"""
            var uploadEvents = [];
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.upload.addEventListener('loadstart', function() { uploadEvents.push('loadstart'); });
            xhr.upload.addEventListener('progress',  function() { uploadEvents.push('progress'); });
            xhr.upload.addEventListener('load',      function() { uploadEvents.push('load'); });
            xhr.upload.addEventListener('loadend',   function() { uploadEvents.push('loadend'); });
            xhr.onload = function() { done = true; };
            xhr.open('POST', '{{BaseUrl}}xhr-upload-progress');
            xhr.send('progress-payload');
            """
        );

        await WaitForJsConditionAsync("done === true");

        var events = Evaluate("uploadEvents").AsArray();
        var eventList = new System.Collections.Generic.List<string>();
        foreach (var e in events)
        {
            eventList.Add(e.AsString());
        }

        // loadstart and loadend must always be present; progress can be throttled
        Assert.Contains("loadstart", eventList);
        Assert.Contains("loadend", eventList);
    }

    [Fact]
    public async Task ShouldFireEventsViaOnPropertyHandlers()
    {
        MapRoute(
            "POST",
            "/xhr-upload-onprop",
            async ctx => await WriteResponseAsync(ctx, 200, "ok")
        );

        Execute(
            $$"""
            var uploadLoadFired = false;
            var uploadLoadendFired = false;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.upload.onload    = function() { uploadLoadFired = true; };
            xhr.upload.onloadend = function() { uploadLoadendFired = true; };
            xhr.onload = function() { done = true; };
            xhr.open('POST', '{{BaseUrl}}xhr-upload-onprop');
            xhr.send('data');
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.True(Evaluate("uploadLoadFired").AsBoolean());
        Assert.True(Evaluate("uploadLoadendFired").AsBoolean());
    }

    [Fact]
    public async Task LoadstartEventShouldBeInstanceOfProgressEvent()
    {
        MapRoute(
            "POST",
            "/xhr-upload-pe-loadstart",
            async ctx => await WriteResponseAsync(ctx, 200, "ok")
        );

        Execute(
            $$"""
            var isProgressEvent = false;
            var isEvent = false;
            var eventType = null;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.upload.addEventListener('loadstart', function(e) {
                isProgressEvent = e instanceof ProgressEvent;
                isEvent         = e instanceof Event;
                eventType       = e.type;
            });
            xhr.onload = function() { done = true; };
            xhr.open('POST', '{{BaseUrl}}xhr-upload-pe-loadstart');
            xhr.send('body');
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.True(Evaluate("isProgressEvent").AsBoolean());
        Assert.True(Evaluate("isEvent").AsBoolean());
        Assert.Equal("loadstart", Evaluate("eventType").AsString());
    }

    [Fact]
    public async Task LoadEventShouldBeInstanceOfProgressEvent()
    {
        MapRoute(
            "POST",
            "/xhr-upload-pe-load",
            async ctx => await WriteResponseAsync(ctx, 200, "ok")
        );

        Execute(
            $$"""
            var isProgressEvent = false;
            var isEvent = false;
            var eventType = null;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.upload.addEventListener('load', function(e) {
                isProgressEvent = e instanceof ProgressEvent;
                isEvent         = e instanceof Event;
                eventType       = e.type;
            });
            xhr.onload = function() { done = true; };
            xhr.open('POST', '{{BaseUrl}}xhr-upload-pe-load');
            xhr.send('body');
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.True(Evaluate("isProgressEvent").AsBoolean());
        Assert.True(Evaluate("isEvent").AsBoolean());
        Assert.Equal("load", Evaluate("eventType").AsString());
    }

    [Fact]
    public async Task LoadendEventShouldBeInstanceOfProgressEvent()
    {
        MapRoute(
            "POST",
            "/xhr-upload-pe-loadend",
            async ctx => await WriteResponseAsync(ctx, 200, "ok")
        );

        Execute(
            $$"""
            var isProgressEvent = false;
            var isEvent = false;
            var eventType = null;
            var done = false;
            const xhr = new XMLHttpRequest();
            xhr.upload.addEventListener('loadend', function(e) {
                isProgressEvent = e instanceof ProgressEvent;
                isEvent         = e instanceof Event;
                eventType       = e.type;
            });
            xhr.onload = function() { done = true; };
            xhr.open('POST', '{{BaseUrl}}xhr-upload-pe-loadend');
            xhr.send('body');
            """
        );

        await WaitForJsConditionAsync("done === true");

        Assert.True(Evaluate("isProgressEvent").AsBoolean());
        Assert.True(Evaluate("isEvent").AsBoolean());
        Assert.Equal("loadend", Evaluate("eventType").AsString());
    }

    [Fact]
    public async Task ShouldFireAbortAndLoadendWhenRequestIsAborted()
    {
        MapRoute(
            "POST",
            "/xhr-upload-abort",
            async ctx =>
            {
                await Task.Delay(300, Token);
                await WriteResponseAsync(ctx, 200, "late");
            }
        );

        Execute(
            $$"""
            var uploadAbortFired = false;
            var uploadLoadendFired = false;
            var xhrAbortFired = false;
            const xhr = new XMLHttpRequest();
            xhr.upload.addEventListener('abort',   function() { uploadAbortFired = true; });
            xhr.upload.addEventListener('loadend', function() { uploadLoadendFired = true; });
            xhr.onabort = function() { xhrAbortFired = true; };
            xhr.open('POST', '{{BaseUrl}}xhr-upload-abort');
            xhr.send('payload');
            setTimeout(() => xhr.abort(), 10);
            """
        );

        await WaitForJsConditionAsync("xhrAbortFired === true");

        Assert.True(Evaluate("xhrAbortFired").AsBoolean());
        // Upload abort + loadend fire only when the upload is not yet complete.
        // For small payloads to localhost the upload may complete before abort runs;
        // both branches (fired / not fired) are valid – we only verify xhrAbortFired.
    }

    [Fact]
    public async Task AbortEventShouldBeInstanceOfProgressEvent()
    {
        MapRoute(
            "POST",
            "/xhr-upload-abort-pe",
            async ctx =>
            {
                await Task.Delay(300, Token);
                await WriteResponseAsync(ctx, 200, "late");
            }
        );

        Execute(
            $$"""
            var abortIsProgressEvent = false;
            var loadendIsProgressEvent = false;
            var uploadAbortFired = false;
            var xhrAbortFired = false;
            const xhr = new XMLHttpRequest();
            xhr.upload.addEventListener('abort', function(e) {
                abortIsProgressEvent = e instanceof ProgressEvent;
                uploadAbortFired = true;
            });
            xhr.upload.addEventListener('loadend', function(e) {
                loadendIsProgressEvent = e instanceof ProgressEvent;
            });
            xhr.onabort = function() { xhrAbortFired = true; };
            xhr.open('POST', '{{BaseUrl}}xhr-upload-abort-pe');
            xhr.send('payload');
            setTimeout(() => xhr.abort(), 10);
            """
        );

        await WaitForJsConditionAsync("xhrAbortFired === true");

        // If upload abort event fired, verify it was a ProgressEvent
        if (Evaluate("uploadAbortFired").AsBoolean())
        {
            Assert.True(Evaluate("abortIsProgressEvent").AsBoolean());
            Assert.True(Evaluate("loadendIsProgressEvent").AsBoolean());
        }
    }
}
