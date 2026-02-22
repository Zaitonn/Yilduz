using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.Headers;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void AppendAndGetShouldWork()
    {
        Execute(
            "const headers = new Headers(); headers.append('Content-Type', 'application/json');"
        );
        var value = Evaluate("headers.get('content-type')").AsString();

        Assert.Equal("application/json", value);
    }

    [Fact]
    public void SetShouldReplaceExistingValues()
    {
        Execute(
            """
            const headers = new Headers();
            headers.append('X-Test', 'one');
            headers.append('x-test', 'two');
            headers.set('x-test', 'final');
            const entriesCount = Array.from(headers.entries()).length;
            """
        );

        var value = Evaluate("headers.get('X-Test')").AsString();
        var entriesCount = (int)Evaluate("entriesCount").AsNumber();

        Assert.Equal("final", value);
        Assert.Equal(1, entriesCount);
    }

    [Fact]
    public void DeleteShouldRemoveValues()
    {
        Execute(
            """
            const headers = new Headers();
            headers.append('X-Remove', 'value');
            headers.delete('x-remove');
            const hasValue = headers.has('X-Remove');
            """
        );

        var hasValue = Evaluate("hasValue").AsBoolean();
        Assert.False(hasValue);
    }

    [Fact]
    public void HasShouldBeCaseInsensitive()
    {
        Execute(
            """
            const headers = new Headers();
            headers.append('X-Custom', 'value');
            const hasLower = headers.has('x-custom');
            const hasUpper = headers.has('X-CUSTOM');
            """
        );

        Assert.True(Evaluate("hasLower").AsBoolean());
        Assert.True(Evaluate("hasUpper").AsBoolean());
    }

    [Fact]
    public void GetShouldCombineValues()
    {
        Execute(
            """
            const headers = new Headers();
            headers.append('X-Test', 'one');
            headers.append('x-test', 'two');
            const combined = headers.get('X-Test');
            """
        );

        Assert.Equal("one, two", Evaluate("combined").AsString());
    }

    [Fact]
    public void IteratorsShouldBeSortedAndCombined()
    {
        Execute(
            """
            const headers = new Headers();
            headers.append('b-header', 'two');
            headers.append('a-header', 'one');
            headers.append('A-Header', 'three');

            const entries = Array.from(headers.entries());
            const keys = Array.from(headers.keys());
            const values = Array.from(headers.values());
            const defaults = Array.from(headers);
            """
        );

        var entries = Evaluate("entries").AsArray();
        var keys = Evaluate("keys").AsArray();
        var values = Evaluate("values").AsArray();
        var defaults = Evaluate("defaults").AsArray();

        Assert.Equal<uint>(2, entries.Length);
        Assert.Equal<uint>(2, keys.Length);
        Assert.Equal<uint>(2, values.Length);
        Assert.Equal<uint>(2, defaults.Length);

        var firstEntry = entries[0].AsArray();
        Assert.Equal("a-header", firstEntry[0].AsString());
        Assert.Equal("one, three", firstEntry[1].AsString());

        var secondEntry = entries[1].AsArray();
        Assert.Equal("b-header", secondEntry[0].AsString());
        Assert.Equal("two", secondEntry[1].AsString());

        Assert.Equal("a-header", keys[0].AsString());
        Assert.Equal("b-header", keys[1].AsString());

        Assert.Equal("one, three", values[0].AsString());
        Assert.Equal("two", values[1].AsString());

        var defaultEntry = defaults[0].AsArray();
        Assert.Equal("a-header", defaultEntry[0].AsString());
        Assert.Equal("one, three", defaultEntry[1].AsString());
    }

    [Fact]
    public void ForEachShouldSupportThisArg()
    {
        Execute(
            """
            const headers = new Headers();
            headers.append('Name', 'John');
            const ctx = { marker: 'CTX' };
            const seen = [];

            headers.forEach(function(value, key) {
                seen.push({ key, value, correctThis: this === ctx });
            }, ctx);
            """
        );

        var seen = Evaluate("seen").AsArray();
        Assert.Equal<uint>(1, seen.Length);

        var first = seen[0].AsObject();
        Assert.Equal("name", first.Get("key").AsString());
        Assert.Equal("John", first.Get("value").AsString());
        Assert.True(first.Get("correctThis").AsBoolean());
    }

    [Fact]
    public void ConstructorShouldAcceptPlainObject()
    {
        Execute("const headers = new Headers({ 'Content-Type': 'text/plain', 'X-Test': 'ok' });");

        Assert.Equal("text/plain", Evaluate("headers.get('content-type')").AsString());
        Assert.Equal("ok", Evaluate("headers.get('x-test')").AsString());
    }

    [Fact]
    public void ConstructorShouldThrowForInvalidRecordName()
    {
        Assert.Throws<JavaScriptException>(() => Execute("new Headers({ 'bad name': 'v' });"));
    }

    [Fact]
    public void ConstructorShouldThrowForInvalidRecordValue()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("new Headers({ 'X-Test': 'line\\nbreak' });")
        );
    }

    [Fact]
    public void ConstructorShouldThrowForIterableNonObjectEntry()
    {
        Assert.Throws<JavaScriptException>(() => Execute("new Headers([1]);"));
    }

    [Fact]
    public void ConstructorShouldThrowForIterableEntryWithoutLength()
    {
        Assert.Throws<JavaScriptException>(() => Execute("new Headers([{}]);"));
    }

    [Fact]
    public void GetSetCookieShouldReturnEmptyArrayWhenNone()
    {
        Execute("const h = new Headers(); const cookies = h.getSetCookie();");

        var cookies = Evaluate("cookies").AsArray();
        Assert.Equal<uint>(0, cookies.Length);
    }

    [Fact]
    public void GetSetCookieShouldReturnAllSetCookieValues()
    {
        Execute(
            """
            const h = new Headers();
            h.append('Set-Cookie', 'a=1');
            h.append('set-cookie', 'b=2');
            h.append('x-test', 'ignore');
            const cookies = h.getSetCookie();
            """
        );

        var cookies = Evaluate("cookies").AsArray();
        Assert.Equal<uint>(2, cookies.Length);
        Assert.Equal("a=1", cookies[0].AsString());
        Assert.Equal("b=2", cookies[1].AsString());
    }

    [Fact]
    public void ConstructorShouldThrowForInvalidType()
    {
        Assert.Throws<JavaScriptException>(() => Execute("new Headers(114514);"));
        Assert.Throws<JavaScriptException>(() => Execute("new Headers('114514');"));
    }

    [Fact]
    public void ConstructorShouldThrowForIterableEntryWithShortLength()
    {
        Assert.Throws<JavaScriptException>(() => Execute("new Headers([{ length: 1, 0: 'a' }]);"));
    }

    [Fact]
    public void ConstructorShouldAcceptIterablePairs()
    {
        Execute(
            """
            const headers = new Headers([
                ['X-Test', 'one'],
                ['x-test', 'two'],
            ]);
            """
        );

        Assert.Equal("one, two", Evaluate("headers.get('x-test')").AsString());
    }

    [Fact]
    public void ConstructorShouldCopyExistingHeaders()
    {
        Execute(
            """
            const first = new Headers({ 'X-Test': 'one' });
            const second = new Headers(first);
            first.set('X-Test', 'changed');

            const firstValue = first.get('x-test');
            const secondValue = second.get('x-test');
            """
        );

        Assert.Equal("changed", Evaluate("firstValue").AsString());
        Assert.Equal("one", Evaluate("secondValue").AsString());
    }

    [Fact]
    public void InvalidHeaderNameShouldThrow()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("const h = new Headers(); h.append('bad name', 'v');")
        );
    }

    [Fact]
    public void InvalidHeaderNameInDeleteShouldThrow()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("const h = new Headers(); h.delete('bad name');")
        );
    }

    [Fact]
    public void InvalidHeaderNameInGetShouldThrow()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("const h = new Headers(); h.get('bad name');")
        );
    }

    [Fact]
    public void InvalidHeaderNameInHasShouldThrow()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("const h = new Headers(); h.has('bad name');")
        );
    }

    [Fact]
    public void InvalidHeaderValueShouldThrow()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("const h = new Headers(); h.append('x-test', 'line\nbreak');")
        );
    }

    [Fact]
    public void InvalidHeaderValueInSetShouldThrow()
    {
        Assert.Throws<JavaScriptException>(
            () => Execute("const h = new Headers(); h.set('x-test', 'line\nbreak');")
        );
    }
}
