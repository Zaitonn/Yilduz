using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.URL;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldCreateURLFromString()
    {
        Engine.Execute("const url = new URL('https://example.com/path?query=value#hash');");

        var href = Engine.Evaluate("url.href").AsString();
        var protocol = Engine.Evaluate("url.protocol").AsString();
        var hostname = Engine.Evaluate("url.hostname").AsString();
        var pathname = Engine.Evaluate("url.pathname").AsString();
        var search = Engine.Evaluate("url.search").AsString();
        var hash = Engine.Evaluate("url.hash").AsString();

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
        Engine.Execute("const url = new URL('/path?query=value', 'https://example.com');");

        var href = Engine.Evaluate("url.href").AsString();
        var protocol = Engine.Evaluate("url.protocol").AsString();
        var hostname = Engine.Evaluate("url.hostname").AsString();
        var pathname = Engine.Evaluate("url.pathname").AsString();

        Assert.Equal("https://example.com/path?query=value", href);
        Assert.Equal("https:", protocol);
        Assert.Equal("example.com", hostname);
        Assert.Equal("/path", pathname);
    }

    [Fact]
    public void ShouldThrowWhenCreatingWithRelativeURLWithoutBase()
    {
        Assert.Throws<JavaScriptException>(() => Engine.Execute("const url = new URL('/path');"));
    }

    [Fact]
    public void ShouldParseURLWithUsernameAndPassword()
    {
        Engine.Execute("const url = new URL('https://user:pass@example.com/path');");

        var username = Engine.Evaluate("url.username").AsString();
        var password = Engine.Evaluate("url.password").AsString();
        var host = Engine.Evaluate("url.host").AsString();
        var hostname = Engine.Evaluate("url.hostname").AsString();

        Assert.Equal("user", username);
        Assert.Equal("pass", password);
        Assert.Equal("example.com", host);
        Assert.Equal("example.com", hostname);
    }

    [Fact]
    public void ShouldParseURLWithOnlyUsername()
    {
        Engine.Execute("const url = new URL('https://user@example.com/path');");

        var username = Engine.Evaluate("url.username").AsString();
        var password = Engine.Evaluate("url.password").AsString();

        Assert.Equal("user", username);
        Assert.Equal("", password);
    }

    [Fact]
    public void ShouldParseURLWithPort()
    {
        Engine.Execute("const url = new URL('https://example.com:8080/path');");

        var port = Engine.Evaluate("url.port").AsString();
        var host = Engine.Evaluate("url.host").AsString();
        var hostname = Engine.Evaluate("url.hostname").AsString();

        Assert.Equal("8080", port);
        Assert.Equal("example.com:8080", host);
        Assert.Equal("example.com", hostname);
    }

    [Fact]
    public void ShouldHaveCorrectOriginProperty()
    {
        Engine.Execute("const url = new URL('https://example.com:8080/path');");
        var origin = Engine.Evaluate("url.origin").AsString();

        Assert.Equal("https://example.com:8080", origin);
    }

    [Fact]
    public void ShouldSetAndGetHrefProperty()
    {
        Engine.Execute("const url = new URL('https://example.com');");
        Engine.Execute("url.href = 'https://newdomain.com/newpath';");

        var href = Engine.Evaluate("url.href").AsString();
        Assert.Equal("https://newdomain.com/newpath", href);
    }

    [Fact]
    public void ShouldSetAndGetProtocolProperty()
    {
        Engine.Execute("const url = new URL('https://example.com');");
        Engine.Execute("url.protocol = 'http:';");

        var protocol = Engine.Evaluate("url.protocol").AsString();
        Assert.Equal("http:", protocol);
    }

    [Fact]
    public void ShouldSetAndGetHostProperty()
    {
        Engine.Execute("const url = new URL('https://example.com');");
        Engine.Execute("url.host = 'newhost.com:9000';");

        var host = Engine.Evaluate("url.host").AsString();
        Assert.Equal("newhost.com:9000", host);
    }

    [Fact]
    public void ShouldSetAndGetHostnameProperty()
    {
        Engine.Execute("const url = new URL('https://example.com:8080');");
        Engine.Execute("url.hostname = 'newhost.com';");

        var hostname = Engine.Evaluate("url.hostname").AsString();
        Assert.Equal("newhost.com", hostname);
    }

    [Fact]
    public void ShouldSetAndGetPortProperty()
    {
        Engine.Execute("const url = new URL('https://example.com');");
        Engine.Execute("url.port = '9000';");

        var port = Engine.Evaluate("url.port").AsString();
        Assert.Equal("9000", port);
    }

    [Fact]
    public void ShouldSetAndGetPathnameProperty()
    {
        Engine.Execute("const url = new URL('https://example.com/oldpath');");
        Engine.Execute("url.pathname = '/newpath/subpath';");

        var pathname = Engine.Evaluate("url.pathname").AsString();
        Assert.Equal("/newpath/subpath", pathname);
    }

    [Fact]
    public void ShouldSetAndGetSearchProperty()
    {
        Engine.Execute("const url = new URL('https://example.com');");
        Engine.Execute("url.search = '?key=value&other=data';");

        var search = Engine.Evaluate("url.search").AsString();
        Assert.Equal("?key=value&other=data", search);
    }

    [Fact]
    public void ShouldSetAndGetHashProperty()
    {
        Engine.Execute("const url = new URL('https://example.com');");
        Engine.Execute("url.hash = '#section1';");

        var hash = Engine.Evaluate("url.hash").AsString();
        Assert.Equal("#section1", hash);
    }

    [Fact]
    public void ShouldSetAndGetUsernameProperty()
    {
        Engine.Execute("const url = new URL('https://example.com');");
        Engine.Execute("url.username = 'newuser';");

        var username = Engine.Evaluate("url.username").AsString();
        Assert.Equal("newuser", username);
    }

    [Fact]
    public void ShouldSetAndGetPasswordProperty()
    {
        Engine.Execute("const url = new URL('https://example.com');");
        Engine.Execute("url.password = 'newpass';");

        var password = Engine.Evaluate("url.password").AsString();
        Assert.Equal("newpass", password);
    }

    [Fact]
    public void ShouldHaveCorrectPrototype()
    {
        Engine.Execute("const url = new URL('https://example.com');");
        var constructorName = Engine.Evaluate("url.constructor.name").AsString();

        Assert.Equal("URL", constructorName);
    }

    [Fact]
    public void ShouldHaveToStringMethod()
    {
        Engine.Execute("const url = new URL('https://example.com/path');");
        var toString = Engine.Evaluate("url.toString()").AsString();

        Assert.Equal("https://example.com/path", toString);
    }

    [Fact]
    public void ShouldHaveToJSONMethod()
    {
        Engine.Execute("const url = new URL('https://example.com/path');");
        var toJSON = Engine.Evaluate("url.toJSON()").AsString();

        Assert.Equal("https://example.com/path", toJSON);
    }

    [Fact]
    public void ShouldHaveSearchParamsProperty()
    {
        Engine.Execute("const url = new URL('https://example.com?foo=bar&baz=qux');");
        var searchParamsType = Engine.Evaluate("typeof url.searchParams");

        Assert.Equal("object", searchParamsType);
    }

    [Fact]
    public void SearchParamsShouldBeReadOnly()
    {
        Engine.Execute("const url = new URL('https://example.com?foo=bar');");
        Engine.Execute("const originalSearchParams = url.searchParams;");
        Engine.Execute("url.searchParams = 'should not work';");

        var searchParamsAfterAttempt = Engine
            .Evaluate("url.searchParams === originalSearchParams")
            .AsBoolean();
        Assert.True(searchParamsAfterAttempt);
    }

    [Fact]
    public void ShouldHandleMinimalURL()
    {
        Engine.Execute("const url = new URL('https://example.com');");

        var href = Engine.Evaluate("url.href").AsString();
        var protocol = Engine.Evaluate("url.protocol").AsString();
        var hostname = Engine.Evaluate("url.hostname").AsString();
        var pathname = Engine.Evaluate("url.pathname").AsString();
        var search = Engine.Evaluate("url.search").AsString();
        var hash = Engine.Evaluate("url.hash").AsString();

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
        Engine.Execute(
            "const url = new URL('https://user:pass@subdomain.example.com:9000/deep/path/to/resource?param1=value1&param2=value2#fragment');"
        );

        var href = Engine.Evaluate("url.href").AsString();
        var origin = Engine.Evaluate("url.origin").AsString();
        var protocol = Engine.Evaluate("url.protocol").AsString();
        var username = Engine.Evaluate("url.username").AsString();
        var password = Engine.Evaluate("url.password").AsString();
        var host = Engine.Evaluate("url.host").AsString();
        var hostname = Engine.Evaluate("url.hostname").AsString();
        var port = Engine.Evaluate("url.port").AsString();
        var pathname = Engine.Evaluate("url.pathname").AsString();
        var search = Engine.Evaluate("url.search").AsString();
        var hash = Engine.Evaluate("url.hash").AsString();

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
        Engine.Execute("const url = new URL('http://example.com/path');");

        var protocol = Engine.Evaluate("url.protocol").AsString();
        var origin = Engine.Evaluate("url.origin").AsString();

        Assert.Equal("http:", protocol);
        Assert.Equal("http://example.com", origin);
    }

    [Fact]
    public void ShouldHandleFTPURL()
    {
        Engine.Execute("const url = new URL('ftp://ftp.example.com/files/');");

        var protocol = Engine.Evaluate("url.protocol").AsString();
        var hostname = Engine.Evaluate("url.hostname").AsString();
        var pathname = Engine.Evaluate("url.pathname").AsString();

        Assert.Equal("ftp:", protocol);
        Assert.Equal("ftp.example.com", hostname);
        Assert.Equal("/files/", pathname);
    }

    [Fact]
    public void ShouldThrowForInvalidURL()
    {
        Assert.Throws<JavaScriptException>(
            () => Engine.Execute("const url = new URL('invalid-url');")
        );
    }

    [Fact]
    public void ShouldThrowForEmptyURL()
    {
        Assert.Throws<JavaScriptException>(() => Engine.Execute("const url = new URL('');"));
    }
}
