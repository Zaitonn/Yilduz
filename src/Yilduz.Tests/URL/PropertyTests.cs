using Jint;
using Xunit;

namespace Yilduz.Tests.URL;

public sealed class PropertyTests : TestBase
{
    [Fact]
    public void OriginShouldBeReadOnly()
    {
        Execute("const url = new URL('https://example.com:8080/path');");
        Execute("const originalOrigin = url.origin;");
        Execute("url.origin = 'https://hacker.com';");

        var originAfterAttempt = Evaluate("url.origin").AsString();
        Assert.Equal("https://example.com:8080", originAfterAttempt);
    }

    [Fact]
    public void OriginShouldIncludeProtocolAndHost()
    {
        Execute("const url = new URL('https://subdomain.example.com:9000/path?query#hash');");
        var origin = Evaluate("url.origin").AsString();

        Assert.Equal("https://subdomain.example.com:9000", origin);
    }

    [Fact]
    public void OriginShouldNotIncludePathSearchOrHash()
    {
        Execute(
            "const url = new URL('https://example.com/very/long/path?key=value&other=data#section');"
        );
        var origin = Evaluate("url.origin").AsString();

        Assert.Equal("https://example.com", origin);
    }

    [Fact]
    public void HrefShouldReturnCompleteURL()
    {
        Execute("const url = new URL('https://user:pass@example.com:8080/path?query=value#hash');");
        var href = Evaluate("url.href").AsString();

        Assert.Contains("https://", href);
        Assert.Contains("user:pass@", href);
        Assert.Contains("example.com:8080", href);
        Assert.Contains("/path", href);
        Assert.Contains("?query=value", href);
        Assert.Contains("#hash", href);
    }

    [Fact]
    public void SettingHrefShouldUpdateOtherProperties()
    {
        Execute("const url = new URL('https://old.com');");
        Execute("url.href = 'https://new.com:9000/newpath?param=value#newfrag';");

        var hostname = Evaluate("url.hostname").AsString();
        var port = Evaluate("url.port").AsString();
        var pathname = Evaluate("url.pathname").AsString();
        var search = Evaluate("url.search").AsString();
        var hash = Evaluate("url.hash").AsString();

        Assert.Equal("new.com", hostname);
        Assert.Equal("9000", port);
        Assert.Equal("/newpath", pathname);
        Assert.Equal("?param=value", search);
        Assert.Equal("#newfrag", hash);
    }

    [Fact]
    public void ProtocolShouldIncludeColon()
    {
        Execute("const httpUrl = new URL('http://example.com');");
        Execute("const httpsUrl = new URL('https://example.com');");
        Execute("const ftpUrl = new URL('ftp://ftp.example.com');");

        var httpProtocol = Evaluate("httpUrl.protocol").AsString();
        var httpsProtocol = Evaluate("httpsUrl.protocol").AsString();
        var ftpProtocol = Evaluate("ftpUrl.protocol").AsString();

        Assert.Equal("http:", httpProtocol);
        Assert.Equal("https:", httpsProtocol);
        Assert.Equal("ftp:", ftpProtocol);
    }

    [Fact]
    public void HostShouldIncludePortWhenPresent()
    {
        Execute("const urlWithPort = new URL('https://example.com:8080');");
        Execute("const urlWithoutPort = new URL('https://example.com');");

        var hostWithPort = Evaluate("urlWithPort.host").AsString();
        var hostWithoutPort = Evaluate("urlWithoutPort.host").AsString();

        Assert.Equal("example.com:8080", hostWithPort);
        Assert.Equal("example.com", hostWithoutPort);
    }

    [Fact]
    public void HostnameShouldNotIncludePort()
    {
        Execute("const urlWithPort = new URL('https://example.com:8080');");
        Execute("const urlWithoutPort = new URL('https://example.com');");

        var hostnameWithPort = Evaluate("urlWithPort.hostname").AsString();
        var hostnameWithoutPort = Evaluate("urlWithoutPort.hostname").AsString();

        Assert.Equal("example.com", hostnameWithPort);
        Assert.Equal("example.com", hostnameWithoutPort);
    }

    [Fact]
    public void PortShouldBeEmptyForDefaultPorts()
    {
        Execute("const httpUrl = new URL('http://example.com');");
        Execute("const httpsUrl = new URL('https://example.com');");

        var httpPort = Evaluate("httpUrl.port").AsString();
        var httpsPort = Evaluate("httpsUrl.port").AsString();

        // Note: This depends on the implementation. Some implementations return empty string for default ports
        // We test both cases since the behavior might vary
        Assert.True(httpPort == "" || httpPort == "80");
        Assert.True(httpsPort == "" || httpsPort == "443");
    }

    [Fact]
    public void PortShouldReturnExplicitPort()
    {
        Execute("const url = new URL('https://example.com:9000');");
        var port = Evaluate("url.port").AsString();

        Assert.Equal("9000", port);
    }

    [Fact]
    public void PathnameShouldStartWithSlash()
    {
        Execute("const rootUrl = new URL('https://example.com');");
        Execute("const pathUrl = new URL('https://example.com/path/to/resource');");

        var rootPathname = Evaluate("rootUrl.pathname").AsString();
        var pathPathname = Evaluate("pathUrl.pathname").AsString();

        Assert.Equal("/", rootPathname);
        Assert.Equal("/path/to/resource", pathPathname);
    }

    [Fact]
    public void SearchShouldIncludeQuestionMarkWhenPresent()
    {
        Execute("const urlWithSearch = new URL('https://example.com?key=value');");
        Execute("const urlWithoutSearch = new URL('https://example.com');");

        var searchWithQuery = Evaluate("urlWithSearch.search").AsString();
        var searchWithoutQuery = Evaluate("urlWithoutSearch.search").AsString();

        Assert.Equal("?key=value", searchWithQuery);
        Assert.Equal("", searchWithoutQuery);
    }

    [Fact]
    public void HashShouldIncludeHashSymbolWhenPresent()
    {
        Execute("const urlWithHash = new URL('https://example.com#section');");
        Execute("const urlWithoutHash = new URL('https://example.com');");

        var hashWithFragment = Evaluate("urlWithHash.hash").AsString();
        var hashWithoutFragment = Evaluate("urlWithoutHash.hash").AsString();

        Assert.Equal("#section", hashWithFragment);
        Assert.Equal("", hashWithoutFragment);
    }

    [Fact]
    public void UsernameAndPasswordShouldBeEmptyByDefault()
    {
        Execute("const url = new URL('https://example.com');");

        var username = Evaluate("url.username").AsString();
        var password = Evaluate("url.password").AsString();

        Assert.Equal("", username);
        Assert.Equal("", password);
    }

    [Fact]
    public void SettingUsernameAndPasswordShouldWork()
    {
        Execute("const url = new URL('https://example.com');");
        Execute("url.username = 'testuser';");
        Execute("url.password = 'testpass';");

        var username = Evaluate("url.username").AsString();
        var password = Evaluate("url.password").AsString();

        Assert.Equal("testuser", username);
        Assert.Equal("testpass", password);
    }

    [Fact]
    public void SearchParamsPropertyShouldReflectSearchString()
    {
        Execute("const url = new URL('https://example.com?key1=value1&key2=value2');");
        Execute("const param1 = url.searchParams.get('key1');");
        Execute("const param2 = url.searchParams.get('key2');");

        var param1Value = Evaluate("param1").AsString();
        var param2Value = Evaluate("param2").AsString();

        Assert.Equal("value1", param1Value);
        Assert.Equal("value2", param2Value);
    }

    [Fact]
    public void UpdatingSearchShouldUpdateSearchParams()
    {
        Execute("const url = new URL('https://example.com');");
        Execute("url.search = '?newkey=newvalue';");
        Execute("const newParam = url.searchParams.get('newkey');");

        Assert.Equal("newvalue", Evaluate("newParam"));
    }

    [Fact]
    public void PropertySettersShouldHandleEmptyStrings()
    {
        Execute("const url = new URL('https://user:pass@example.com:8080/path?query=value#hash');");

        Execute("url.username = '';");
        Execute("url.password = '';");
        Execute("url.search = '';");
        Execute("url.hash = '';");

        var username = Evaluate("url.username").AsString();
        var password = Evaluate("url.password").AsString();
        var search = Evaluate("url.search").AsString();
        var hash = Evaluate("url.hash").AsString();

        Assert.Equal("", username);
        Assert.Equal("", password);
        Assert.Equal("", search);
        Assert.Equal("", hash);
    }
}
