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
        var storage = Engine.GetValue(storageName);
        Assert.NotNull(storage);
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
}
