using Jint;
using Xunit;

namespace Yilduz.Tests.URLSearchParams;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldHandleSpecialCharacters()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.append('key with spaces', 'value with spaces');");
        Execute("params.append('unicode', '中文测试');");
        Execute("params.append('symbols', '!@#$%^&*()');");

        var spaceValue = Evaluate("params.get('key with spaces')").AsString();
        var unicodeValue = Evaluate("params.get('unicode')").AsString();
        var symbolsValue = Evaluate("params.get('symbols')").AsString();

        Assert.Equal("value with spaces", spaceValue);
        Assert.Equal("中文测试", unicodeValue);
        Assert.Equal("!@#$%^&*()", symbolsValue);
    }

    [Fact]
    public void ShouldHandleEmptyValues()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.append('empty', '');");
        Execute("params.append('null', null);");
        Execute("params.append('undefined', undefined);");

        var emptyValue = Evaluate("params.get('empty')").AsString();
        var nullValue = Evaluate("params.get('null')").AsString();
        var undefinedValue = Evaluate("params.get('undefined')").AsString();

        Assert.Equal("", emptyValue);
        Assert.Equal("null", nullValue);
        Assert.Equal("undefined", undefinedValue);
    }

    [Fact]
    public void ShouldConvertNonStringValues()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.append('number', 123);");
        Execute("params.append('boolean', true);");
        Execute("params.append('object', {toString: () => 'custom'});");

        var numberValue = Evaluate("params.get('number')").AsString();
        var booleanValue = Evaluate("params.get('boolean')").AsString();
        var objectValue = Evaluate("params.get('object')").AsString();

        Assert.Equal("123", numberValue);
        Assert.Equal("true", booleanValue);
        Assert.Equal("custom", objectValue);
    }

    [Fact]
    public void ShouldMaintainInsertionOrder()
    {
        Execute("const params = new URLSearchParams();");
        Execute("params.append('c', '3');");
        Execute("params.append('a', '1');");
        Execute("params.append('b', '2');");

        var toString = Evaluate("params.toString()").AsString();
        Assert.Equal("c=3&a=1&b=2", toString);
    }

    [Fact]
    public void ShouldHandleLargeDataSets()
    {
        Execute(
            """
            const params = new URLSearchParams();
            for (let i = 0; i < 100; i++) {
                params.append('key' + i, 'value' + i);
            }
            """
        );

        var size = Evaluate("params.size").AsNumber();
        var firstValue = Evaluate("params.get('key0')").AsString();
        var lastValue = Evaluate("params.get('key99')").AsString();

        Assert.Equal(100, size);
        Assert.Equal("value0", firstValue);
        Assert.Equal("value99", lastValue);
    }

    [Fact]
    public void ShouldHandleIteratorMethods()
    {
        Execute("const params = new URLSearchParams('a=1&b=2&c=3');");

        var keys = Evaluate("typeof params.keys").AsString();
        var values = Evaluate("typeof params.values").AsString();
        var entries = Evaluate("typeof params.entries").AsString();

        Assert.Equal("function", keys);
        Assert.Equal("function", values);
        Assert.Equal("function", entries);
    }

    [Fact]
    public void ShouldHandleComplexQueryStrings()
    {
        var complexQuery =
            "name=John+Doe&age=30&city=New+York&hobbies=reading&hobbies=swimming&special=%21%40%23";
        Execute($"const params = new URLSearchParams('{complexQuery}');");

        var name = Evaluate("params.get('name')").AsString();
        var age = Evaluate("params.get('age')").AsString();
        var city = Evaluate("params.get('city')").AsString();
        var hobbiesLength = Evaluate("params.getAll('hobbies').length").AsNumber();
        var special = Evaluate("params.get('special')").AsString();

        Assert.Equal("John Doe", name);
        Assert.Equal("30", age);
        Assert.Equal("New York", city);
        Assert.Equal(2, hobbiesLength);
        Assert.Equal("!@#", special);
    }

    [Fact]
    public void ShouldHandleEqualsSignInValue()
    {
        Execute("const params = new URLSearchParams('key=value=with=equals');");
        var valueWithEquals = Evaluate("params.get('key')").AsString();
        Assert.Equal("value=with=equals", valueWithEquals);
    }

    [Fact]
    public void ShouldHandleEmptyKey()
    {
        Execute("const params = new URLSearchParams('=value');");
        var emptyKeyValue = Evaluate("params.get('')").AsString();
        Assert.Equal("value", emptyKeyValue);
    }

    [Fact]
    public void ShouldHandleMultipleConsecutiveAmpersands()
    {
        Execute("const params = new URLSearchParams('a=1&&b=2&&&c=3');");
        var size = Evaluate("params.size").AsNumber();
        Assert.Equal(3, size);
    }
}
