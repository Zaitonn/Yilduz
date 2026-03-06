using System;
using System.Net.Http;
using System.Net.WebSockets;

namespace Yilduz;

public sealed partial class Options
{
    public NetworkOptions Network { get; } = new();

    public sealed class NetworkOptions
    {
        /// <summary>
        /// A factory that provides the <see cref="HttpClient"/> used by the
        /// <c>fetch()</c> global function.
        /// </summary>
        public Func<HttpClient>? HttpClientFactory { get; set; }

        /// <summary>
        /// A factory that provides the <see cref="ClientWebSocket"/> used by the
        /// <c>WebSocket</c> global class.
        /// </summary>
        public Func<ClientWebSocket>? WebSocketFactory { get; set; }
    }
}
