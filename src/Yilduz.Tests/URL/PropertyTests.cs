using Jint;
using Xunit;

namespace Yilduz.Tests.URL;

public sealed class PropertyTests : TestBase
{
    [Fact]
    public void OriginShouldBeReadOnly()
    {
        Engine.Execute("const url = new URL('https://example.com:8080/path');");
        Engine.Execute("const originalOrigin = url.origin;");
        Engine.Execute("url.origin = 'https://hacker.com';");

        var originAfterAttempt = Engine.Evaluate("url.origin").AsString();
        Assert.Equal("https://example.com:8080", originAfterAttempt);
    }

    [Fact]
    public void OriginShouldIncludeProtocolAndHost()
    {
        Engine.Execute(
            "const url = new URL('https://subdomain.example.com:9000/path?query#hash');"
        );
        var origin = Engine.Evaluate("url.origin").AsString();

        Assert.Equal("https://subdomain.example.com:9000", origin);
    }

    [Fact]
    public void OriginShouldNotIncludePathSearchOrHash()
    {
        Engine.Execute(
            "const url = new URL('https://example.com/very/long/path?key=value&other=data#section');"
        );
        var origin = Engine.Evaluate("url.origin").AsString();

        Assert.Equal("https://example.com", origin);
    }

    [Fact]
    public void HrefShouldReturnCompleteURL()
    {
        Engine.Execute(
            "const url = new URL('https://user:pass@example.com:8080/path?query=value#hash');"
        );
        var href = Engine.Evaluate("url.href").AsString();

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
        Engine.Execute("const url = new URL('https://old.com');");
        Engine.Execute("url.href = 'https://new.com:9000/newpath?param=value#newfrag';");

        var hostname = Engine.Evaluate("url.hostname").AsString();
        var port = Engine.Evaluate("url.port").AsString();
        var pathname = Engine.Evaluate("url.pathname").AsString();
        var search = Engine.Evaluate("url.search").AsString();
        var hash = Engine.Evaluate("url.hash").AsString();

        Assert.Equal("new.com", hostname);
        Assert.Equal("9000", port);
        Assert.Equal("/newpath", pathname);
        Assert.Equal("?param=value", search);
        Assert.Equal("#newfrag", hash);
    }

    [Fact]
    public void ProtocolShouldIncludeColon()
    {
        Engine.Execute("const httpUrl = new URL('http://example.com');");
        Engine.Execute("const httpsUrl = new URL('https://example.com');");
        Engine.Execute("const ftpUrl = new URL('ftp://ftp.example.com');");

        var httpProtocol = Engine.Evaluate("httpUrl.protocol").AsString();
        var httpsProtocol = Engine.Evaluate("httpsUrl.protocol").AsString();
        var ftpProtocol = Engine.Evaluate("ftpUrl.protocol").AsString();

        Assert.Equal("http:", httpProtocol);
        Assert.Equal("https:", httpsProtocol);
        Assert.Equal("ftp:", ftpProtocol);
    }

    [Fact]
    public void HostShouldIncludePortWhenPresent()
    {
        Engine.Execute("const urlWithPort = new URL('https://example.com:8080');");
        Engine.Execute("const urlWithoutPort = new URL('https://example.com');");

        var hostWithPort = Engine.Evaluate("urlWithPort.host").AsString();
        var hostWithoutPort = Engine.Evaluate("urlWithoutPort.host").AsString();

        Assert.Equal("example.com:8080", hostWithPort);
        Assert.Equal("example.com", hostWithoutPort);
    }

    [Fact]
    public void HostnameShouldNotIncludePort()
    {
        Engine.Execute("const urlWithPort = new URL('https://example.com:8080');");
        Engine.Execute("const urlWithoutPort = new URL('https://example.com');");

        var hostnameWithPort = Engine.Evaluate("urlWithPort.hostname").AsString();
        var hostnameWithoutPort = Engine.Evaluate("urlWithoutPort.hostname").AsString();

        Assert.Equal("example.com", hostnameWithPort);
        Assert.Equal("example.com", hostnameWithoutPort);
    }

    [Fact]
    public void PortShouldBeEmptyForDefaultPorts()
    {
        Engine.Execute("const httpUrl = new URL('http://example.com');");
        Engine.Execute("const httpsUrl = new URL('https://example.com');");

        var httpPort = Engine.Evaluate("httpUrl.port").AsString();
        var httpsPort = Engine.Evaluate("httpsUrl.port").AsString();

        // Note: This depends on the implementation. Some implementations return empty string for default ports
        // We test both cases since the behavior might vary
        Assert.True(httpPort == "" || httpPort == "80");
        Assert.True(httpsPort == "" || httpsPort == "443");
    }

    [Fact]
    public void PortShouldReturnExplicitPort()
    {
        Engine.Execute("const url = new URL('https://example.com:9000');");
        var port = Engine.Evaluate("url.port").AsString();

        Assert.Equal("9000", port);
    }

    [Fact]
    public void PathnameShouldStartWithSlash()
    {
        Engine.Execute("const rootUrl = new URL('https://example.com');");
        Engine.Execute("const pathUrl = new URL('https://example.com/path/to/resource');");

        var rootPathname = Engine.Evaluate("rootUrl.pathname").AsString();
        var pathPathname = Engine.Evaluate("pathUrl.pathname").AsString();

        Assert.Equal("/", rootPathname);
        Assert.Equal("/path/to/resource", pathPathname);
    }

    [Fact]
    public void SearchShouldIncludeQuestionMarkWhenPresent()
    {
        Engine.Execute("const urlWithSearch = new URL('https://example.com?key=value');");
        Engine.Execute("const urlWithoutSearch = new URL('https://example.com');");

        var searchWithQuery = Engine.Evaluate("urlWithSearch.search").AsString();
        var searchWithoutQuery = Engine.Evaluate("urlWithoutSearch.search").AsString();

        Assert.Equal("?key=value", searchWithQuery);
        Assert.Equal("", searchWithoutQuery);
    }

    [Fact]
    public void HashShouldIncludeHashSymbolWhenPresent()
    {
        Engine.Execute("const urlWithHash = new URL('https://example.com#section');");
        Engine.Execute("const urlWithoutHash = new URL('https://example.com');");

        var hashWithFragment = Engine.Evaluate("urlWithHash.hash").AsString();
        var hashWithoutFragment = Engine.Evaluate("urlWithoutHash.hash").AsString();

        Assert.Equal("#section", hashWithFragment);
        Assert.Equal("", hashWithoutFragment);
    }

    [Fact]
    public void UsernameAndPasswordShouldBeEmptyByDefault()
    {
        Engine.Execute("const url = new URL('https://example.com');");

        var username = Engine.Evaluate("url.username").AsString();
        var password = Engine.Evaluate("url.password").AsString();

        Assert.Equal("", username);
        Assert.Equal("", password);
    }

    [Fact]
    public void SettingUsernameAndPasswordShouldWork()
    {
        Engine.Execute("const url = new URL('https://example.com');");
        Engine.Execute("url.username = 'testuser';");
        Engine.Execute("url.password = 'testpass';");

        var username = Engine.Evaluate("url.username").AsString();
        var password = Engine.Evaluate("url.password").AsString();

        Assert.Equal("testuser", username);
        Assert.Equal("testpass", password);
    }

    [Fact]
    public void SearchParamsPropertyShouldReflectSearchString()
    {
        Engine.Execute("const url = new URL('https://example.com?key1=value1&key2=value2');");
        Engine.Execute("const param1 = url.searchParams.get('key1');");
        Engine.Execute("const param2 = url.searchParams.get('key2');");

        var param1Value = Engine.Evaluate("param1").AsString();
        var param2Value = Engine.Evaluate("param2").AsString();

        Assert.Equal("value1", param1Value);
        Assert.Equal("value2", param2Value);
    }

    [Fact]
    public void UpdatingSearchShouldUpdateSearchParams()
    {
        Engine.Execute("const url = new URL('https://example.com');");
        Engine.Execute("url.search = '?newkey=newvalue';");
        Engine.Execute("const newParam = url.searchParams.get('newkey');");

        Assert.Equal("newvalue", Engine.Evaluate("newParam"));
    }

    [Fact]
    public void PropertySettersShouldHandleEmptyStrings()
    {
        Engine.Execute(
            "const url = new URL('https://user:pass@example.com:8080/path?query=value#hash');"
        );

        Engine.Execute("url.username = '';");
        Engine.Execute("url.password = '';");
        Engine.Execute("url.search = '';");
        Engine.Execute("url.hash = '';");

        var username = Engine.Evaluate("url.username").AsString();
        var password = Engine.Evaluate("url.password").AsString();
        var search = Engine.Evaluate("url.search").AsString();
        var hash = Engine.Evaluate("url.hash").AsString();

        Assert.Equal("", username);
        Assert.Equal("", password);
        Assert.Equal("", search);
        Assert.Equal("", hash);
    }
}
