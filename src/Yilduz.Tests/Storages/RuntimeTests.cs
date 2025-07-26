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

        Engine.Execute($"{storageName}.setItem('testKey', 'testValue')");
        Assert.Equal("testValue", Engine.Evaluate($"{storageName}.getItem('testKey')"));

        storage["testKey"] = "newValue";
        Assert.Equal("newValue", Engine.Evaluate($"{storageName}.getItem('testKey')"));
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void CanClearStorage(string storageName)
    {
        Engine.Execute($"{storageName}.setItem('testKey', 'testValue')");
        Assert.Equal("testValue", Engine.Evaluate($"{storageName}.getItem('testKey')"));

        Engine.Execute($"{storageName}.clear()");
        Assert.Equal(JsValue.Null, Engine.Evaluate($"{storageName}.getItem('testKey')"));
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void CanRemoveItem(string storageName)
    {
        Engine.Execute($"{storageName}.setItem('testKey', 'testValue')");
        Assert.Equal("testValue", Engine.Evaluate($"{storageName}.getItem('testKey')"));

        Engine.Execute($"{storageName}.removeItem('testKey')");
        Assert.Equal(JsValue.Null, Engine.Evaluate($"{storageName}.getItem('testKey')"));
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void CanGetStorageLength(string storageName)
    {
        Engine.Execute($"{storageName}.setItem('testKey1', 'value1')");
        Engine.Execute($"{storageName}.setItem('testKey2', 'value2')");

        Assert.Equal(2, Engine.Evaluate($"{storageName}.length"));

        Engine.Execute($"{storageName}.removeItem('testKey1')");
        Assert.Equal(1, Engine.Evaluate($"{storageName}.length"));

        Engine.Execute($"{storageName}.clear()");
        Assert.Equal(0, Engine.Evaluate($"{storageName}.length"));
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void CanGetKeyByIndex(string storageName)
    {
        Engine.Execute($"{storageName}.setItem('testKey1', 'value1')");
        Engine.Execute($"{storageName}.setItem('testKey2', 'value2')");

        Assert.Equal("testKey1", Engine.Evaluate($"{storageName}.key(0)"));
        Assert.Equal("testKey2", Engine.Evaluate($"{storageName}.key(1)"));

        Assert.Equal(JsValue.Null, Engine.Evaluate($"{storageName}.key(2)"));
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void CanGetAndSetValueByKey(string storageName)
    {
        Engine.Execute($"{storageName}.setItem('testKey', 'value')");

        Assert.Equal("value", Engine.Evaluate($"{storageName}['testKey']"));
        Assert.Equal(JsValue.Null, Engine.Evaluate($"{storageName}['non-existing-key']"));

        Engine.Execute($"{storageName}['testKey'] = 'newValue'");
        Assert.Equal("newValue", Engine.Evaluate($"{storageName}['testKey']"));
        Assert.Equal("newValue", Engine.Evaluate($"{storageName}.getItem('testKey')"));
        Assert.True(Engine.Evaluate($"{storageName}.hasOwnProperty('testKey')").AsBoolean());

        Engine.Execute($"{storageName}[123] = 'value'");
        Assert.Equal("value", Engine.Evaluate($"{storageName}['123']"));
        Assert.Equal("value", Engine.Evaluate($"{storageName}.getItem('123')"));
        Assert.True(Engine.Evaluate($"{storageName}.hasOwnProperty('123')").AsBoolean());

        Engine.Execute($"{storageName}[{{}}] = 'value'");
        Assert.Equal("value", Engine.Evaluate($"{storageName}['[object Object]']"));
        Assert.Equal("value", Engine.Evaluate($"{storageName}.getItem('[object Object]')"));
        Assert.True(
            Engine.Evaluate($"{storageName}.hasOwnProperty('[object Object]')").AsBoolean()
        );
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleSpecialCharacters(string storageName)
    {
        Engine.Execute($"{storageName}.clear();");
        Engine.Execute($"{storageName}.setItem('key with spaces', 'value with spaces');");
        Engine.Execute($"{storageName}.setItem('unicode测试', '中文值');");
        Engine.Execute($"{storageName}.setItem('symbols!@#$', 'special%^&*');");

        Assert.Equal(
            "value with spaces",
            Engine.Evaluate($"{storageName}.getItem('key with spaces')").AsString()
        );
        Assert.Equal("中文值", Engine.Evaluate($"{storageName}.getItem('unicode测试')").AsString());
        Assert.Equal(
            "special%^&*",
            Engine.Evaluate($"{storageName}.getItem('symbols!@#$')").AsString()
        );
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleNullAndUndefinedValues(string storageName)
    {
        Engine.Execute($"{storageName}.clear();");
        Engine.Execute($"{storageName}.setItem('nullKey', null);");
        Engine.Execute($"{storageName}.setItem('undefinedKey', undefined);");

        Assert.Equal("null", Engine.Evaluate($"{storageName}.getItem('nullKey')").AsString());
        Assert.Equal(
            "undefined",
            Engine.Evaluate($"{storageName}.getItem('undefinedKey')").AsString()
        );
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldConvertNonStringValues(string storageName)
    {
        Engine.Execute($"{storageName}.setItem('number', 123);");
        Engine.Execute($"{storageName}.setItem('boolean', true);");
        Engine.Execute($"{storageName}.setItem('object', {{toString: () => 'custom'}});");

        Assert.Equal("123", Engine.Evaluate($"{storageName}.getItem('number')"));
        Assert.Equal("true", Engine.Evaluate($"{storageName}.getItem('boolean')").AsString());
        Assert.Equal("custom", Engine.Evaluate($"{storageName}.getItem('object')").AsString());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleEmptyKeys(string storageName)
    {
        Engine.Execute($"{storageName}.setItem('', 'empty key value');");

        Assert.Equal("empty key value", Engine.Evaluate($"{storageName}.getItem('')").AsString());
        Assert.Equal(1, Engine.Evaluate($"{storageName}.length").AsNumber());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldPreserveKeyOrder(string storageName)
    {
        Engine.Execute($"{storageName}.clear();");
        Engine.Execute($"{storageName}.setItem('z', '1');");
        Engine.Execute($"{storageName}.setItem('a', '2');");
        Engine.Execute($"{storageName}.setItem('m', '3');");

        Assert.Equal("z", Engine.Evaluate($"{storageName}.key(0)").AsString());
        Assert.Equal("a", Engine.Evaluate($"{storageName}.key(1)").AsString());
        Assert.Equal("m", Engine.Evaluate($"{storageName}.key(2)").AsString());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleSetItemWithSameKey(string storageName)
    {
        Engine.Execute($"{storageName}.clear();");
        Engine.Execute($"{storageName}.setItem('key', 'value1');");
        Engine.Execute($"{storageName}.setItem('key', 'value2');");
        Engine.Execute($"{storageName}.setItem('key', 'value3');");

        Assert.Equal(1, Engine.Evaluate($"{storageName}.length").AsNumber());
        Assert.Equal("value3", Engine.Evaluate($"{storageName}.getItem('key')").AsString());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleEdgeCaseOperations(string storageName)
    {
        Engine.Execute($"{storageName}.clear();");

        Engine.Execute($"{storageName}.removeItem('nonexistent');");
        Engine.Execute($"{storageName}.removeItem('nonexistent');");
        Engine.Execute($"{storageName}.removeItem('nonexistent');");

        Assert.Equal(0, Engine.Evaluate($"{storageName}.length").AsNumber());

        Engine.Execute($"{storageName}.clear();");
        Assert.Equal(0, Engine.Evaluate($"{storageName}.length").AsNumber());

        Assert.True(Engine.Evaluate($"{storageName}.key(0)").IsNull());
    }

    [Theory]
    [InlineData("localStorage")]
    [InlineData("sessionStorage")]
    public void ShouldHandleComplexObjectValues(string storageName)
    {
        Engine.Execute($"{storageName}.clear();");
        Engine.Execute(
            $@"
            const obj = {{ nested: {{ value: 'test' }}, array: [1, 2, 3] }};
            {storageName}.setItem('object', obj);
            {storageName}.setItem('array', [1, 2, 3]);
            {storageName}.setItem('function', function() {{ return 'test'; }});
        "
        );

        Assert.Equal(
            "[object Object]",
            Engine.Evaluate($"{storageName}.getItem('object')").AsString()
        );
        Assert.Equal("1,2,3", Engine.Evaluate($"{storageName}.getItem('array')").AsString());
        Assert.Contains(
            "function",
            Engine.Evaluate($"{storageName}.getItem('function')").AsString()
        );
    }
}
