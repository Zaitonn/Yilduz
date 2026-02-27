using Jint;
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
    [InlineData("blob:https://example.com:8080/resource-id")]
    public void HrefShouldRoundTripForStandardUrls(string input)
    {
        Execute($"const url = new URL('{input}');");
        var href = Evaluate("url.href").AsString();

        Assert.Contains(Evaluate("url.protocol").AsString().TrimEnd(':'), href);
        Assert.Contains(Evaluate("url.hostname").AsString(), href);
        Assert.Contains(Evaluate("url.pathname").AsString(), href);
        Assert.Equal(input, href);
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
}
