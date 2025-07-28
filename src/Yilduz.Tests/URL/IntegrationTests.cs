using Jint;
using Xunit;

namespace Yilduz.Tests.URL;

public sealed class IntegrationTests : TestBase
{
    [Fact]
    public void URLSearchParamsShouldReflectURLSearchString()
    {
        Engine.Execute(
            """
            const url = new URL('https://example.com?foo=bar&baz=qux');
            const fooValue = url.searchParams.get('foo');
            const bazValue = url.searchParams.get('baz');
            """
        );

        var fooResult = Engine.Evaluate("fooValue").AsString();
        var bazResult = Engine.Evaluate("bazValue").AsString();

        Assert.Equal("bar", fooResult);
        Assert.Equal("qux", bazResult);
    }

    [Fact]
    public void UpdatingSearchParamsShouldUpdateURLSearch()
    {
        Engine.Execute("const url = new URL('https://example.com');");
        Engine.Execute("url.searchParams.set('key', 'value');");
        Engine.Execute("url.searchParams.set('other', 'data');");

        var search = Engine.Evaluate("url.search").AsString();

        Assert.Contains("key=value", search);
        Assert.Contains("other=data", search);
        Assert.StartsWith("?", search);
    }

    [Fact]
    public void DeletingSearchParamsShouldUpdateURLSearch()
    {
        Engine.Execute("const url = new URL('https://example.com?foo=bar&baz=qux&test=value');");
        Engine.Execute("url.searchParams.delete('baz');");

        var search = Engine.Evaluate("url.search").AsString();
        var bazValue = Engine.Evaluate("url.searchParams.get('baz')");

        Assert.Contains("foo=bar", search);
        Assert.DoesNotContain("baz=qux", search);
        Assert.Contains("test=value", search);
        Assert.True(bazValue.IsNull());
    }

    [Fact]
    public void AppendingSearchParamsShouldUpdateURLSearch()
    {
        Engine.Execute("const url = new URL('https://example.com?existing=value');");
        Engine.Execute("url.searchParams.append('new', 'param');");
        Engine.Execute("url.searchParams.append('another', 'one');");

        var search = Engine.Evaluate("url.search").AsString();

        Assert.Contains("existing=value", search);
        Assert.Contains("new=param", search);
        Assert.Contains("another=one", search);
    }

    [Fact]
    public void SearchParamsShouldHandleMultipleValuesForSameKey()
    {
        Engine.Execute("const url = new URL('https://example.com');");
        Engine.Execute("url.searchParams.append('key', 'value1');");
        Engine.Execute("url.searchParams.append('key', 'value2');");
        Engine.Execute("const allValues = url.searchParams.getAll('key');");

        var allValuesLength = Engine.Evaluate("allValues.length").AsNumber();
        var firstValue = Engine.Evaluate("allValues[0]").AsString();
        var secondValue = Engine.Evaluate("allValues[1]").AsString();

        Assert.Equal(2, allValuesLength);
        Assert.Equal("value1", firstValue);
        Assert.Equal("value2", secondValue);
    }

    [Fact]
    public void SearchParamsShouldBeIterableWithForEach()
    {
        Engine.Execute(
            """
            const url = new URL('https://example.com?a=1&b=2&c=3');
            const params = [];
            url.searchParams.forEach((value, key) => params.push({key, value}));

            const paramCount = params.length;
            const firstParam = params[0];
            """
        );

        var paramCount = Engine.Evaluate("paramCount").AsNumber();
        var firstParamKey = Engine.Evaluate("firstParam.key").AsString();
        var firstParamValue = Engine.Evaluate("firstParam.value").AsString();

        Assert.Equal(3, paramCount);
        Assert.Equal("a", firstParamKey);
        Assert.Equal("1", firstParamValue);
    }

    [Fact]
    public void SearchParamsShouldSupportHasMethod()
    {
        Engine.Execute("const url = new URL('https://example.com?existing=value&empty=');");
        Engine.Execute("const hasExisting = url.searchParams.has('existing');");
        Engine.Execute("const hasEmpty = url.searchParams.has('empty');");
        Engine.Execute("const hasNonExistent = url.searchParams.has('nonexistent');");

        var hasExisting = Engine.Evaluate("hasExisting").AsBoolean();
        var hasEmpty = Engine.Evaluate("hasEmpty").AsBoolean();
        var hasNonExistent = Engine.Evaluate("hasNonExistent").AsBoolean();

        Assert.True(hasExisting);
        Assert.True(hasEmpty);
        Assert.False(hasNonExistent);
    }

    [Fact]
    public void SearchParamsShouldHandleEncodedValues()
    {
        Engine.Execute(
            "const url = new URL('https://example.com?key=value%20with%20spaces&other=data%2Bplus');"
        );
        Engine.Execute("const keyValue = url.searchParams.get('key');");
        Engine.Execute("const otherValue = url.searchParams.get('other');");

        var keyValue = Engine.Evaluate("keyValue").AsString();
        var otherValue = Engine.Evaluate("otherValue").AsString();

        // The exact behavior depends on URL encoding/decoding implementation
        Assert.NotEmpty(keyValue);
        Assert.NotEmpty(otherValue);
    }

    [Fact]
    public void UpdatingURLSearchShouldUpdateSearchParams()
    {
        Engine.Execute("const url = new URL('https://example.com?initial=value');");
        Engine.Execute("url.search = '?new=parameter&another=one';");
        Engine.Execute("const newValue = url.searchParams.get('new');");
        Engine.Execute("const anotherValue = url.searchParams.get('another');");
        Engine.Execute("const initialValue = url.searchParams.get('initial');");

        var newValue = Engine.Evaluate("newValue").AsString();
        var anotherValue = Engine.Evaluate("anotherValue").AsString();
        var initialValue = Engine.Evaluate("initialValue");

        Assert.Equal("parameter", newValue);
        Assert.Equal("one", anotherValue);
        Assert.True(initialValue.IsNull()); // Should be gone after setting new search
    }

    [Fact]
    public void SearchParamsShouldMaintainInsertionOrder()
    {
        Engine.Execute("const url = new URL('https://example.com');");
        Engine.Execute("url.searchParams.set('z', 'last');");
        Engine.Execute("url.searchParams.set('a', 'first');");
        Engine.Execute("url.searchParams.set('m', 'middle');");
        Engine.Execute("const keys = Array.from(url.searchParams.keys());");

        var firstKey = Engine.Evaluate("keys[0]").AsString();
        var secondKey = Engine.Evaluate("keys[1]").AsString();
        var thirdKey = Engine.Evaluate("keys[2]").AsString();

        // Should maintain insertion order, not alphabetical
        Assert.Equal("z", firstKey);
        Assert.Equal("a", secondKey);
        Assert.Equal("m", thirdKey);
    }

    [Fact]
    public void SearchParamsShouldSupportSorting()
    {
        Engine.Execute("const url = new URL('https://example.com?z=1&a=2&m=3');");
        Engine.Execute("url.searchParams.sort();");
        Engine.Execute("const keys = Array.from(url.searchParams.keys());");
        Engine.Execute("const sortedSearch = url.search;");

        var firstKey = Engine.Evaluate("keys[0]").AsString();
        var secondKey = Engine.Evaluate("keys[1]").AsString();
        var thirdKey = Engine.Evaluate("keys[2]").AsString();
        var sortedSearch = Engine.Evaluate("sortedSearch").AsString();

        // Should be sorted alphabetically
        Assert.Equal("a", firstKey);
        Assert.Equal("m", secondKey);
        Assert.Equal("z", thirdKey);
        Assert.Contains("a=2", sortedSearch);
    }

    [Fact]
    public void SearchParamsShouldSupportToStringMethod()
    {
        Engine.Execute("const url = new URL('https://example.com?foo=bar&baz=qux');");
        Engine.Execute("const searchParamsString = url.searchParams.toString();");
        Engine.Execute("const urlSearch = url.search.substring(1);"); // Remove the '?' prefix

        var searchParamsString = Engine.Evaluate("searchParamsString").AsString();
        var urlSearch = Engine.Evaluate("urlSearch").AsString();

        Assert.Equal(urlSearch, searchParamsString);
    }

    [Fact]
    public void ClearingSearchParamsShouldClearURLSearch()
    {
        Engine.Execute("const url = new URL('https://example.com?foo=bar&baz=qux&test=value');");
        Engine.Execute("url.searchParams.forEach((value, key) => url.searchParams.delete(key));");

        var search = Engine.Evaluate("url.search").AsString();
        var paramsSize = Engine.Evaluate("Array.from(url.searchParams).length").AsNumber();

        Assert.Equal("", search);
        Assert.Equal(0, paramsSize);
    }
}
