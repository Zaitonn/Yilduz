using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.URL;

public sealed class ToStringTests : TestBase
{
    [Theory]
    [InlineData("https://example.com/")]
    [InlineData("https://example.com/path")]
    [InlineData("https://example.com/path/to/resource")]
    [InlineData("https://example.com/path?query=value")]
    [InlineData("https://example.com/path?a=1&b=2")]
    [InlineData("https://example.com/path#section")]
    [InlineData("https://example.com/path?query=value#section")]
    [InlineData("http://localhost:3000/api/v1")]
    [InlineData("https://example.com:8080/path?q=1#frag")]
    [InlineData("ftp://ftp.example.com/pub/file.txt")]
    public void HrefShouldRoundTripForStandardUrls(string input)
    {
        Execute($"const url = new URL('{input}');");
        var href = Evaluate("url.href").AsString();

        Assert.Contains(Evaluate("url.protocol").AsString().TrimEnd(':'), href);
        Assert.Contains(Evaluate("url.hostname").AsString(), href);
    }

    [Fact]
    public void ToStringShouldEqualHref()
    {
        Execute("const url = new URL('https://example.com/path?q=1#frag');");

        var href = Evaluate("url.href").AsString();
        var toStringResult = Evaluate("url.toString()").AsString();

        Assert.Equal(href, toStringResult);
    }

    [Fact]
    public void ToStringShouldContainAllComponents()
    {
        Execute("const url = new URL('https://user:pass@example.com:8080/path?query=value#hash');");

        var result = Evaluate("url.toString()").AsString();

        Assert.StartsWith("https://", result);
        Assert.Contains("example.com:8080", result);
        Assert.Contains("/path", result);
        Assert.Contains("query=value", result);
        Assert.Contains("hash", result);
    }

    [Fact]
    public void ToStringShouldContainCredentialsWhenPresent()
    {
        Execute("const url = new URL('https://alice:secret@example.com/');");

        var result = Evaluate("url.toString()").AsString();

        Assert.Contains("alice", result);
        Assert.Contains("secret", result);
        Assert.Contains("@", result);
    }

    [Fact]
    public void ToStringShouldNotContainTrailingQuestionMarkForEmptySearch()
    {
        Execute("const url = new URL('https://example.com/path');");

        var result = Evaluate("url.toString()").AsString();
        var search = Evaluate("url.search").AsString();

        Assert.Empty(search);
        Assert.DoesNotContain("?", result);
    }

    [Fact]
    public void ToStringShouldNotContainTrailingHashForEmptyFragment()
    {
        Execute("const url = new URL('https://example.com/path');");

        var result = Evaluate("url.toString()").AsString();
        var hash = Evaluate("url.hash").AsString();

        Assert.Empty(hash);
        Assert.DoesNotContain("#", result);
    }

    [Fact]
    public void ToStringShouldReflectMutatedProperties()
    {
        Execute("const url = new URL('https://example.com/original');");
        Execute("url.pathname = '/updated';");
        Execute("url.search = '?newkey=newval';");
        Execute("url.hash = '#newhash';");

        var result = Evaluate("url.toString()").AsString();

        Assert.Contains("/updated", result);
        Assert.Contains("newkey=newval", result);
        Assert.Contains("newhash", result);
    }

    [Fact]
    public void ShouldParseMailtoUrl()
    {
        Execute("const url = new URL('mailto:user@example.com');");

        var protocol = Evaluate("url.protocol").AsString();

        Assert.Equal("mailto:", protocol);
    }

    [Fact]
    public void MailtoToStringShouldPreserveSchemeAndAddress()
    {
        Execute("const url = new URL('mailto:user@example.com');");

        var result = Evaluate("url.toString()").AsString();

        Assert.StartsWith("mailto:", result);
        Assert.Contains("user", result);
        Assert.Contains("example.com", result);
    }

    [Fact]
    public void MailtoCanParseShouldReturnTrue()
    {
        var result = Evaluate("URL.canParse('mailto:user@example.com')").AsBoolean();

        Assert.True(result);
    }

    [Fact]
    public void ShouldParseCustomSchemeUrl()
    {
        Execute("const url = new URL('weixin://scanqrcode');");

        Assert.Equal("weixin:", Evaluate("url.protocol"));
        Assert.Equal("scanqrcode", Evaluate("url.hostname"));
    }

    [Fact]
    public void CustomSchemeToStringShouldRoundTrip()
    {
        Execute("const url = new URL('weixin://scanqrcode');");

        var result = Evaluate("url.toString()").AsString();

        Assert.StartsWith("weixin://", result);
        Assert.Contains("scanqrcode", result);
    }

    [Fact]
    public void CustomSchemeWithPathShouldParse()
    {
        Execute("const url = new URL('myapp://open/screen?id=42#section');");

        var protocol = Evaluate("url.protocol").AsString();
        var hostname = Evaluate("url.hostname").AsString();
        var pathname = Evaluate("url.pathname").AsString();
        var search = Evaluate("url.search").AsString();
        var hash = Evaluate("url.hash").AsString();

        Assert.Equal("myapp:", protocol);
        Assert.Equal("open", hostname);
        Assert.Contains("screen", pathname);
        Assert.Contains("id=42", search);
        Assert.Contains("section", hash);
    }

    [Fact]
    public void CustomSchemeToStringShouldContainAllComponents()
    {
        Execute("const url = new URL('myapp://open/screen?id=42#section');");

        var result = Evaluate("url.toString()").AsString();

        Assert.StartsWith("myapp://", result);
        Assert.Contains("screen", result);
        Assert.Contains("id=42", result);
        Assert.Contains("section", result);
    }

    [Fact]
    public void CustomSchemeCanParseShouldReturnTrue()
    {
        var result = Evaluate("URL.canParse('weixin://scanqrcode')").AsBoolean();

        Assert.True(result);
    }

    [Theory]
    [InlineData("weixin://scanqrcode")]
    [InlineData("myapp://host/path")]
    [InlineData("intent://example.com/#Intent;scheme=http;end")]
    public void VariousCustomSchemesShouldBeAccepted(string input)
    {
        var canParse = Evaluate($"URL.canParse('{input}')").AsBoolean();

        Assert.True(canParse);
    }
}
