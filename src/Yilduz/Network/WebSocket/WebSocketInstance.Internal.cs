using System;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Jint.Native;
using SystemEncoding = System.Text.Encoding;

namespace Yilduz.Network.WebSocket;

public sealed partial class WebSocketInstance
{
    private readonly ClientWebSocket _client;
    private readonly CancellationTokenSource _cancellationTokenSource;

    private async Task ConnectAsync()
    {
        var token = _cancellationTokenSource.Token;

#if NET7_0_OR_GREATER
        _client.Options.CollectHttpResponseDetails = true;
#endif

        try
        {
            foreach (var protocol in _protocols)
            {
                _client.Options.AddSubProtocol(protocol);
            }

            await _client.ConnectAsync(new Uri(Url), token).ConfigureAwait(false);

            Protocol = _client.SubProtocol ?? string.Empty;

#if NET7_0_OR_GREATER
            Extensions =
                _client.HttpResponseHeaders is not null
                && _client.HttpResponseHeaders.TryGetValue(
                    "Sec-WebSocket-Extensions",
                    out var extensions
                )
                    ? string.Join(", ", extensions)
                    : null;
#endif

            _webApiIntrinsics.EventLoop.QueueMacrotask(() =>
            {
                ReadyState = WebSocketReadyState.Open;
                FireSimpleEvent("open");
            });

            await ReceiveLoopAsync(token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Engine is shutting down — cleanup is handled by the CTS registration.
        }
        catch
        {
            _webApiIntrinsics.EventLoop.QueueMacrotask(() =>
            {
                ReadyState = WebSocketReadyState.Closed;

                FireSimpleEvent("error");
                FireCloseEvent(false, 1006, string.Empty);
            });
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken token)
    {
        var buffer = new byte[4096];

        while (!token.IsCancellationRequested)
        {
            using var messageBuffer = new MemoryStream();
            WebSocketReceiveResult result;

            do
            {
                result = await _client
                    .ReceiveAsync(new ArraySegment<byte>(buffer), token)
                    .ConfigureAwait(false);

                messageBuffer.Write(buffer, 0, result.Count);
            } while (!result.EndOfMessage);

            if (result.MessageType == WebSocketMessageType.Close)
            {
                var closeStatus = (ushort)(
                    result.CloseStatus ?? WebSocketCloseStatus.NormalClosure
                );
                var closeDesc = result.CloseStatusDescription ?? string.Empty;

                try
                {
                    await _client
                        .CloseOutputAsync(
                            result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                            closeDesc,
                            token
                        )
                        .ConfigureAwait(false);
                }
                catch { }

                _webApiIntrinsics.EventLoop.QueueMacrotask(() =>
                {
                    ReadyState = WebSocketReadyState.Closed;
                    // wasClean=true because we completed the handshake.
                    FireCloseEvent(true, closeStatus, closeDesc);
                });

                return;
            }

            var messageBytes = messageBuffer.ToArray();
            var messageType = result.MessageType;
            var binaryType = _binaryType; // snapshot on I/O thread

            _webApiIntrinsics.EventLoop.QueueMacrotask(() =>
            {
                if (ReadyState != WebSocketReadyState.Open)
                {
                    return;
                }

                JsValue data;

                if (messageType == WebSocketMessageType.Text)
                {
                    data = SystemEncoding.UTF8.GetString(messageBytes);
                }
                else if (binaryType == "blob")
                {
                    data = _webApiIntrinsics.Blob.CreateInstance(
                        messageBytes,
                        "application/octet-stream"
                    );
                }
                else
                {
                    data = Engine.Intrinsics.ArrayBuffer.Construct(messageBytes);
                }

                var origin = GetOriginFromUrl(Url);
                DispatchEvent(
                    _webApiIntrinsics.MessageEvent.CreateInstance("message", data, origin)
                );
            });
        }
    }

    private async Task SendInternalAsync(byte[] bytes, WebSocketMessageType msgType)
    {
        try
        {
            await _client
                .SendAsync(
                    new ArraySegment<byte>(bytes),
                    msgType,
                    true,
                    _cancellationTokenSource.Token
                )
                .ConfigureAwait(false);
        }
        catch
        {
            // Errors during send are surfaced through the receive loop (connection drop).
        }
        finally
        {
            var len = (ulong)bytes.Length;
            BufferedAmount = BufferedAmount >= len ? BufferedAmount - len : 0;
        }
    }

    private async Task CloseInternalAsync(WebSocketCloseStatus status, string reason)
    {
        try
        {
            if (_client.State == WebSocketState.Open)
            {
                // Sends the Close frame; the server's echoed Close is handled in ReceiveLoopAsync.
                await _client
                    .CloseOutputAsync(status, reason, _cancellationTokenSource.Token)
                    .ConfigureAwait(false);
            }
        }
        catch
        {
            // If sending the close frame fails the receive loop will detect the disconnection.
        }
    }

    private void FireSimpleEvent(string type)
    {
        DispatchEvent(_webApiIntrinsics.Event.ConstructWithEventName(type, Undefined));
    }

    private void FireCloseEvent(bool wasClean, ushort code, string reason)
    {
        DispatchEvent(_webApiIntrinsics.CloseEvent.CreateInstance(wasClean, code, reason));
    }

    private static string GetOriginFromUrl(string url)
    {
        try
        {
            var uri = new Uri(url);
            return $"{uri.Scheme}://{uri.Authority}";
        }
        catch
        {
            return string.Empty;
        }
    }
}
