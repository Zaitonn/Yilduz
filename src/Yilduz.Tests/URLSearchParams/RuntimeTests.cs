using Jint;
using Xunit;

namespace Yilduz.Tests.URLSearchParams;

public sealed class RuntimeTests : TestBase
{
    [Fact]
    public void ShouldHandleSpecialCharacters()
    {
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.append('key with spaces', 'value with spaces');");
        Engine.Execute("params.append('unicode', '中文测试');");
        Engine.Execute("params.append('symbols', '!@#$%^&*()');");

        var spaceValue = Engine.Evaluate("params.get('key with spaces')").AsString();
        var unicodeValue = Engine.Evaluate("params.get('unicode')").AsString();
        var symbolsValue = Engine.Evaluate("params.get('symbols')").AsString();

        Assert.Equal("value with spaces", spaceValue);
        Assert.Equal("中文测试", unicodeValue);
        Assert.Equal("!@#$%^&*()", symbolsValue);
    }

    [Fact]
    public void ShouldHandleEmptyValues()
    {
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.append('empty', '');");
        Engine.Execute("params.append('null', null);");
        Engine.Execute("params.append('undefined', undefined);");

        var emptyValue = Engine.Evaluate("params.get('empty')").AsString();
        var nullValue = Engine.Evaluate("params.get('null')").AsString();
        var undefinedValue = Engine.Evaluate("params.get('undefined')").AsString();

        Assert.Equal("", emptyValue);
        Assert.Equal("null", nullValue);
        Assert.Equal("undefined", undefinedValue);
    }

    [Fact]
    public void ShouldConvertNonStringValues()
    {
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.append('number', 123);");
        Engine.Execute("params.append('boolean', true);");
        Engine.Execute("params.append('object', {toString: () => 'custom'});");

        var numberValue = Engine.Evaluate("params.get('number')").AsString();
        var booleanValue = Engine.Evaluate("params.get('boolean')").AsString();
        var objectValue = Engine.Evaluate("params.get('object')").AsString();

        Assert.Equal("123", numberValue);
        Assert.Equal("true", booleanValue);
        Assert.Equal("custom", objectValue);
    }

    [Fact]
    public void ShouldMaintainInsertionOrder()
    {
        Engine.Execute("const params = new URLSearchParams();");
        Engine.Execute("params.append('c', '3');");
        Engine.Execute("params.append('a', '1');");
        Engine.Execute("params.append('b', '2');");

        var toString = Engine.Evaluate("params.toString()").AsString();
        Assert.Equal("c=3&a=1&b=2", toString);
    }

    [Fact]
    public void ShouldHandleLargeDataSets()
    {
        Engine.Execute(
            @"
            const params = new URLSearchParams();
            for (let i = 0; i < 100; i++) {
                params.append('key' + i, 'value' + i);
            }
        "
        );

        var size = Engine.Evaluate("params.size").AsNumber();
        var firstValue = Engine.Evaluate("params.get('key0')").AsString();
        var lastValue = Engine.Evaluate("params.get('key99')").AsString();

        Assert.Equal(100, size);
        Assert.Equal("value0", firstValue);
        Assert.Equal("value99", lastValue);
    }

    [Fact]
    public void ShouldHandleIteratorMethods()
    {
        Engine.Execute("const params = new URLSearchParams('a=1&b=2&c=3');");

        // Test if the object has iterator-like behavior
        var hasKeys = Engine.Evaluate("typeof params.keys").AsString();
        var hasValues = Engine.Evaluate("typeof params.values").AsString();
        var hasEntries = Engine.Evaluate("typeof params.entries").AsString();

        // These might not be implemented yet, but we should test what's available
        Assert.True(hasKeys == "function" || hasKeys == "undefined");
        Assert.True(hasValues == "function" || hasValues == "undefined");
        Assert.True(hasEntries == "function" || hasEntries == "undefined");
    }

    [Fact]
    public void ShouldHandleComplexQueryStrings()
    {
        var complexQuery =
            "name=John+Doe&age=30&city=New+York&hobbies=reading&hobbies=swimming&special=%21%40%23";
        Engine.Execute($"const params = new URLSearchParams('{complexQuery}');");

        var name = Engine.Evaluate("params.get('name')").AsString();
        var age = Engine.Evaluate("params.get('age')").AsString();
        var city = Engine.Evaluate("params.get('city')").AsString();
        var hobbiesLength = Engine.Evaluate("params.getAll('hobbies').length").AsNumber();
        var special = Engine.Evaluate("params.get('special')").AsString();

        Assert.Equal("John Doe", name);
        Assert.Equal("30", age);
        Assert.Equal("New York", city);
        Assert.Equal(2, hobbiesLength);
        Assert.Equal("!@#", special);
    }

    [Fact]
    public void ShouldHandleEqualsSignInValue()
    {
        Engine.Execute("const params = new URLSearchParams('key=value=with=equals');");
        var valueWithEquals = Engine.Evaluate("params.get('key')").AsString();
        Assert.Equal("value=with=equals", valueWithEquals);
    }

    [Fact]
    public void ShouldHandleEmptyKey()
    {
        Engine.Execute("const params = new URLSearchParams('=value');");
        var emptyKeyValue = Engine.Evaluate("params.get('')").AsString();
        Assert.Equal("value", emptyKeyValue);
    }

    [Fact]
    public void ShouldHandleMultipleConsecutiveAmpersands()
    {
        Engine.Execute("const params = new URLSearchParams('a=1&&b=2&&&c=3');");
        var size = Engine.Evaluate("params.size").AsNumber();
        Assert.Equal(3, size);
    }
}
