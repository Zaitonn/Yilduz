using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.URL;

public sealed class StaticMethodsTests : TestBase
{
    [Fact]
    public void CanParseShouldReturnTrueForValidURL()
    {
        var result = Evaluate("URL.canParse('https://example.com')").AsBoolean();
        Assert.True(result);
    }

    [Fact]
    public void CanParseShouldReturnTrueForValidRelativeURLWithBase()
    {
        var result = Evaluate("URL.canParse('/path', 'https://example.com')").AsBoolean();
        Assert.True(result);
    }

    [Fact]
    public void CanParseShouldReturnFalseForInvalidURL()
    {
        var result = Evaluate("URL.canParse('invalid-url')").AsBoolean();
        Assert.False(result);
    }

    [Fact]
    public void CanParseShouldReturnFalseForRelativeURLWithoutBase()
    {
        var result = Evaluate("URL.canParse('/path')").AsBoolean();
        Assert.False(result);
    }

    [Fact]
    public void CanParseShouldReturnFalseForEmptyURL()
    {
        var result = Evaluate("URL.canParse('')").AsBoolean();
        Assert.False(result);
    }

    [Fact]
    public void ParseShouldReturnURLInstanceForValidURL()
    {
        Execute("const parsed = URL.parse('https://example.com/path');");

        var href = Evaluate("parsed.href").AsString();
        var hostname = Evaluate("parsed.hostname").AsString();
        var pathname = Evaluate("parsed.pathname").AsString();

        Assert.Equal("https://example.com/path", href);
        Assert.Equal("example.com", hostname);
        Assert.Equal("/path", pathname);
    }

    [Fact]
    public void ParseShouldReturnURLInstanceForValidRelativeURLWithBase()
    {
        Execute("const parsed = URL.parse('/path', 'https://example.com');");

        var href = Evaluate("parsed.href").AsString();
        var hostname = Evaluate("parsed.hostname").AsString();
        var pathname = Evaluate("parsed.pathname").AsString();

        Assert.Equal("https://example.com/path", href);
        Assert.Equal("example.com", hostname);
        Assert.Equal("/path", pathname);
    }

    [Fact]
    public void ParseShouldReturnNullForInvalidURL()
    {
        Execute("const parsed = URL.parse('invalid-url');");
        var result = Evaluate("parsed");

        Assert.True(result.IsNull());
    }

    [Fact]
    public void ParseShouldReturnNullForRelativeURLWithoutBase()
    {
        Execute("const parsed = URL.parse('/path');");
        var result = Evaluate("parsed");

        Assert.True(result.IsNull());
    }

    [Fact]
    public void ParseShouldReturnNullForEmptyURL()
    {
        Execute("const parsed = URL.parse('');");
        var result = Evaluate("parsed");

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
        Execute(
            "const parsed = URL.parse('https://user:pass@subdomain.example.com:8080/path?query=value#hash');"
        );

        var username = Evaluate("parsed.username").AsString();
        var password = Evaluate("parsed.password").AsString();
        var hostname = Evaluate("parsed.hostname").AsString();
        var port = Evaluate("parsed.port").AsString();
        var search = Evaluate("parsed.search").AsString();
        var hash = Evaluate("parsed.hash").AsString();

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
        Assert.True(Evaluate("URL.canParse('http://example.com')").AsBoolean());
        Assert.True(Evaluate("URL.canParse('https://example.com')").AsBoolean());
        Assert.True(Evaluate("URL.canParse('ftp://ftp.example.com')").AsBoolean());
        Assert.True(Evaluate("URL.canParse('file:///path/to/file')").AsBoolean());
    }

    [Fact]
    public void ParseShouldHandleDifferentProtocols()
    {
        Execute("const httpUrl = URL.parse('http://example.com');");
        Execute("const ftpUrl = URL.parse('ftp://ftp.example.com');");

        var httpProtocol = Evaluate("httpUrl.protocol").AsString();
        var ftpProtocol = Evaluate("ftpUrl.protocol").AsString();

        Assert.Equal("http:", httpProtocol);
        Assert.Equal("ftp:", ftpProtocol);
    }

    [Fact]
    public void CanParseShouldRequireAtLeastOneArgument()
    {
        Assert.Throws<JavaScriptException>(() => Execute("URL.canParse();"));
    }

    [Fact]
    public void ParseShouldRequireAtLeastOneArgument()
    {
        Assert.Throws<JavaScriptException>(() => Execute("URL.parse();"));
    }

    [Fact]
    public void ParseShouldReturnNullForMalformedURL()
    {
        Execute("const parsed = URL.parse('https://');");
        var result = Evaluate("parsed");

        Assert.True(result.IsNull());
    }

    [Fact]
    public void ParseShouldReturnNullForInvalidProtocol()
    {
        Execute("const parsed = URL.parse('🥺://example.com');");
        var result = Evaluate("parsed");

        Assert.True(result.IsNull());
    }

    [Fact]
    public void ParseShouldReturnValidURLObjectForValidInput()
    {
        Execute("const parsed = URL.parse('https://example.com/path');");
        Execute("const isNull = parsed === null;");
        Execute("const isURL = parsed instanceof URL;");

        var isNull = Evaluate("isNull").AsBoolean();
        var isURL = Evaluate("isURL").AsBoolean();

        Assert.False(isNull);
        Assert.True(isURL);
    }

    [Fact]
    public void ParseShouldWorkAsAlternativeToTryCatchWithConstructor()
    {
        Execute(
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
        var parseResult1IsNull = Evaluate("parseResult1 === null").AsBoolean();
        var constructorResult1IsNull = Evaluate("constructorResult1 === null").AsBoolean();

        // Invalid URL should return null in both cases
        var parseResult2IsNull = Evaluate("parseResult2 === null").AsBoolean();
        var constructorResult2IsNull = Evaluate("constructorResult2 === null").AsBoolean();

        // Relative URL with base should return URL objects in both cases
        var parseResult3IsNull = Evaluate("parseResult3 === null").AsBoolean();
        var constructorResult3IsNull = Evaluate("constructorResult3 === null").AsBoolean();

        // Relative URL without base should return null in both cases
        var parseResult4IsNull = Evaluate("parseResult4 === null").AsBoolean();
        var constructorResult4IsNull = Evaluate("constructorResult4 === null").AsBoolean();

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
        Execute("const validUrl = 'https://example.com/path';");
        Execute("const canParseValid = URL.canParse(validUrl);");
        Execute("const parseValid = URL.parse(validUrl);");

        var canParseValid = Evaluate("canParseValid").AsBoolean();
        var parseValidIsNull = Evaluate("parseValid === null").AsBoolean();

        Assert.True(canParseValid);
        Assert.False(parseValidIsNull);

        // Invalid URLs - canParse should return false, parse should return null
        Execute("const invalidUrl = 'invalid-url';");
        Execute("const canParseInvalid = URL.canParse(invalidUrl);");
        Execute("const parseInvalid = URL.parse(invalidUrl);");

        var canParseInvalid = Evaluate("canParseInvalid").AsBoolean();
        var parseInvalidIsNull = Evaluate("parseInvalid === null").AsBoolean();

        Assert.False(canParseInvalid);
        Assert.True(parseInvalidIsNull);

        // Relative URL without base - canParse should return false, parse should return null
        Execute("const relativeUrl = '/path';");
        Execute("const canParseRelative = URL.canParse(relativeUrl);");
        Execute("const parseRelative = URL.parse(relativeUrl);");

        var canParseRelative = Evaluate("canParseRelative").AsBoolean();
        var parseRelativeIsNull = Evaluate("parseRelative === null").AsBoolean();

        Assert.False(canParseRelative);
        Assert.True(parseRelativeIsNull);

        // Relative URL with base - canParse should return true, parse should return object
        Execute("const baseUrl = 'https://example.com';");
        Execute("const canParseRelativeWithBase = URL.canParse(relativeUrl, baseUrl);");
        Execute("const parseRelativeWithBase = URL.parse(relativeUrl, baseUrl);");

        var canParseRelativeWithBase = Evaluate("canParseRelativeWithBase").AsBoolean();
        var parseRelativeWithBaseIsNull = Engine
            .Evaluate("parseRelativeWithBase === null")
            .AsBoolean();

        Assert.True(canParseRelativeWithBase);
        Assert.False(parseRelativeWithBaseIsNull);
    }
}
