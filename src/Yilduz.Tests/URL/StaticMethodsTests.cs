using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.URL;

public sealed class StaticMethodsTests : TestBase
{
    [Fact]
    public void CanParseShouldReturnTrueForValidURL()
    {
        var result = Engine.Evaluate("URL.canParse('https://example.com')").AsBoolean();
        Assert.True(result);
    }

    [Fact]
    public void CanParseShouldReturnTrueForValidRelativeURLWithBase()
    {
        var result = Engine.Evaluate("URL.canParse('/path', 'https://example.com')").AsBoolean();
        Assert.True(result);
    }

    [Fact]
    public void CanParseShouldReturnFalseForInvalidURL()
    {
        var result = Engine.Evaluate("URL.canParse('invalid-url')").AsBoolean();
        Assert.False(result);
    }

    [Fact]
    public void CanParseShouldReturnFalseForRelativeURLWithoutBase()
    {
        var result = Engine.Evaluate("URL.canParse('/path')").AsBoolean();
        Assert.False(result);
    }

    [Fact]
    public void CanParseShouldReturnFalseForEmptyURL()
    {
        var result = Engine.Evaluate("URL.canParse('')").AsBoolean();
        Assert.False(result);
    }

    [Fact]
    public void ParseShouldReturnURLInstanceForValidURL()
    {
        Engine.Execute("const parsed = URL.parse('https://example.com/path');");

        var href = Engine.Evaluate("parsed.href").AsString();
        var hostname = Engine.Evaluate("parsed.hostname").AsString();
        var pathname = Engine.Evaluate("parsed.pathname").AsString();

        Assert.Equal("https://example.com/path", href);
        Assert.Equal("example.com", hostname);
        Assert.Equal("/path", pathname);
    }

    [Fact]
    public void ParseShouldReturnURLInstanceForValidRelativeURLWithBase()
    {
        Engine.Execute("const parsed = URL.parse('/path', 'https://example.com');");

        var href = Engine.Evaluate("parsed.href").AsString();
        var hostname = Engine.Evaluate("parsed.hostname").AsString();
        var pathname = Engine.Evaluate("parsed.pathname").AsString();

        Assert.Equal("https://example.com/path", href);
        Assert.Equal("example.com", hostname);
        Assert.Equal("/path", pathname);
    }

    [Fact]
    public void ParseShouldReturnNullForInvalidURL()
    {
        Engine.Execute("const parsed = URL.parse('invalid-url');");
        var result = Engine.Evaluate("parsed");

        Assert.True(result.IsNull());
    }

    [Fact]
    public void ParseShouldReturnNullForRelativeURLWithoutBase()
    {
        Engine.Execute("const parsed = URL.parse('/path');");
        var result = Engine.Evaluate("parsed");

        Assert.True(result.IsNull());
    }

    [Fact]
    public void ParseShouldReturnNullForEmptyURL()
    {
        Engine.Execute("const parsed = URL.parse('');");
        var result = Engine.Evaluate("parsed");

        Assert.True(result.IsNull());
    }

    [Fact]
    public void CanParseShouldHandleComplexURLs()
    {
        var result = Engine
            .Evaluate(
                "URL.canParse('https://user:pass@subdomain.example.com:8080/path?query=value#hash')"
            )
            .AsBoolean();

        Assert.True(result);
    }

    [Fact]
    public void ParseShouldHandleComplexURLs()
    {
        Engine.Execute(
            "const parsed = URL.parse('https://user:pass@subdomain.example.com:8080/path?query=value#hash');"
        );

        var username = Engine.Evaluate("parsed.username").AsString();
        var password = Engine.Evaluate("parsed.password").AsString();
        var hostname = Engine.Evaluate("parsed.hostname").AsString();
        var port = Engine.Evaluate("parsed.port").AsString();
        var search = Engine.Evaluate("parsed.search").AsString();
        var hash = Engine.Evaluate("parsed.hash").AsString();

        Assert.Equal("user", username);
        Assert.Equal("pass", password);
        Assert.Equal("subdomain.example.com", hostname);
        Assert.Equal("8080", port);
        Assert.Equal("?query=value", search);
        Assert.Equal("#hash", hash);
    }

    [Fact]
    public void CanParseShouldHandleDifferentProtocols()
    {
        Assert.True(Engine.Evaluate("URL.canParse('http://example.com')").AsBoolean());
        Assert.True(Engine.Evaluate("URL.canParse('https://example.com')").AsBoolean());
        Assert.True(Engine.Evaluate("URL.canParse('ftp://ftp.example.com')").AsBoolean());
        Assert.True(Engine.Evaluate("URL.canParse('file:///path/to/file')").AsBoolean());
    }

    [Fact]
    public void ParseShouldHandleDifferentProtocols()
    {
        Engine.Execute("const httpUrl = URL.parse('http://example.com');");
        Engine.Execute("const ftpUrl = URL.parse('ftp://ftp.example.com');");

        var httpProtocol = Engine.Evaluate("httpUrl.protocol").AsString();
        var ftpProtocol = Engine.Evaluate("ftpUrl.protocol").AsString();

        Assert.Equal("http:", httpProtocol);
        Assert.Equal("ftp:", ftpProtocol);
    }

    [Fact]
    public void CanParseShouldRequireAtLeastOneArgument()
    {
        Assert.Throws<JavaScriptException>(() => Engine.Execute("URL.canParse();"));
    }

    [Fact]
    public void ParseShouldRequireAtLeastOneArgument()
    {
        Assert.Throws<JavaScriptException>(() => Engine.Execute("URL.parse();"));
    }

    [Fact]
    public void ParseShouldReturnNullForMalformedURL()
    {
        Engine.Execute("const parsed = URL.parse('https://');");
        var result = Engine.Evaluate("parsed");

        Assert.True(result.IsNull());
    }

    [Fact]
    public void ParseShouldReturnNullForInvalidProtocol()
    {
        Engine.Execute("const parsed = URL.parse('🥺://example.com');");
        var result = Engine.Evaluate("parsed");

        Assert.True(result.IsNull());
    }

    [Fact]
    public void ParseShouldReturnValidURLObjectForValidInput()
    {
        Engine.Execute("const parsed = URL.parse('https://example.com/path');");
        Engine.Execute("const isNull = parsed === null;");
        Engine.Execute("const isURL = parsed instanceof URL;");

        var isNull = Engine.Evaluate("isNull").AsBoolean();
        var isURL = Engine.Evaluate("isURL").AsBoolean();

        Assert.False(isNull);
        Assert.True(isURL);
    }

    [Fact]
    public void ParseShouldWorkAsAlternativeToTryCatchWithConstructor()
    {
        Engine.Execute(
            """
            function safeParseWithConstructor(url, base) {
                try {
                    return new URL(url, base);
                } catch {
                    return null;
                }
            }

            const url1 = 'https://example.com/path';
            const url2 = 'invalid-url';
            const url3 = '/path';
            const base = 'https://example.com';

            const parseResult1 = URL.parse(url1);
            const constructorResult1 = safeParseWithConstructor(url1);

            const parseResult2 = URL.parse(url2);
            const constructorResult2 = safeParseWithConstructor(url2);

            const parseResult3 = URL.parse(url3, base);
            const constructorResult3 = safeParseWithConstructor(url3, base);

            const parseResult4 = URL.parse(url3); // without base
            const constructorResult4 = safeParseWithConstructor(url3);
            """
        );

        // Valid URL should return URL objects in both cases
        var parseResult1IsNull = Engine.Evaluate("parseResult1 === null").AsBoolean();
        var constructorResult1IsNull = Engine.Evaluate("constructorResult1 === null").AsBoolean();

        // Invalid URL should return null in both cases
        var parseResult2IsNull = Engine.Evaluate("parseResult2 === null").AsBoolean();
        var constructorResult2IsNull = Engine.Evaluate("constructorResult2 === null").AsBoolean();

        // Relative URL with base should return URL objects in both cases
        var parseResult3IsNull = Engine.Evaluate("parseResult3 === null").AsBoolean();
        var constructorResult3IsNull = Engine.Evaluate("constructorResult3 === null").AsBoolean();

        // Relative URL without base should return null in both cases
        var parseResult4IsNull = Engine.Evaluate("parseResult4 === null").AsBoolean();
        var constructorResult4IsNull = Engine.Evaluate("constructorResult4 === null").AsBoolean();

        Assert.False(parseResult1IsNull);
        Assert.False(constructorResult1IsNull);

        Assert.True(parseResult2IsNull);
        Assert.True(constructorResult2IsNull);

        Assert.False(parseResult3IsNull);
        Assert.False(constructorResult3IsNull);

        Assert.True(parseResult4IsNull);
        Assert.True(constructorResult4IsNull);
    }

    [Fact]
    public void CanParseAndParseShouldBeConsistent()
    {
        // Valid URLs - canParse should return true, parse should return object
        Engine.Execute("const validUrl = 'https://example.com/path';");
        Engine.Execute("const canParseValid = URL.canParse(validUrl);");
        Engine.Execute("const parseValid = URL.parse(validUrl);");

        var canParseValid = Engine.Evaluate("canParseValid").AsBoolean();
        var parseValidIsNull = Engine.Evaluate("parseValid === null").AsBoolean();

        Assert.True(canParseValid);
        Assert.False(parseValidIsNull);

        // Invalid URLs - canParse should return false, parse should return null
        Engine.Execute("const invalidUrl = 'invalid-url';");
        Engine.Execute("const canParseInvalid = URL.canParse(invalidUrl);");
        Engine.Execute("const parseInvalid = URL.parse(invalidUrl);");

        var canParseInvalid = Engine.Evaluate("canParseInvalid").AsBoolean();
        var parseInvalidIsNull = Engine.Evaluate("parseInvalid === null").AsBoolean();

        Assert.False(canParseInvalid);
        Assert.True(parseInvalidIsNull);

        // Relative URL without base - canParse should return false, parse should return null
        Engine.Execute("const relativeUrl = '/path';");
        Engine.Execute("const canParseRelative = URL.canParse(relativeUrl);");
        Engine.Execute("const parseRelative = URL.parse(relativeUrl);");

        var canParseRelative = Engine.Evaluate("canParseRelative").AsBoolean();
        var parseRelativeIsNull = Engine.Evaluate("parseRelative === null").AsBoolean();

        Assert.False(canParseRelative);
        Assert.True(parseRelativeIsNull);

        // Relative URL with base - canParse should return true, parse should return object
        Engine.Execute("const baseUrl = 'https://example.com';");
        Engine.Execute("const canParseRelativeWithBase = URL.canParse(relativeUrl, baseUrl);");
        Engine.Execute("const parseRelativeWithBase = URL.parse(relativeUrl, baseUrl);");

        var canParseRelativeWithBase = Engine.Evaluate("canParseRelativeWithBase").AsBoolean();
        var parseRelativeWithBaseIsNull = Engine
            .Evaluate("parseRelativeWithBase === null")
            .AsBoolean();

        Assert.True(canParseRelativeWithBase);
        Assert.False(parseRelativeWithBaseIsNull);
    }
}
