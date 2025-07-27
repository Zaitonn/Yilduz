using Jint;
using Xunit;

namespace Yilduz.Tests.URL;

public sealed class JsonSerializationTests : TestBase
{
    [Fact]
    public void ShouldSerializeURLToJSON()
    {
        Engine.Execute("const url = new URL('https://example.com/path?query=value#hash');");

        var jsonResult = Engine.Evaluate("JSON.stringify(url)").AsString();

        Assert.Equal("\"https://example.com/path?query=value#hash\"", jsonResult);
    }

    [Fact]
    public void ShouldCallToJSONMethod()
    {
        Engine.Execute("const url = new URL('https://example.com/test');");

        var toJsonResult = Engine.Evaluate("url.toJSON()").AsString();
        var hrefResult = Engine.Evaluate("url.href").AsString();

        Assert.Equal(hrefResult, toJsonResult);
        Assert.Equal("https://example.com/test", toJsonResult);
    }

    [Fact]
    public void ShouldSerializeURLWithComplexPath()
    {
        Engine.Execute(
            """
            const url = new URL('https://user:pass@example.com:8080/path/to/resource?param1=value1&param2=value2#section');
            """
        );

        var jsonResult = Engine.Evaluate("JSON.stringify(url)").AsString();

        Assert.Equal(
            "\"https://user:pass@example.com:8080/path/to/resource?param1=value1&param2=value2#section\"",
            jsonResult
        );
    }

    [Fact]
    public void ShouldSerializeURLInArray()
    {
        Engine.Execute(
            """
            const urls = [
                new URL('https://example.com/first'),
                new URL('https://example.com/second')
            ];
            """
        );

        var jsonResult = Engine.Evaluate("JSON.stringify(urls)").AsString();

        Assert.Equal("[\"https://example.com/first\",\"https://example.com/second\"]", jsonResult);
    }

    [Fact]
    public void ShouldSerializeURLInObject()
    {
        Engine.Execute(
            """
            const obj = {
                homepage: new URL('https://example.com'),
                apiEndpoint: new URL('https://api.example.com/v1')
            };
            """
        );

        var jsonResult = Engine.Evaluate("JSON.stringify(obj)").AsString();

        Assert.Equal(
            "{\"homepage\":\"https://example.com/\",\"apiEndpoint\":\"https://api.example.com/v1\"}",
            jsonResult
        );
    }

    [Fact]
    public void ShouldHandleURLWithSpecialCharacters()
    {
        Engine.Execute(
            """
            const url = new URL('https://example.com/path?query=hello%20world&test=中文');
            """
        );

        var jsonResult = Engine.Evaluate("JSON.stringify(url)").AsString();
        var expectedUrl = Engine.Evaluate("url.href").AsString();

        Assert.Equal($"\"{expectedUrl}\"", jsonResult);
    }

    [Fact]
    public void ShouldSerializeModifiedURL()
    {
        Engine.Execute(
            """
            const url = new URL('https://example.com');
            url.pathname = '/new-path';
            url.search = '?modified=true';
            url.hash = '#new-hash';
            """
        );

        var jsonResult = Engine.Evaluate("JSON.stringify(url)").AsString();

        Assert.Equal("\"https://example.com/new-path?modified=true#new-hash\"", jsonResult);
    }
}
