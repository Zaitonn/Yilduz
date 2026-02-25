using System;
using System.Net.Http;

namespace Yilduz;

public sealed partial class Options
{
    public NetworkOptions Network { get; init; } = new();

    public sealed class NetworkOptions
    {
        /// <summary>
        /// A factory that provides the <see cref="HttpClient"/> used by the
        /// <c>fetch()</c> global function.
        /// When null, a shared default <see cref="HttpClient"/> instance is used.
        /// </summary>
        public Func<HttpClient>? HttpClientFactory { get; init; }
    }
}
