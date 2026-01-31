using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.URL;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldCreateURLFromString()
    {
        Execute("const url = new URL('https://example.com/path?query=value#hash');");

        var href = Evaluate("url.href").AsString();
        var protocol = Evaluate("url.protocol").AsString();
        var hostname = Evaluate("url.hostname").AsString();
        var pathname = Evaluate("url.pathname").AsString();
        var search = Evaluate("url.search").AsString();
        var hash = Evaluate("url.hash").AsString();

        Assert.Equal("https://example.com/path?query=value#hash", href);
        Assert.Equal("https:", protocol);
        Assert.Equal("example.com", hostname);
        Assert.Equal("/path", pathname);
        Assert.Equal("?query=value", search);
        Assert.Equal("#hash", hash);
    }

    [Fact]
    public void ShouldCreateURLWithRelativeURLAndBase()
    {
        Execute("const url = new URL('/path?query=value', 'https://example.com');");

        var href = Evaluate("url.href").AsString();
        var protocol = Evaluate("url.protocol").AsString();
        var hostname = Evaluate("url.hostname").AsString();
        var pathname = Evaluate("url.pathname").AsString();

        Assert.Equal("https://example.com/path?query=value", href);
        Assert.Equal("https:", protocol);
        Assert.Equal("example.com", hostname);
        Assert.Equal("/path", pathname);
    }

    [Fact]
    public void ShouldThrowWhenCreatingWithRelativeURLWithoutBase()
    {
        Assert.Throws<JavaScriptException>(() => Execute("const url = new URL('/path');"));
    }

    [Fact]
    public void ShouldParseURLWithUsernameAndPassword()
    {
        Execute("const url = new URL('https://user:pass@example.com/path');");

        var username = Evaluate("url.username").AsString();
        var password = Evaluate("url.password").AsString();
        var host = Evaluate("url.host").AsString();
        var hostname = Evaluate("url.hostname").AsString();

        Assert.Equal("user", username);
        Assert.Equal("pass", password);
        Assert.Equal("example.com", host);
        Assert.Equal("example.com", hostname);
    }

    [Fact]
    public void ShouldParseURLWithOnlyUsername()
    {
        Execute("const url = new URL('https://user@example.com/path');");

        var username = Evaluate("url.username").AsString();
        var password = Evaluate("url.password").AsString();

        Assert.Equal("user", username);
        Assert.Equal("", password);
    }

    [Fact]
    public void ShouldParseURLWithPort()
    {
        Execute("const url = new URL('https://example.com:8080/path');");

        var port = Evaluate("url.port").AsString();
        var host = Evaluate("url.host").AsString();
        var hostname = Evaluate("url.hostname").AsString();

        Assert.Equal("8080", port);
        Assert.Equal("example.com:8080", host);
        Assert.Equal("example.com", hostname);
    }

    [Fact]
    public void ShouldHaveCorrectOriginProperty()
    {
        Execute("const url = new URL('https://example.com:8080/path');");
        var origin = Evaluate("url.origin").AsString();

        Assert.Equal("https://example.com:8080", origin);
    }

    [Fact]
    public void ShouldSetAndGetHrefProperty()
    {
        Execute("const url = new URL('https://example.com');");
        Execute("url.href = 'https://newdomain.com/newpath';");

        var href = Evaluate("url.href").AsString();
        Assert.Equal("https://newdomain.com/newpath", href);
    }

    [Fact]
    public void ShouldSetAndGetProtocolProperty()
    {
        Execute("const url = new URL('https://example.com');");
        Execute("url.protocol = 'http:';");

        var protocol = Evaluate("url.protocol").AsString();
        Assert.Equal("http:", protocol);
    }

    [Fact]
    public void ShouldSetAndGetHostProperty()
    {
        Execute("const url = new URL('https://example.com');");
        Execute("url.host = 'newhost.com:9000';");

        var host = Evaluate("url.host").AsString();
        Assert.Equal("newhost.com:9000", host);
    }

    [Fact]
    public void ShouldSetAndGetHostnameProperty()
    {
        Execute("const url = new URL('https://example.com:8080');");
        Execute("url.hostname = 'newhost.com';");

        var hostname = Evaluate("url.hostname").AsString();
        Assert.Equal("newhost.com", hostname);
    }

    [Fact]
    public void ShouldSetAndGetPortProperty()
    {
        Execute("const url = new URL('https://example.com');");
        Execute("url.port = '9000';");

        var port = Evaluate("url.port").AsString();
        Assert.Equal("9000", port);
    }

    [Fact]
    public void ShouldSetAndGetPathnameProperty()
    {
        Execute("const url = new URL('https://example.com/oldpath');");
        Execute("url.pathname = '/newpath/subpath';");

        var pathname = Evaluate("url.pathname").AsString();
        Assert.Equal("/newpath/subpath", pathname);
    }

    [Fact]
    public void ShouldSetAndGetSearchProperty()
    {
        Execute("const url = new URL('https://example.com');");
        Execute("url.search = '?key=value&other=data';");

        var search = Evaluate("url.search").AsString();
        Assert.Equal("?key=value&other=data", search);
    }

    [Fact]
    public void ShouldSetAndGetHashProperty()
    {
        Execute("const url = new URL('https://example.com');");
        Execute("url.hash = '#section1';");

        var hash = Evaluate("url.hash").AsString();
        Assert.Equal("#section1", hash);
    }

    [Fact]
    public void ShouldSetAndGetUsernameProperty()
    {
        Execute("const url = new URL('https://example.com');");
        Execute("url.username = 'newuser';");

        var username = Evaluate("url.username").AsString();
        Assert.Equal("newuser", username);
    }

    [Fact]
    public void ShouldSetAndGetPasswordProperty()
    {
        Execute("const url = new URL('https://example.com');");
        Execute("url.password = 'newpass';");

        var password = Evaluate("url.password").AsString();
        Assert.Equal("newpass", password);
    }

    [Fact]
    public void ShouldHaveCorrectPrototype()
    {
        Execute("const url = new URL('https://example.com');");
        var constructorName = Evaluate("url.constructor.name").AsString();

        Assert.Equal("URL", constructorName);
    }

    [Fact]
    public void ShouldHaveToStringMethod()
    {
        Execute("const url = new URL('https://example.com/path');");
        var toString = Evaluate("url.toString()").AsString();

        Assert.Equal("https://example.com/path", toString);
    }

    [Fact]
    public void ShouldHaveToJSONMethod()
    {
        Execute("const url = new URL('https://example.com/path');");
        var toJSON = Evaluate("url.toJSON()").AsString();

        Assert.Equal("https://example.com/path", toJSON);
    }

    [Fact]
    public void ShouldHaveSearchParamsProperty()
    {
        Execute("const url = new URL('https://example.com?foo=bar&baz=qux');");
        var searchParamsType = Evaluate("typeof url.searchParams");

        Assert.Equal("object", searchParamsType);
    }

    [Fact]
    public void SearchParamsShouldBeReadOnly()
    {
        Execute("const url = new URL('https://example.com?foo=bar');");
        Execute("const originalSearchParams = url.searchParams;");
        Execute("url.searchParams = 'should not work';");

        var searchParamsAfterAttempt = Engine
            .Evaluate("url.searchParams === originalSearchParams")
            .AsBoolean();
        Assert.True(searchParamsAfterAttempt);
    }

    [Fact]
    public void ShouldHandleMinimalURL()
    {
        Execute("const url = new URL('https://example.com');");

        var href = Evaluate("url.href").AsString();
        var protocol = Evaluate("url.protocol").AsString();
        var hostname = Evaluate("url.hostname").AsString();
        var pathname = Evaluate("url.pathname").AsString();
        var search = Evaluate("url.search").AsString();
        var hash = Evaluate("url.hash").AsString();

        Assert.Equal("https://example.com/", href);
        Assert.Equal("https:", protocol);
        Assert.Equal("example.com", hostname);
        Assert.Equal("/", pathname);
        Assert.Equal("", search);
        Assert.Equal("", hash);
    }

    [Fact]
    public void ShouldHandleComplexURL()
    {
        Execute(
            "const url = new URL('https://user:pass@subdomain.example.com:9000/deep/path/to/resource?param1=value1&param2=value2#fragment');"
        );

        var href = Evaluate("url.href").AsString();
        var origin = Evaluate("url.origin").AsString();
        var protocol = Evaluate("url.protocol").AsString();
        var username = Evaluate("url.username").AsString();
        var password = Evaluate("url.password").AsString();
        var host = Evaluate("url.host").AsString();
        var hostname = Evaluate("url.hostname").AsString();
        var port = Evaluate("url.port").AsString();
        var pathname = Evaluate("url.pathname").AsString();
        var search = Evaluate("url.search").AsString();
        var hash = Evaluate("url.hash").AsString();

        Assert.Contains("user:pass@subdomain.example.com:9000", href);
        Assert.Equal("https://subdomain.example.com:9000", origin);
        Assert.Equal("https:", protocol);
        Assert.Equal("user", username);
        Assert.Equal("pass", password);
        Assert.Equal("subdomain.example.com:9000", host);
        Assert.Equal("subdomain.example.com", hostname);
        Assert.Equal("9000", port);
        Assert.Equal("/deep/path/to/resource", pathname);
        Assert.Equal("?param1=value1&param2=value2", search);
        Assert.Equal("#fragment", hash);
    }

    [Fact]
    public void ShouldHandleHTTPURL()
    {
        Execute("const url = new URL('http://example.com/path');");

        var protocol = Evaluate("url.protocol").AsString();
        var origin = Evaluate("url.origin").AsString();

        Assert.Equal("http:", protocol);
        Assert.Equal("http://example.com", origin);
    }

    [Fact]
    public void ShouldHandleFTPURL()
    {
        Execute("const url = new URL('ftp://ftp.example.com/files/');");

        var protocol = Evaluate("url.protocol").AsString();
        var hostname = Evaluate("url.hostname").AsString();
        var pathname = Evaluate("url.pathname").AsString();

        Assert.Equal("ftp:", protocol);
        Assert.Equal("ftp.example.com", hostname);
        Assert.Equal("/files/", pathname);
    }

    [Fact]
    public void ShouldThrowForInvalidURL()
    {
        Assert.Throws<JavaScriptException>(() => Execute("const url = new URL('invalid-url');"));
    }

    [Fact]
    public void ShouldThrowForEmptyURL()
    {
        Assert.Throws<JavaScriptException>(() => Execute("const url = new URL('');"));
    }
}
