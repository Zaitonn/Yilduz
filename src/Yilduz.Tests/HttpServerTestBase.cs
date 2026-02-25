using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Yilduz.Tests;

public abstract class HttpServerTestBase : TestBase
{
    private readonly HttpListener _listener;
    private readonly CancellationTokenSource _serverCts;
    private readonly Task? _serverTask;

    protected string BaseUrl { get; }

    protected HttpServerTestBase()
    {
        // Pick a random available port.
        var port = GetAvailablePort();
        BaseUrl = $"http://localhost:{port}/";

        _listener = new HttpListener();
        _listener.Prefixes.Add(BaseUrl);
        _listener.Start();

        _serverCts = new CancellationTokenSource();
        _serverTask = Task.Run(() => RunServerAsync(_serverCts.Token));
    }

    protected override Options GetOptions()
    {
        return new Options { CancellationToken = Token };
    }

    /// <summary>
    /// Override this to handle incoming HTTP requests. The default implementation returns 404.
    /// </summary>
    protected virtual Task HandleRequestAsync(HttpListenerContext context)
    {
        context.Response.StatusCode = 404;
        context.Response.Close();
        return Task.CompletedTask;
    }

    private async Task RunServerAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var context = await _listener.GetContextAsync().ConfigureAwait(false);
                // Fire and forget – allows concurrent requests if needed.
                _ = Task.Run(
                    async () =>
                    {
                        try
                        {
                            await HandleRequestAsync(context).ConfigureAwait(false);
                        }
                        catch
                        {
                            try
                            {
                                context.Response.StatusCode = 500;
                                context.Response.Close();
                            }
                            catch
                            { /* listener may have been stopped */
                            }
                        }
                    },
                    ct
                );
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (HttpListenerException)
            {
                break;
            }
        }
    }

    protected override void OnDisposing()
    {
        _serverCts.Cancel();
        _listener.Stop();
        _listener.Close();
        _serverCts.Dispose();
    }

    private protected static async Task WriteResponseAsync(
        HttpListenerContext context,
        int statusCode,
        string body,
        string contentType = "text/plain"
    )
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = contentType;
        var bytes = System.Text.Encoding.UTF8.GetBytes(body);
        context.Response.ContentLength64 = bytes.Length;
        await context.Response.OutputStream.WriteAsync(bytes).ConfigureAwait(false);
        context.Response.Close();
    }

    private protected static async Task WriteResponseAsync(
        HttpListenerContext context,
        int statusCode,
        byte[] body,
        string contentType = "application/octet-stream"
    )
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = contentType;
        context.Response.ContentLength64 = body.Length;
        await context.Response.OutputStream.WriteAsync(body).ConfigureAwait(false);
        context.Response.Close();
    }

    private static int GetAvailablePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}
