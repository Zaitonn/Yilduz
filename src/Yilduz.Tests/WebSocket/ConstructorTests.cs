using Jint;
using Xunit;

namespace Yilduz.Tests.WebSocket;

public sealed class ConstructorTests : WebSocketTestBase
{
    [Fact]
    public void ShouldThrowWhenNoArgumentsGiven()
    {
        var ex = Assert.Throws<Jint.Runtime.JavaScriptException>(() => Execute("new WebSocket()"));
        Assert.Equal("TypeError", ex.Error.Get("name").AsString());
    }

    [Fact]
    public void ShouldThrowSyntaxErrorForInvalidUrl()
    {
        Execute(
            """
            let err = null;
            try { new WebSocket('not a url at all'); }
            catch (e) { err = e; }
            """
        );
        Assert.Equal("SyntaxError", Evaluate("err.name").AsString());
    }

    [Fact]
    public void ShouldThrowSyntaxErrorForFtpScheme()
    {
        Execute(
            """
            let err = null;
            try { new WebSocket('ftp://example.com'); }
            catch (e) { err = e; }
            """
        );
        Assert.Equal("SyntaxError", Evaluate("err.name").AsString());
    }

    [Fact]
    public void ShouldThrowSyntaxErrorForUrlWithFragment()
    {
        Execute(
            $$"""
            let err = null;
            try { new WebSocket('{{WsUrl}}#section'); }
            catch (e) { err = e; }
            """
        );
        Assert.Equal("SyntaxError", Evaluate("err.name").AsString());
    }

    [Fact]
    public void ShouldThrowSyntaxErrorForDuplicateCaseSensitiveProtocols()
    {
        Execute(
            $$"""
            let err = null;
            try { new WebSocket('{{WsUrl}}', ['chat', 'chat']); }
            catch (e) { err = e; }
            """
        );
        Assert.Equal("SyntaxError", Evaluate("err.name").AsString());
    }

    [Fact]
    public void ShouldThrowSyntaxErrorForDuplicateCaseInsensitiveProtocols()
    {
        Execute(
            $$"""
            let err = null;
            try { new WebSocket('{{WsUrl}}', ['chat', 'CHAT']); }
            catch (e) { err = e; }
            """
        );
        Assert.Equal("SyntaxError", Evaluate("err.name").AsString());
    }

    [Fact]
    public void ShouldAcceptStringProtocol()
    {
        // Constructing with a string protocol must not throw.
        Execute(
            $$"""
            let err = null;
            try { new WebSocket('{{WsUrl}}', 'chat'); }
            catch (e) { err = e; }
            """
        );
        Assert.True(Evaluate("err === null").AsBoolean());
    }

    [Fact]
    public void ShouldAcceptArrayOfProtocols()
    {
        Execute(
            $$"""
            let err = null;
            try { new WebSocket('{{WsUrl}}', ['chat', 'superchat']); }
            catch (e) { err = e; }
            """
        );
        Assert.True(Evaluate("err === null").AsBoolean());
    }

    [Fact]
    public void ShouldAcceptWssScheme()
    {
        Execute(
            """
            let err = null;
            try { new WebSocket('wss://example.com'); }
            catch (e) { err = e; }
            """
        );
        Assert.True(Evaluate("err === null").AsBoolean());
    }

    [Fact]
    public void ShouldHaveReadyStateConnectingAfterConstruction()
    {
        Execute($$"""const ws = new WebSocket('{{WsUrl}}');""");
        Assert.Equal(0, Evaluate("ws.readyState").AsNumber());
    }

    [Fact]
    public void ShouldHaveBufferedAmountZeroAfterConstruction()
    {
        Execute($$"""const ws = new WebSocket('{{WsUrl}}');""");
        Assert.Equal(0, Evaluate("ws.bufferedAmount").AsNumber());
    }

    [Fact]
    public void ShouldHaveEmptyExtensionsAfterConstruction()
    {
        Execute($$"""const ws = new WebSocket('{{WsUrl}}');""");
        Assert.Equal(string.Empty, Evaluate("ws.extensions").AsString());
    }

    [Fact]
    public void ShouldHaveEmptyProtocolAfterConstruction()
    {
        Execute($$"""const ws = new WebSocket('{{WsUrl}}');""");
        Assert.Equal(string.Empty, Evaluate("ws.protocol").AsString());
    }

    [Fact]
    public void ShouldHaveBlobBinaryTypeByDefault()
    {
        Execute($$"""const ws = new WebSocket('{{WsUrl}}');""");
        Assert.Equal("blob", Evaluate("ws.binaryType").AsString());
    }

    [Fact]
    public void ShouldHaveNullOnHandlersByDefault()
    {
        Execute($$"""const ws = new WebSocket('{{WsUrl}}');""");
        Assert.True(Evaluate("ws.onopen === null").AsBoolean());
        Assert.True(Evaluate("ws.onmessage === null").AsBoolean());
        Assert.True(Evaluate("ws.onerror === null").AsBoolean());
        Assert.True(Evaluate("ws.onclose === null").AsBoolean());
    }
}
