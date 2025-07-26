using Jint;
using Jint.Runtime;
using Xunit;

namespace Yilduz.Tests.URLSearchParams;

public sealed class MethodTests : TestBase
{
    [Fact]
    public void AppendShouldAddParameter()
    {
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.append('foo', 'bar');");

        var size = Engine.Evaluate("params.size").AsNumber();
        var value = Engine.Evaluate("params.get('foo')").AsString();

        Assert.Equal(1, size);
        Assert.Equal("bar", value);
    }

    [Fact]
    public void AppendShouldAllowDuplicateKeys()
    {
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.append('foo', 'bar');");
        Engine.Execute("params.append('foo', 'baz');");

        var size = Engine.Evaluate("params.size").AsNumber();
        var valuesLength = Engine.Evaluate("params.getAll('foo').length").AsNumber();
        var firstValue = Engine.Evaluate("params.getAll('foo')[0]").AsString();
        var secondValue = Engine.Evaluate("params.getAll('foo')[1]").AsString();

        Assert.Equal(2, size);
        Assert.Equal(2, valuesLength);
        Assert.Equal("bar", firstValue);
        Assert.Equal("baz", secondValue);
    }

    [Fact]
    public void GetShouldReturnFirstValue()
    {
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.append('foo', 'bar');");
        Engine.Execute("params.append('foo', 'baz');");

        var value = Engine.Evaluate("params.get('foo')").AsString();
        Assert.Equal("bar", value);
    }

    [Fact]
    public void GetShouldReturnNullForNonExistentKey()
    {
        Engine.Execute("const params = new URLSearchParams();");
        var value = Engine.Evaluate("params.get('nonexistent')");
        Assert.True(value.IsNull());
    }

    [Fact]
    public void GetAllShouldReturnAllValues()
    {
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.append('foo', 'bar');");
        Engine.Execute("params.append('foo', 'baz');");
        Engine.Execute("params.append('qux', 'quux');");

        var fooValuesLength = Engine.Evaluate("params.getAll('foo').length").AsNumber();
        var fooFirstValue = Engine.Evaluate("params.getAll('foo')[0]").AsString();
        var fooSecondValue = Engine.Evaluate("params.getAll('foo')[1]").AsString();
        var quxValuesLength = Engine.Evaluate("params.getAll('qux').length").AsNumber();
        var quxFirstValue = Engine.Evaluate("params.getAll('qux')[0]").AsString();
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
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.append('foo', 'bar');");
        Engine.Execute("params.append('foo', 'baz');");
        Engine.Execute("params.append('qux', 'quux');");
        Engine.Execute("params.delete('foo');");

        var size = Engine.Evaluate("params.size").AsNumber();
        var hasFoo = Engine.Evaluate("params.has('foo')").AsBoolean();
        var hasQux = Engine.Evaluate("params.has('qux')").AsBoolean();

        Assert.Equal(1, size);
        Assert.False(hasFoo);
        Assert.True(hasQux);
    }

    [Fact]
    public void HasShouldReturnTrueForExistingKey()
    {
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.append('foo', 'bar');");

        var hasFoo = Engine.Evaluate("params.has('foo')").AsBoolean();
        var hasBar = Engine.Evaluate("params.has('bar')").AsBoolean();

        Assert.True(hasFoo);
        Assert.False(hasBar);
    }

    [Fact]
    public void HasWithValueShouldReturnTrueForExactMatch()
    {
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.append('foo', 'bar');");
        Engine.Execute("params.append('foo', 'baz');");

        var hasExactMatch = Engine.Evaluate("params.has('foo', 'bar')").AsBoolean();
        var hasNoMatch = Engine.Evaluate("params.has('foo', 'qux')").AsBoolean();

        Assert.True(hasExactMatch);
        Assert.False(hasNoMatch);
    }

    [Fact]
    public void SetShouldReplaceAllOccurrences()
    {
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.append('foo', 'bar');");
        Engine.Execute("params.append('foo', 'baz');");
        Engine.Execute("params.append('qux', 'quux');");
        Engine.Execute("params.set('foo', 'new-value');");

        var size = Engine.Evaluate("params.size").AsNumber();
        var fooValue = Engine.Evaluate("params.get('foo')").AsString();
        var fooValuesLength = Engine.Evaluate("params.getAll('foo').length").AsNumber();
        var fooFirstValue = Engine.Evaluate("params.getAll('foo')[0]").AsString();

        Assert.Equal(2, size);
        Assert.Equal("new-value", fooValue);
        Assert.Equal(1, fooValuesLength);
        Assert.Equal("new-value", fooFirstValue);
    }

    [Fact]
    public void SetShouldAddNewParameterIfNotExists()
    {
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.set('foo', 'bar');");

        var size = Engine.Evaluate("params.size").AsNumber();
        var value = Engine.Evaluate("params.get('foo')").AsString();

        Assert.Equal(1, size);
        Assert.Equal("bar", value);
    }

    [Fact]
    public void SortShouldSortParametersByKey()
    {
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.append('z', '1');");
        Engine.Execute("params.append('a', '2');");
        Engine.Execute("params.append('m', '3');");
        Engine.Execute("params.sort();");

        var toString = Engine.Evaluate("params.toString()").AsString();
        Assert.Equal("a=2&m=3&z=1", toString);
    }

    [Fact]
    public void ToStringShouldReturnUrlEncodedString()
    {
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.append('foo', 'hello world');");
        Engine.Execute("params.append('bar', '=&');");

        var toString = Engine.Evaluate("params.toString()").AsString();
        Assert.Equal("foo=hello+world&bar=%3d%26", toString);
    }

    [Fact]
    public void ToStringShouldReturnEmptyStringForEmptyParams()
    {
        Engine.Execute("const params = new URLSearchParams();");
        var toString = Engine.Evaluate("params.toString()").AsString();
        Assert.Equal("", toString);
    }

    [Fact]
    public void ShouldThrowTypeErrorForInsufficientArguments()
    {
        var exception = Assert.Throws<JavaScriptException>(() =>
        {
            Engine.Execute(
                @"
                const params = new URLSearchParams();
                params.append(); // No arguments provided
            "
            );
        });

        Assert.Contains("TypeError", exception.Error.ToString());
    }
}
