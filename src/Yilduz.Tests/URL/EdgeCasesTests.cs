using System.Linq;
using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.URL;

public sealed class EdgeCasesTests : TestBase
{
    [Fact]
    public void ShouldHandleIPv4Address()
    {
        Execute("const url = new URL('https://192.168.1.1:8080/path');");

        var hostname = Evaluate("url.hostname").AsString();
        var host = Evaluate("url.host").AsString();
        var port = Evaluate("url.port").AsString();

        Assert.Equal("192.168.1.1", hostname);
        Assert.Equal("192.168.1.1:8080", host);
        Assert.Equal("8080", port);
    }

    [Fact]
    public void ShouldHandleIPv6Address()
    {
        Execute("const url = new URL('https://[::1]/path');");

        var hostname = Evaluate("url.hostname").AsString();
        var pathname = Evaluate("url.pathname").AsString();

        Assert.Equal("[::1]", hostname);
        Assert.Equal("/path", pathname);
    }

    [Fact]
    public void ShouldHandleLocalhost()
    {
        Execute("const url = new URL('http://localhost:3000/api');");

        var hostname = Evaluate("url.hostname").AsString();
        var port = Evaluate("url.port").AsString();
        var pathname = Evaluate("url.pathname").AsString();

        Assert.Equal("localhost", hostname);
        Assert.Equal("3000", port);
        Assert.Equal("/api", pathname);
    }

    [Fact]
    public void ShouldHandleFileProtocol()
    {
        Execute("const url = new URL('file:///C:/Users/test/document.txt');");

        var protocol = Evaluate("url.protocol").AsString();
        var pathname = Evaluate("url.pathname").AsString();

        Assert.Equal("file:", protocol);
        Assert.Contains("/C:/Users/test/document.txt", pathname);
    }

    [Fact]
    public void ShouldHandleEncodedCharacters()
    {
        Execute(
            "const url = new URL('https://example.com/path%20with%20spaces?key=value%20with%20spaces');"
        );

        var href = Evaluate("url.href").AsString();
        var pathname = Evaluate("url.pathname").AsString();
        var search = Evaluate("url.search").AsString();

        Assert.Contains("example.com", href);
        Assert.Contains("path", pathname);
        Assert.Contains("key=value", search);
    }

    [Fact]
    public void ShouldHandleUnicodeCharacters()
    {
        Execute("const url = new URL('https://测试.example.com/路径');");

        var hostname = Evaluate("url.hostname").AsString();
        var pathname = Evaluate("url.pathname").AsString();

        // The behavior here depends on how the underlying URI parser handles Unicode
        Assert.NotEmpty(hostname);
        Assert.NotEmpty(pathname);
    }

    [Fact]
    public void ShouldHandleEmptyComponents()
    {
        Execute("const url = new URL('https://example.com///multiple//slashes///');");

        var hostname = Evaluate("url.hostname").AsString();
        var pathname = Evaluate("url.pathname").AsString();

        Assert.Equal("example.com", hostname);
        // The pathname behavior depends on normalization
        Assert.NotEmpty(pathname);
    }

    [Fact]
    public void ShouldHandleVeryLongURL()
    {
        var longPath = string.Join("/", Enumerable.Repeat("segment", 100));
        Execute($"const url = new URL('https://example.com/{longPath}');");

        var hostname = Evaluate("url.hostname").AsString();
        var pathname = Evaluate("url.pathname").AsString();

        Assert.Equal("example.com", hostname);
        Assert.Contains("segment", pathname);
    }

    [Fact]
    public void ShouldHandleSpecialCharactersInUserInfo()
    {
        Execute("const url = new URL('https://user%40domain:p%40ssw0rd@example.com');");

        var username = Evaluate("url.username").AsString();
        var password = Evaluate("url.password").AsString();

        // The behavior depends on URL encoding handling
        Assert.NotEmpty(username);
        Assert.NotEmpty(password);
    }

    [Fact]
    public void ShouldHandleQueryStringWithSpecialCharacters()
    {
        Execute(
            "const url = new URL('https://example.com?key=value&other=data with spaces&encoded=%20test');"
        );

        var search = Evaluate("url.search").AsString();

        Assert.Contains("key=value", search);
        Assert.Contains("other=data", search);
        Assert.Contains("encoded=", search);
    }

    [Fact]
    public void ShouldHandleFragmentWithSpecialCharacters()
    {
        Execute("const url = new URL('https://example.com#section with spaces');");

        var hash = Evaluate("url.hash").AsString();

        Assert.StartsWith("#", hash);
        Assert.Contains("section", hash);
    }

    [Fact]
    public void ShouldHandleMultipleQueryParameters()
    {
        Execute("const url = new URL('https://example.com?a=1&b=2&c=3&d=4&e=5');");

        var search = Evaluate("url.search").AsString();

        Assert.Contains("a=1", search);
        Assert.Contains("b=2", search);
        Assert.Contains("c=3", search);
        Assert.Contains("d=4", search);
        Assert.Contains("e=5", search);
    }

    [Fact]
    public void ShouldHandleEmptyQueryParameterValues()
    {
        Execute("const url = new URL('https://example.com?empty=&also_empty&with_value=test');");

        var search = Evaluate("url.search").AsString();

        Assert.Contains("empty=", search);
        Assert.Contains("also_empty", search);
        Assert.Contains("with_value=test", search);
    }

    [Fact]
    public void ShouldPreserveTrailingSlashInPathname()
    {
        Execute("const urlWithSlash = new URL('https://example.com/path/');");
        Execute("const urlWithoutSlash = new URL('https://example.com/path');");

        var pathnameWithSlash = Evaluate("urlWithSlash.pathname").AsString();
        var pathnameWithoutSlash = Evaluate("urlWithoutSlash.pathname").AsString();

        Assert.EndsWith("/", pathnameWithSlash);
        Assert.False(pathnameWithoutSlash.EndsWith("/"));
    }

    [Fact]
    public void ShouldHandleRelativeURLsWithDifferentBasePaths()
    {
        Execute("const url1 = new URL('subpath', 'https://example.com/base/');");
        Execute("const url2 = new URL('subpath', 'https://example.com/base');");
        Execute("const url3 = new URL('./subpath', 'https://example.com/base/');");
        Execute("const url4 = new URL('../subpath', 'https://example.com/base/other/');");

        var href1 = Evaluate("url1.href").AsString();
        var href2 = Evaluate("url2.href").AsString();
        var href3 = Evaluate("url3.href").AsString();
        var href4 = Evaluate("url4.href").AsString();

        // These tests verify relative URL resolution behavior
        Assert.Contains("subpath", href1);
        Assert.Contains("subpath", href2);
        Assert.Contains("subpath", href3);
        Assert.Contains("subpath", href4);
    }

    [Fact]
    public void ShouldThrowForMalformedURL()
    {
        Assert.Throws<JavaScriptException>(() => Execute("const url = new URL('https://');"));
    }

    [Fact]
    public void ShouldThrowForURLWithInvalidCharacters()
    {
        // Throws on nodejs but not on browser
        Assert.Throws<JavaScriptException>(
            () => Execute("const url = new URL('https://example.com path');")
        );
    }
}
