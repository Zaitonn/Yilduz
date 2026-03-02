using System.Threading.Tasks;
using Fleck;
using Jint;
using Xunit;

namespace Yilduz.Tests.WebSocket;

public sealed class BasicTests : WebSocketTestBase
{
    [Fact]
    public async Task ShouldConnectAndFireOpenEvent()
    {
        Execute(
            $$"""
            let opened = false;
            const ws = new WebSocket('{{WsUrl}}');
            ws.onopen = () => { opened = true; };
            """
        );

        await WaitForJsConditionAsync("opened === true");
        Assert.True(Evaluate("opened").AsBoolean());
    }

    [Fact]
    public async Task ShouldConnectUsingHttpScheme()
    {
        Execute(
            $$"""
            let opened = false;
            const ws = new WebSocket('{{WsUrl.Replace("ws://", "http://")}}');
            ws.onopen = () => { opened = true; };
            """
        );

        await WaitForJsConditionAsync("opened === true");
        Assert.True(Evaluate("opened").AsBoolean());
    }

    [Fact]
    public async Task ShouldExposeCorrectReadyStateAfterOpen()
    {
        Execute(
            $$"""
            let readyState = -1;
            const ws = new WebSocket('{{WsUrl}}');
            ws.onopen = () => { readyState = ws.readyState; };
            """
        );

        await WaitForJsConditionAsync("readyState !== -1");
        // OPEN = 1
        Assert.Equal(1, Evaluate("readyState").AsNumber());
    }

    [Fact]
    public void ShouldExposeUrlProperty()
    {
        Execute(
            $$"""
            const ws = new WebSocket('{{WsUrl}}');
            """
        );

        // url is set synchronously on construction; the implementation normalises it with a trailing slash
        Assert.Equal(WsUrl + "/", Evaluate("ws.url").AsString());
    }

    [Fact]
    public void ShouldHaveConnectingReadyStateInitially()
    {
        Execute(
            $$"""
            const ws = new WebSocket('{{WsUrl}}');
            const initialState = ws.readyState;
            """
        );

        // CONNECTING = 0
        Assert.Equal(0, Evaluate("initialState").AsNumber());
    }

    [Fact]
    public async Task ShouldFireCloseEventWhenServerClosesConnection()
    {
        IWebSocketConnection? serverSocket = null;
        _clientConnected = socket => serverSocket = socket;

        Execute(
            $$"""
            let closed = false;
            let closeCode = -1;
            const ws = new WebSocket('{{WsUrl}}');
            ws.onclose = (e) => {
                closed = true;
                closeCode = e.code;
            };
            """
        );

        // Wait for connection to be established on the server side.
        await WaitForJsConditionAsync("ws.readyState === 1");

        // Server initiates close.
        serverSocket!.Close();

        await WaitForJsConditionAsync("closed === true");
        Assert.True(Evaluate("closed").AsBoolean());
    }

    [Fact]
    public async Task ShouldFireErrorEventOnInvalidUrl()
    {
        Execute(
            """
            let errored = false;
            const ws = new WebSocket('ws://localhost:1');
            ws.onerror = () => { errored = true; };
            """
        );

        await WaitForJsConditionAsync("errored === true");
        Assert.True(Evaluate("errored").AsBoolean());
    }

    [Fact]
    public async Task ShouldSupportAddEventListenerForOpen()
    {
        Execute(
            $$"""
            let opened = false;
            const ws = new WebSocket('{{WsUrl}}');
            ws.addEventListener('open', () => { opened = true; });
            """
        );

        await WaitForJsConditionAsync("opened === true");
        Assert.True(Evaluate("opened").AsBoolean());
    }

    [Fact]
    public async Task ShouldCloseWithCodeAndFireCloseEvent()
    {
        Execute(
            $$"""
            let closed = false;
            let closeCode = -1;
            const ws = new WebSocket('{{WsUrl}}');
            ws.onopen = () => { ws.close(1000, 'done'); };
            ws.onclose = (e) => {
                closed = true;
                closeCode = e.code;
            };
            """
        );

        await WaitForJsConditionAsync("closed === true");
        Assert.True(Evaluate("closed").AsBoolean());
        Assert.Equal(1000, Evaluate("closeCode").AsNumber());
    }

    [Fact]
    public async Task ShouldHaveClosedReadyStateAfterClose()
    {
        Execute(
            $$"""
            let finalState = -1;
            const ws = new WebSocket('{{WsUrl}}');
            ws.onopen = () => { ws.close(); };
            ws.onclose = () => { finalState = ws.readyState; };
            """
        );

        await WaitForJsConditionAsync("finalState !== -1");
        // CLOSED = 3
        Assert.Equal(3, Evaluate("finalState").AsNumber());
    }
}
