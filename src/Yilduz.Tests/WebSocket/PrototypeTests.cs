using Jint;
using Xunit;

namespace Yilduz.Tests.WebSocket;

public sealed class PrototypeTests : WebSocketTestBase
{
    [Fact]
    public void StaticConstantsShouldHaveCorrectValues()
    {
        Assert.Equal(0, Evaluate("WebSocket.CONNECTING"));
        Assert.Equal(1, Evaluate("WebSocket.OPEN"));
        Assert.Equal(2, Evaluate("WebSocket.CLOSING"));
        Assert.Equal(3, Evaluate("WebSocket.CLOSED"));
    }

    [Fact]
    public void StaticConstantsShouldAlsoExistOnPrototype()
    {
        Assert.Equal(0, Evaluate("WebSocket.prototype.CONNECTING"));
        Assert.Equal(1, Evaluate("WebSocket.prototype.OPEN"));
        Assert.Equal(2, Evaluate("WebSocket.prototype.CLOSING"));
        Assert.Equal(3, Evaluate("WebSocket.prototype.CLOSED"));
    }

    [Fact]
    public void InstanceShouldBeInstanceofWebSocket()
    {
        Execute($$"""const ws = new WebSocket('{{WsUrl}}');""");
        Assert.True(Evaluate("ws instanceof WebSocket").AsBoolean());
    }

    [Fact]
    public void InstanceShouldBeInstanceofEventTarget()
    {
        Execute($$"""const ws = new WebSocket('{{WsUrl}}');""");
        Assert.True(Evaluate("ws instanceof EventTarget").AsBoolean());
    }

    [Fact]
    public void ShouldExposeExpectedMethods()
    {
        Execute($$"""const ws = new WebSocket('{{WsUrl}}');""");
        Assert.Equal("function", Evaluate("typeof ws.send").AsString());
        Assert.Equal("function", Evaluate("typeof ws.close").AsString());
        Assert.Equal("function", Evaluate("typeof ws.addEventListener").AsString());
        Assert.Equal("function", Evaluate("typeof ws.removeEventListener").AsString());
        Assert.Equal("function", Evaluate("typeof ws.dispatchEvent").AsString());
    }

    [Fact]
    public void ShouldExposeExpectedReadOnlyProperties()
    {
        Execute($$"""const ws = new WebSocket('{{WsUrl}}');""");
        // url, readyState, bufferedAmount, extensions, protocol all exist
        Assert.NotEqual("undefined", Evaluate("typeof ws.url").AsString());
        Assert.NotEqual("undefined", Evaluate("typeof ws.readyState").AsString());
        Assert.NotEqual("undefined", Evaluate("typeof ws.bufferedAmount").AsString());
        Assert.NotEqual("undefined", Evaluate("typeof ws.extensions").AsString());
        Assert.NotEqual("undefined", Evaluate("typeof ws.protocol").AsString());
    }

    [Fact]
    public void ShouldExposeEventHandlerProperties()
    {
        Execute($$"""const ws = new WebSocket('{{WsUrl}}');""");
        Assert.NotEqual("undefined", Evaluate("typeof ws.onopen").AsString());
        Assert.NotEqual("undefined", Evaluate("typeof ws.onmessage").AsString());
        Assert.NotEqual("undefined", Evaluate("typeof ws.onerror").AsString());
        Assert.NotEqual("undefined", Evaluate("typeof ws.onclose").AsString());
    }

    [Fact]
    public void ConstructorReferenceShouldBeWebSocket()
    {
        Execute($$"""const ws = new WebSocket('{{WsUrl}}');""");
        Assert.True(Evaluate("ws.constructor === WebSocket").AsBoolean());
    }

    [Fact]
    public void ShouldAllowSettingBinaryTypeToArraybuffer()
    {
        Execute(
            $$"""
            const ws = new WebSocket('{{WsUrl}}');
            ws.binaryType = 'arraybuffer';
            """
        );
        Assert.Equal("arraybuffer", Evaluate("ws.binaryType").AsString());
    }

    [Fact]
    public void ShouldAllowResettingBinaryTypeToBlob()
    {
        Execute(
            $$"""
            const ws = new WebSocket('{{WsUrl}}');
            ws.binaryType = 'arraybuffer';
            ws.binaryType = 'blob';
            """
        );
        Assert.Equal("blob", Evaluate("ws.binaryType").AsString());
    }

    [Fact]
    public void ShouldThrowTypeErrorForInvalidBinaryType()
    {
        Execute(
            $$"""
            const ws = new WebSocket('{{WsUrl}}');
            let err = null;
            try { ws.binaryType = 'text'; }
            catch (e) { err = e; }
            """
        );
        Assert.Equal("TypeError", Evaluate("err.name").AsString());
        // value must remain unchanged
        Assert.Equal("blob", Evaluate("ws.binaryType").AsString());
    }

    [Fact]
    public void ShouldThrowForCloseCodeOutOfRange()
    {
        Execute(
            $$"""
            const ws = new WebSocket('{{WsUrl}}');
            let err = null;
            try { ws.close(2999); }
            catch (e) { err = e; }
            """
        );
        Assert.Equal("InvalidAccessError", Evaluate("err.name").AsString());
    }

    [Fact]
    public void ShouldThrowForCloseCodeAboveRange()
    {
        Execute(
            $$"""
            const ws = new WebSocket('{{WsUrl}}');
            let err = null;
            try { ws.close(5000); }
            catch (e) { err = e; }
            """
        );
        Assert.Equal("InvalidAccessError", Evaluate("err.name").AsString());
    }

    [Fact]
    public void ShouldAcceptCloseCode1000()
    {
        Execute(
            $$"""
            const ws = new WebSocket('{{WsUrl}}');
            let err = null;
            try { ws.close(1000); }
            catch (e) { err = e; }
            """
        );
        Assert.True(Evaluate("err === null").AsBoolean());
    }

    [Fact]
    public void ShouldAcceptCloseCodeInCustomRange()
    {
        Execute(
            $$"""
            const ws = new WebSocket('{{WsUrl}}');
            let err = null;
            try { ws.close(3000); }
            catch (e) { err = e; }
            """
        );
        Assert.True(Evaluate("err === null").AsBoolean());
    }

    [Fact]
    public void ShouldThrowForCloseSyntaxErrorWhenReasonTooLong()
    {
        // 124 ASCII chars = 124 UTF-8 bytes, which exceeds the 123-byte limit.
        var reason = new string('x', 124);
        Execute(
            $$"""
            const ws = new WebSocket('{{WsUrl}}');
            let err = null;
            try { ws.close(1000, '{{reason}}'); }
            catch (e) { err = e; }
            """
        );
        Assert.Equal("SyntaxError", Evaluate("err.name").AsString());
    }

    [Fact]
    public void ShouldAcceptCloseReasonExactlyAt123Bytes()
    {
        var reason = new string('x', 123);
        Execute(
            $$"""
            const ws = new WebSocket('{{WsUrl}}');
            let err = null;
            try { ws.close(1000, '{{reason}}'); }
            catch (e) { err = e; }
            """
        );
        Assert.True(Evaluate("err === null").AsBoolean());
    }
}
