using System;
using Jint;
using Xunit;

namespace Yilduz.Tests.WebSocket;

public sealed class BaseUrlTests : TestBase
{
    protected override Options GetOptions()
    {
        return new() { CancellationToken = Token, BaseUrl = new Uri("https://example.test/app/") };
    }

    [Fact]
    public void ShouldResolveRelativeUrlAgainstBaseUrl()
    {
        Execute("const ws = new WebSocket('/chat');");

        Assert.Equal("wss://example.test/chat", Evaluate("ws.url").AsString());
    }

    [Fact]
    public void ShouldResolvePathRelativeUrlAgainstBaseUrl()
    {
        Execute("const ws = new WebSocket('room');");

        Assert.Equal("wss://example.test/app/room", Evaluate("ws.url").AsString());
    }

    [Fact]
    public void ShouldIgnoreBaseUrlForAbsoluteWsUrl()
    {
        Execute("const ws = new WebSocket('ws://localhost:18080/socket');");

        Assert.Equal("ws://localhost:18080/socket", Evaluate("ws.url").AsString());
    }
}
