using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Xunit;

namespace Yilduz.Tests.Fetch;

public sealed class BaseUrlTests : TestBase
{
    protected override Options GetOptions()
    {
        return new()
        {
            CancellationToken = Token,
            BaseUrl = new("https://example.test/base/"),
            Network = new()
            {
                HttpClientFactory = () => new(CapturingHandler.Instance, disposeHandler: false),
            },
        };
    }

    [Fact]
    public void ShouldResolveRootRelativeUrlAgainstBaseUrl()
    {
        CapturingHandler.Reset();

        Execute(
            """
            let status = 0;
            async function run() {
                const res = await fetch('/api/ping');
                status = res.status;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.Equal(200, Evaluate("status").AsNumber());
        Assert.Equal("https://example.test/api/ping", CapturingHandler.LastRequestUri);
    }

    [Fact]
    public void ShouldResolvePathRelativeUrlAgainstBaseUrl()
    {
        CapturingHandler.Reset();

        Execute(
            """
            let status = 0;
            async function run() {
                const res = await fetch('api/ping');
                status = res.status;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.Equal(200, Evaluate("status").AsNumber());
        Assert.Equal("https://example.test/base/api/ping", CapturingHandler.LastRequestUri);
    }

    [Fact]
    public void ShouldIgnoreBaseUrlForAbsoluteUrl()
    {
        CapturingHandler.Reset();

        Execute(
            """
            let status = 0;
            async function run() {
                const res = await fetch('https://another.test/hello');
                status = res.status;
            }
            """
        );

        Evaluate("run()").UnwrapIfPromise();

        Assert.Equal(200, Evaluate("status").AsNumber());
        Assert.Equal("https://another.test/hello", CapturingHandler.LastRequestUri);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public static CapturingHandler Instance { get; } = new();

        public static string? LastRequestUri { get; private set; }

        public static void Reset()
        {
            LastRequestUri = null;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken
        )
        {
            LastRequestUri = request.RequestUri?.AbsoluteUri;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                RequestMessage = request,
                Content = new StringContent("ok"),
            };

            return Task.FromResult(response);
        }
    }
}
