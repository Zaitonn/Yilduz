using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.URLSearchParams;

public sealed class MethodTests : TestBase
{
    [Fact]
    public void AppendShouldAddParameter()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.append('foo', 'bar');");

        var size = Evaluate("params.size").AsNumber();
        var value = Evaluate("params.get('foo')").AsString();

        Assert.Equal(1, size);
        Assert.Equal("bar", value);
    }

    [Fact]
    public void AppendShouldAllowDuplicateKeys()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.append('foo', 'bar');");
        Execute("params.append('foo', 'baz');");

        var size = Evaluate("params.size").AsNumber();
        var valuesLength = Evaluate("params.getAll('foo').length").AsNumber();
        var firstValue = Evaluate("params.getAll('foo')[0]").AsString();
        var secondValue = Evaluate("params.getAll('foo')[1]").AsString();

        Assert.Equal(2, size);
        Assert.Equal(2, valuesLength);
        Assert.Equal("bar", firstValue);
        Assert.Equal("baz", secondValue);
    }

    [Fact]
    public void GetShouldReturnFirstValue()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.append('foo', 'bar');");
        Execute("params.append('foo', 'baz');");

        var value = Evaluate("params.get('foo')").AsString();
        Assert.Equal("bar", value);
    }

    [Fact]
    public void GetShouldReturnNullForNonExistentKey()
    {
        Execute("const params = new URLSearchParams();");
        var value = Evaluate("params.get('nonexistent')");
        Assert.True(value.IsNull());
    }

    [Fact]
    public void GetAllShouldReturnAllValues()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.append('foo', 'bar');");
        Execute("params.append('foo', 'baz');");
        Execute("params.append('qux', 'quux');");

        var fooValuesLength = Evaluate("params.getAll('foo').length").AsNumber();
        var fooFirstValue = Evaluate("params.getAll('foo')[0]").AsString();
        var fooSecondValue = Evaluate("params.getAll('foo')[1]").AsString();
        var quxValuesLength = Evaluate("params.getAll('qux').length").AsNumber();
        var quxFirstValue = Evaluate("params.getAll('qux')[0]").AsString();
        var nonExistentValuesLength = Engine
            .Evaluate("params.getAll('nonexistent').length")
            .AsNumber();

        Assert.Equal(2, fooValuesLength);
        Assert.Equal("bar", fooFirstValue);
        Assert.Equal("baz", fooSecondValue);

        Assert.Equal(1, quxValuesLength);
        Assert.Equal("quux", quxFirstValue);

        Assert.Equal(0, nonExistentValuesLength);
    }

    [Fact]
    public void DeleteShouldRemoveAllOccurrences()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.append('foo', 'bar');");
        Execute("params.append('foo', 'baz');");
        Execute("params.append('qux', 'quux');");
        Execute("params.delete('foo');");

        var size = Evaluate("params.size").AsNumber();
        var hasFoo = Evaluate("params.has('foo')").AsBoolean();
        var hasQux = Evaluate("params.has('qux')").AsBoolean();

        Assert.Equal(1, size);
        Assert.False(hasFoo);
        Assert.True(hasQux);
    }

    [Fact]
    public void HasShouldReturnTrueForExistingKey()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.append('foo', 'bar');");

        var hasFoo = Evaluate("params.has('foo')").AsBoolean();
        var hasBar = Evaluate("params.has('bar')").AsBoolean();

        Assert.True(hasFoo);
        Assert.False(hasBar);
    }

    [Fact]
    public void HasWithValueShouldReturnTrueForExactMatch()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.append('foo', 'bar');");
        Execute("params.append('foo', 'baz');");

        var hasExactMatch = Evaluate("params.has('foo', 'bar')").AsBoolean();
        var hasNoMatch = Evaluate("params.has('foo', 'qux')").AsBoolean();

        Assert.True(hasExactMatch);
        Assert.False(hasNoMatch);
    }

    [Fact]
    public void SetShouldReplaceAllOccurrences()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.append('foo', 'bar');");
        Execute("params.append('foo', 'baz');");
        Execute("params.append('qux', 'quux');");
        Execute("params.set('foo', 'new-value');");

        var size = Evaluate("params.size").AsNumber();
        var fooValue = Evaluate("params.get('foo')").AsString();
        var fooValuesLength = Evaluate("params.getAll('foo').length").AsNumber();
        var fooFirstValue = Evaluate("params.getAll('foo')[0]").AsString();

        Assert.Equal(2, size);
        Assert.Equal("new-value", fooValue);
        Assert.Equal(1, fooValuesLength);
        Assert.Equal("new-value", fooFirstValue);
    }

    [Fact]
    public void SetShouldAddNewParameterIfNotExists()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.set('foo', 'bar');");

        var size = Evaluate("params.size").AsNumber();
        var value = Evaluate("params.get('foo')").AsString();

        Assert.Equal(1, size);
        Assert.Equal("bar", value);
    }

    [Fact]
    public void SortShouldSortParametersByKey()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.append('z', '1');");
        Execute("params.append('a', '2');");
        Execute("params.append('m', '3');");
        Execute("params.sort();");

        var toString = Evaluate("params.toString()").AsString();
        Assert.Equal("a=2&m=3&z=1", toString);
    }

    [Fact]
    public void ToStringShouldReturnUrlEncodedString()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.append('foo', 'hello world');");
        Execute("params.append('bar', '=&');");

        var toString = Evaluate("params.toString()").AsString();
        Assert.Equal("foo=hello+world&bar=%3d%26", toString);
    }

    [Fact]
    public void ToStringShouldReturnEmptyStringForEmptyParams()
    {
        Execute("const params = new URLSearchParams();");
        var toString = Evaluate("params.toString()").AsString();
        Assert.Equal("", toString);
    }

    [Fact]
    public void ShouldThrowTypeErrorForInsufficientArguments()
    {
        var exception = Assert.Throws<JavaScriptException>(() =>
        {
            Execute(
                """
                const params = new URLSearchParams();
                params.append(); // No arguments provided
                """
            );
        });

        Assert.Contains("TypeError", exception.Error.ToString());
    }
}
