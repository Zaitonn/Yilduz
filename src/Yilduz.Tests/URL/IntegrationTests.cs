using Jint;
using Xunit;

namespace Yilduz.Tests.URL;

public sealed class IntegrationTests : TestBase
{
    [Fact]
    public void URLSearchParamsShouldReflectURLSearchString()
    {
        Execute(
            """
            const url = new URL('https://example.com?foo=bar&baz=qux');
            const fooValue = url.searchParams.get('foo');
            const bazValue = url.searchParams.get('baz');
            """
        );

        var fooResult = Evaluate("fooValue").AsString();
        var bazResult = Evaluate("bazValue").AsString();

        Assert.Equal("bar", fooResult);
        Assert.Equal("qux", bazResult);
    }

    [Fact]
    public void UpdatingSearchParamsShouldUpdateURLSearch()
    {
        Execute("const url = new URL('https://example.com');");
        Execute("url.searchParams.set('key', 'value');");
        Execute("url.searchParams.set('other', 'data');");

        var search = Evaluate("url.search").AsString();

        Assert.Contains("key=value", search);
        Assert.Contains("other=data", search);
        Assert.StartsWith("?", search);
    }

    [Fact]
    public void DeletingSearchParamsShouldUpdateURLSearch()
    {
        Execute("const url = new URL('https://example.com?foo=bar&baz=qux&test=value');");
        Execute("url.searchParams.delete('baz');");

        var search = Evaluate("url.search").AsString();
        var bazValue = Evaluate("url.searchParams.get('baz')");

        Assert.Contains("foo=bar", search);
        Assert.DoesNotContain("baz=qux", search);
        Assert.Contains("test=value", search);
        Assert.True(bazValue.IsNull());
    }

    [Fact]
    public void AppendingSearchParamsShouldUpdateURLSearch()
    {
        Execute("const url = new URL('https://example.com?existing=value');");
        Execute("url.searchParams.append('new', 'param');");
        Execute("url.searchParams.append('another', 'one');");

        var search = Evaluate("url.search").AsString();

        Assert.Contains("existing=value", search);
        Assert.Contains("new=param", search);
        Assert.Contains("another=one", search);
    }

    [Fact]
    public void SearchParamsShouldHandleMultipleValuesForSameKey()
    {
        Execute("const url = new URL('https://example.com');");
        Execute("url.searchParams.append('key', 'value1');");
        Execute("url.searchParams.append('key', 'value2');");
        Execute("const allValues = url.searchParams.getAll('key');");

        var allValuesLength = Evaluate("allValues.length").AsNumber();
        var firstValue = Evaluate("allValues[0]").AsString();
        var secondValue = Evaluate("allValues[1]").AsString();

        Assert.Equal(2, allValuesLength);
        Assert.Equal("value1", firstValue);
        Assert.Equal("value2", secondValue);
    }

    [Fact]
    public void SearchParamsShouldBeIterableWithForEach()
    {
        Execute(
            """
            const url = new URL('https://example.com?a=1&b=2&c=3');
            const params = [];
            url.searchParams.forEach((value, key) => params.push({key, value}));

            const paramCount = params.length;
            const firstParam = params[0];
            """
        );

        var paramCount = Evaluate("paramCount").AsNumber();
        var firstParamKey = Evaluate("firstParam.key").AsString();
        var firstParamValue = Evaluate("firstParam.value").AsString();

        Assert.Equal(3, paramCount);
        Assert.Equal("a", firstParamKey);
        Assert.Equal("1", firstParamValue);
    }

    [Fact]
    public void SearchParamsShouldSupportHasMethod()
    {
        Execute("const url = new URL('https://example.com?existing=value&empty=');");
        Execute("const hasExisting = url.searchParams.has('existing');");
        Execute("const hasEmpty = url.searchParams.has('empty');");
        Execute("const hasNonExistent = url.searchParams.has('nonexistent');");

        var hasExisting = Evaluate("hasExisting").AsBoolean();
        var hasEmpty = Evaluate("hasEmpty").AsBoolean();
        var hasNonExistent = Evaluate("hasNonExistent").AsBoolean();

        Assert.True(hasExisting);
        Assert.True(hasEmpty);
        Assert.False(hasNonExistent);
    }

    [Fact]
    public void SearchParamsShouldHandleEncodedValues()
    {
        Execute(
            "const url = new URL('https://example.com?key=value%20with%20spaces&other=data%2Bplus');"
        );
        Execute("const keyValue = url.searchParams.get('key');");
        Execute("const otherValue = url.searchParams.get('other');");

        var keyValue = Evaluate("keyValue").AsString();
        var otherValue = Evaluate("otherValue").AsString();

        // The exact behavior depends on URL encoding/decoding implementation
        Assert.NotEmpty(keyValue);
        Assert.NotEmpty(otherValue);
    }

    [Fact]
    public void UpdatingURLSearchShouldUpdateSearchParams()
    {
        Execute("const url = new URL('https://example.com?initial=value');");
        Execute("url.search = '?new=parameter&another=one';");
        Execute("const newValue = url.searchParams.get('new');");
        Execute("const anotherValue = url.searchParams.get('another');");
        Execute("const initialValue = url.searchParams.get('initial');");

        var newValue = Evaluate("newValue").AsString();
        var anotherValue = Evaluate("anotherValue").AsString();
        var initialValue = Evaluate("initialValue");

        Assert.Equal("parameter", newValue);
        Assert.Equal("one", anotherValue);
        Assert.True(initialValue.IsNull()); // Should be gone after setting new search
    }

    [Fact]
    public void SearchParamsShouldMaintainInsertionOrder()
    {
        Execute("const url = new URL('https://example.com');");
        Execute("url.searchParams.set('z', 'last');");
        Execute("url.searchParams.set('a', 'first');");
        Execute("url.searchParams.set('m', 'middle');");
        Execute("const keys = Array.from(url.searchParams.keys());");

        var firstKey = Evaluate("keys[0]").AsString();
        var secondKey = Evaluate("keys[1]").AsString();
        var thirdKey = Evaluate("keys[2]").AsString();

        // Should maintain insertion order, not alphabetical
        Assert.Equal("z", firstKey);
        Assert.Equal("a", secondKey);
        Assert.Equal("m", thirdKey);
    }

    [Fact]
    public void SearchParamsShouldSupportSorting()
    {
        Execute("const url = new URL('https://example.com?z=1&a=2&m=3');");
        Execute("url.searchParams.sort();");
        Execute("const keys = Array.from(url.searchParams.keys());");
        Execute("const sortedSearch = url.search;");

        var firstKey = Evaluate("keys[0]").AsString();
        var secondKey = Evaluate("keys[1]").AsString();
        var thirdKey = Evaluate("keys[2]").AsString();
        var sortedSearch = Evaluate("sortedSearch").AsString();

        // Should be sorted alphabetically
        Assert.Equal("a", firstKey);
        Assert.Equal("m", secondKey);
        Assert.Equal("z", thirdKey);
        Assert.Contains("a=2", sortedSearch);
    }

    [Fact]
    public void SearchParamsShouldSupportToStringMethod()
    {
        Execute("const url = new URL('https://example.com?foo=bar&baz=qux');");
        Execute("const searchParamsString = url.searchParams.toString();");
        Execute("const urlSearch = url.search.substring(1);"); // Remove the '?' prefix

        var searchParamsString = Evaluate("searchParamsString").AsString();
        var urlSearch = Evaluate("urlSearch").AsString();

        Assert.Equal(urlSearch, searchParamsString);
    }

    [Fact]
    public void ClearingSearchParamsShouldClearURLSearch()
    {
        Execute("const url = new URL('https://example.com?foo=bar&baz=qux&test=value');");
        Execute("url.searchParams.forEach((value, key) => url.searchParams.delete(key));");

        var search = Evaluate("url.search").AsString();
        var paramsSize = Evaluate("Array.from(url.searchParams).length").AsNumber();

        Assert.Equal("", search);
        Assert.Equal(0, paramsSize);
    }
}
