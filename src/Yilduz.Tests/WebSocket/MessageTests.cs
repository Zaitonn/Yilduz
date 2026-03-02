using System;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.WebSocket;

public sealed class MessageTests : WebSocketTestBase
{
    [Fact]
    public async Task ShouldReceiveTextMessageFromServer()
    {
        _clientConnected = socket => socket.Send("hello from server");

        Execute(
            $$"""
            let received = null;
            const ws = new WebSocket('{{WsUrl}}');
            ws.onmessage = (e) => { received = e.data; };
            """
        );

        await WaitForJsConditionAsync("received !== null");
        Assert.Equal("hello from server", Evaluate("received").AsString());
    }

    [Fact]
    public async Task ShouldSendTextMessageToServer()
    {
        string? serverReceived = null;
        _messageReceived = (_, msg) => serverReceived = msg;

        Execute(
            $$"""
            const ws = new WebSocket('{{WsUrl}}');
            ws.onopen = () => { ws.send('hello from client'); };
            """
        );

        await WaitForJsConditionAsync("ws.readyState === 1");

        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (serverReceived is null && DateTime.UtcNow < deadline)
        {
            await Task.Delay(10);
        }

        Assert.Equal("hello from client", serverReceived);
    }

    [Fact]
    public async Task ShouldEchoTextMessage()
    {
        _messageReceived = (socket, msg) => _ = socket.Send("echo: " + msg);

        Execute(
            $$"""
            let echo = null;
            const ws = new WebSocket('{{WsUrl}}');
            ws.onopen = () => { ws.send('ping'); };
            ws.onmessage = (e) => { echo = e.data; };
            """
        );

        await WaitForJsConditionAsync("echo !== null");
        Assert.Equal("echo: ping", Evaluate("echo").AsString());
    }

    [Fact]
    public async Task ShouldReceiveMultipleMessages()
    {
        _clientConnected = socket =>
        {
            _ = socket.Send("msg1");
            _ = socket.Send("msg2");
            _ = socket.Send("msg3");
        };

        Execute(
            $$"""
            let messages = [];
            const ws = new WebSocket('{{WsUrl}}');
            ws.onmessage = (e) => { messages.push(e.data); };
            """
        );

        await WaitForJsConditionAsync("messages.length >= 3");
        Assert.Equal(3, Evaluate("messages.length").AsNumber());
        Assert.Equal("msg1", Evaluate("messages[0]").AsString());
        Assert.Equal("msg2", Evaluate("messages[1]").AsString());
        Assert.Equal("msg3", Evaluate("messages[2]").AsString());
    }

    [Fact]
    public async Task ShouldReceiveBinaryMessageAsBlob()
    {
        _clientConnected = socket => _ = socket.Send([1, 2, 3, 4]);

        Execute(
            $$"""
            let binaryReceived = null;
            const ws = new WebSocket('{{WsUrl}}');
            ws.binaryType = 'blob';
            ws.onmessage = (e) => { binaryReceived = e.data; };
            """
        );

        await WaitForJsConditionAsync("binaryReceived !== null");
        Assert.True(Evaluate("binaryReceived instanceof Blob").AsBoolean());
        Assert.Equal(4, Evaluate("binaryReceived.size").AsNumber());
    }

    [Fact]
    public async Task ShouldReceiveBinaryMessageAsArrayBuffer()
    {
        _clientConnected = socket => _ = socket.Send([10, 20, 30]);

        Execute(
            $$"""
            let buffer = null;
            const ws = new WebSocket('{{WsUrl}}');
            ws.binaryType = 'arraybuffer';
            ws.onmessage = (e) => { buffer = e.data; };
            """
        );

        await WaitForJsConditionAsync("buffer !== null");
        Assert.True(Evaluate("buffer instanceof ArrayBuffer").AsBoolean());
        Assert.Equal(3, Evaluate("buffer.byteLength").AsNumber());
    }

    [Fact]
    public async Task ShouldSendBinaryDataAsArrayBuffer()
    {
        byte[]? serverReceived = null;
        var received = new SemaphoreSlim(0, 1);
        _binaryMessageReceived = (_, data) =>
        {
            serverReceived = data;
            received.Release();
        };

        Execute(
            $$"""
            const ws = new WebSocket('{{WsUrl}}');
            ws.onopen = () => {
                const buf = new ArrayBuffer(3);
                const view = new Uint8Array(buf);
                view[0] = 7; view[1] = 8; view[2] = 9;
                ws.send(buf);
            };
            """
        );

        await WaitForJsConditionAsync("ws.readyState === 1");
        var signalled = await received.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(signalled);
        Assert.Equal(new byte[] { 7, 8, 9 }, serverReceived);
    }

    [Fact]
    public async Task ShouldSupportAddEventListenerForMessage()
    {
        _clientConnected = socket => _ = socket.Send("via-listener");

        Execute(
            $$"""
            let received = null;
            const ws = new WebSocket('{{WsUrl}}');
            ws.addEventListener('message', (e) => { received = e.data; });
            """
        );

        await WaitForJsConditionAsync("received !== null");
        Assert.Equal("via-listener", Evaluate("received").AsString());
    }

    [Fact]
    public async Task ShouldThrowWhenSendingInConnectingState()
    {
        Execute(
            $$"""
            let error = null;
            const ws = new WebSocket('{{WsUrl}}');
            // readyState is CONNECTING (0) right after construction
            try { ws.send('too early'); }
            catch(e) { error = e.name; }
            """
        );

        // No async wait needed – the error is thrown synchronously
        await Task.CompletedTask;
        Assert.Equal("InvalidStateError", Evaluate("error").AsString());
    }
}
