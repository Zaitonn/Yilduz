using System.Net;
using System.Net.Sockets;

namespace Yilduz.Tests;

internal static class NetworkTestHelper
{
    public static int GetAvailablePort()
    {
#if NETCOREAPP
        using var listener = new TcpListener(IPAddress.Loopback, 0);
#else
        var listener = new TcpListener(IPAddress.Loopback, 0);
#endif
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
