using Jint;
using Jint.Native;
using Xunit;
using Yilduz.Storages.Storage;

namespace Yilduz.Tests.Storages;

public sealed class RuntimeTests : TestBase
{
    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void CanAccessStorage(string storageName)
    {
        Assert.NotNull(Engine.GetValue(storageName));
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void CanGetAndSetItem(string storageName)
    {
        var storage = Engine.GetValue(storageName) as StorageInstance;
        Assert.NotNull(storage);

        Execute($"{storageName}.setItem('testKey', 'testValue')");
        Assert.Equal("testValue", Evaluate($"{storageName}.getItem('testKey')"));

        storage["testKey"] = "newValue";
        Assert.Equal("newValue", Evaluate($"{storageName}.getItem('testKey')"));
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void CanClearStorage(string storageName)
    {
        Execute($"{storageName}.setItem('testKey', 'testValue')");
        Assert.Equal("testValue", Evaluate($"{storageName}.getItem('testKey')"));

        Execute($"{storageName}.clear()");
        Assert.Equal(JsValue.Null, Evaluate($"{storageName}.getItem('testKey')"));
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void CanRemoveItem(string storageName)
    {
        Execute($"{storageName}.setItem('testKey', 'testValue')");
        Assert.Equal("testValue", Evaluate($"{storageName}.getItem('testKey')"));

        Execute($"{storageName}.removeItem('testKey')");
        Assert.Equal(JsValue.Null, Evaluate($"{storageName}.getItem('testKey')"));
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void CanGetStorageLength(string storageName)
    {
        Execute($"{storageName}.setItem('testKey1', 'value1')");
        Execute($"{storageName}.setItem('testKey2', 'value2')");

        Assert.Equal(2, Evaluate($"{storageName}.length"));

        Execute($"{storageName}.removeItem('testKey1')");
        Assert.Equal(1, Evaluate($"{storageName}.length"));

        Execute($"{storageName}.clear()");
        Assert.Equal(0, Evaluate($"{storageName}.length"));
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void CanGetKeyByIndex(string storageName)
    {
        Execute($"{storageName}.setItem('testKey1', 'value1')");
        Execute($"{storageName}.setItem('testKey2', 'value2')");

        Assert.Equal("testKey1", Evaluate($"{storageName}.key(0)"));
        Assert.Equal("testKey2", Evaluate($"{storageName}.key(1)"));

        Assert.Equal(JsValue.Null, Evaluate($"{storageName}.key(2)"));
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void CanGetAndSetValueByKey(string storageName)
    {
        Execute($"{storageName}.setItem('testKey', 'value')");

        Assert.Equal("value", Evaluate($"{storageName}['testKey']"));
        Assert.Equal(JsValue.Null, Evaluate($"{storageName}['non-existing-key']"));

        Execute($"{storageName}['testKey'] = 'newValue'");
        Assert.Equal("newValue", Evaluate($"{storageName}['testKey']"));
        Assert.Equal("newValue", Evaluate($"{storageName}.getItem('testKey')"));
        Assert.True(Evaluate($"{storageName}.hasOwnProperty('testKey')").AsBoolean());

        Execute($"{storageName}[123] = 'value'");
        Assert.Equal("value", Evaluate($"{storageName}['123']"));
        Assert.Equal("value", Evaluate($"{storageName}.getItem('123')"));
        Assert.True(Evaluate($"{storageName}.hasOwnProperty('123')").AsBoolean());

        Execute($"{storageName}[{{}}] = 'value'");
        Assert.Equal("value", Evaluate($"{storageName}['[object Object]']"));
        Assert.Equal("value", Evaluate($"{storageName}.getItem('[object Object]')"));
        Assert.True(Evaluate($"{storageName}.hasOwnProperty('[object Object]')").AsBoolean());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleSpecialCharacters(string storageName)
    {
        Execute($"{storageName}.clear();");
        Execute($"{storageName}.setItem('key with spaces', 'value with spaces');");
        Execute($"{storageName}.setItem('unicode测试', '中文值');");
        Execute($"{storageName}.setItem('symbols!@#$', 'special%^&*');");

        Assert.Equal(
            "value with spaces",
            Evaluate($"{storageName}.getItem('key with spaces')").AsString()
        );
        Assert.Equal("中文值", Evaluate($"{storageName}.getItem('unicode测试')").AsString());
        Assert.Equal("special%^&*", Evaluate($"{storageName}.getItem('symbols!@#$')").AsString());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleNullAndUndefinedValues(string storageName)
    {
        Execute($"{storageName}.clear();");
        Execute($"{storageName}.setItem('nullKey', null);");
        Execute($"{storageName}.setItem('undefinedKey', undefined);");

        Assert.Equal("null", Evaluate($"{storageName}.getItem('nullKey')").AsString());
        Assert.Equal("undefined", Evaluate($"{storageName}.getItem('undefinedKey')").AsString());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldConvertNonStringValues(string storageName)
    {
        Execute($"{storageName}.setItem('number', 123);");
        Execute($"{storageName}.setItem('boolean', true);");
        Execute($"{storageName}.setItem('object', {{toString: () => 'custom'}});");

        Assert.Equal("123", Evaluate($"{storageName}.getItem('number')"));
        Assert.Equal("true", Evaluate($"{storageName}.getItem('boolean')").AsString());
        Assert.Equal("custom", Evaluate($"{storageName}.getItem('object')").AsString());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleEmptyKeys(string storageName)
    {
        Execute($"{storageName}.setItem('', 'empty key value');");

        Assert.Equal("empty key value", Evaluate($"{storageName}.getItem('')").AsString());
        Assert.Equal(1, Evaluate($"{storageName}.length").AsNumber());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldPreserveKeyOrder(string storageName)
    {
        Execute($"{storageName}.clear();");
        Execute($"{storageName}.setItem('z', '1');");
        Execute($"{storageName}.setItem('a', '2');");
        Execute($"{storageName}.setItem('m', '3');");

        Assert.Equal("z", Evaluate($"{storageName}.key(0)").AsString());
        Assert.Equal("a", Evaluate($"{storageName}.key(1)").AsString());
        Assert.Equal("m", Evaluate($"{storageName}.key(2)").AsString());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleSetItemWithSameKey(string storageName)
    {
        Execute($"{storageName}.clear();");
        Execute($"{storageName}.setItem('key', 'value1');");
        Execute($"{storageName}.setItem('key', 'value2');");
        Execute($"{storageName}.setItem('key', 'value3');");

        Assert.Equal(1, Evaluate($"{storageName}.length").AsNumber());
        Assert.Equal("value3", Evaluate($"{storageName}.getItem('key')").AsString());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleEdgeCaseOperations(string storageName)
    {
        Execute($"{storageName}.clear();");

        Execute($"{storageName}.removeItem('nonexistent');");
        Execute($"{storageName}.removeItem('nonexistent');");
        Execute($"{storageName}.removeItem('nonexistent');");

        Assert.Equal(0, Evaluate($"{storageName}.length").AsNumber());

        Execute($"{storageName}.clear();");
        Assert.Equal(0, Evaluate($"{storageName}.length").AsNumber());

        Assert.True(Evaluate($"{storageName}.key(0)").IsNull());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleComplexObjectValues(string storageName)
    {
        Execute($"{storageName}.clear();");
        Execute(
            $$"""
            const obj = { nested: { value: 'test' }, array: [1, 2, 3] };
            {{storageName}}.setItem('object', obj);
            {{storageName}}.setItem('array', [1, 2, 3]);
            {{storageName}}.setItem('function', function() { return 'test'; });
            """
        );

        Assert.Equal("[object Object]", Evaluate($"{storageName}.getItem('object')").AsString());
        Assert.Equal("1,2,3", Evaluate($"{storageName}.getItem('array')").AsString());
        Assert.Contains("function", Evaluate($"{storageName}.getItem('function')").AsString());
    }
}
