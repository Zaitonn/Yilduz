using Jint;
using Xunit;

namespace Yilduz.Tests.URLSearchParams;

public sealed class JsonSerializationTests : TestBase
{
    [Fact]
    public void ShouldSerializeEmptyURLSearchParamsToJSON()
    {
        Engine.Execute("const params = new URLSearchParams();");

        var jsonResult = Engine.Evaluate("JSON.stringify(params)").AsString();

        Assert.Equal("\"\"", jsonResult);
    }

    [Fact]
    public void ShouldSerializeURLSearchParamsWithSingleParam()
    {
        Engine.Execute(
            @"
            const params = new URLSearchParams();
            params.append('key', 'value');
        "
        );

        var jsonResult = Engine.Evaluate("JSON.stringify(params)").AsString();

        Assert.Equal("\"key=value\"", jsonResult);
    }

    [Fact]
    public void ShouldSerializeURLSearchParamsWithMultipleParams()
    {
        Engine.Execute(
            @"
            const params = new URLSearchParams();
            params.append('first', 'value1');
            params.append('second', 'value2');
            params.append('third', 'value3');
        "
        );

        var jsonResult = Engine.Evaluate("JSON.stringify(params)").AsString();

        Assert.Equal("\"first=value1&second=value2&third=value3\"", jsonResult);
    }

    [Fact]
    public void ShouldCallToJSONMethod()
    {
        Engine.Execute(
            @"
            const params = new URLSearchParams();
            params.append('test', 'value');
        "
        );

        var toJsonResult = Engine.Evaluate("params.toJSON()").AsString();
        var toStringResult = Engine.Evaluate("params.toString()").AsString();

        Assert.Equal(toStringResult, toJsonResult);
        Assert.Equal("test=value", toJsonResult);
    }

    [Fact]
    public void ShouldSerializeURLSearchParamsWithSpecialCharacters()
    {
        Engine.Execute(
            @"
            const params = new URLSearchParams();
            params.append('key with spaces', 'value with spaces');
            params.append('unicode', '中文测试');
            params.append('symbols', '!@#$%^&*()');
        "
        );

        var jsonResult = Engine.Evaluate("JSON.stringify(params)").AsString();
        var expectedString = Engine.Evaluate("params.toString()").AsString();

        Assert.Equal($"\"{expectedString}\"", jsonResult);
    }

    [Fact]
    public void ShouldSerializeURLSearchParamsWithDuplicateKeys()
    {
        Engine.Execute(
            @"
            const params = new URLSearchParams();
            params.append('key', 'value1');
            params.append('key', 'value2');
            params.append('key', 'value3');
        "
        );

        var jsonResult = Engine.Evaluate("JSON.stringify(params)").AsString();

        Assert.Equal("\"key=value1&key=value2&key=value3\"", jsonResult);
    }

    [Fact]
    public void ShouldSerializeURLSearchParamsInArray()
    {
        Engine.Execute(
            @"
            const params1 = new URLSearchParams('a=1&b=2');
            const params2 = new URLSearchParams('c=3&d=4');
            const array = [params1, params2];
        "
        );

        var jsonResult = Engine.Evaluate("JSON.stringify(array)").AsString();

        Assert.Equal("[\"a=1&b=2\",\"c=3&d=4\"]", jsonResult);
    }

    [Fact]
    public void ShouldSerializeURLSearchParamsInObject()
    {
        Engine.Execute(
            @"
            const obj = {
                search: new URLSearchParams('query=hello&type=world'),
                filters: new URLSearchParams('category=test')
            };
        "
        );

        var jsonResult = Engine.Evaluate("JSON.stringify(obj)").AsString();

        Assert.Equal(
            "{\"search\":\"query=hello&type=world\",\"filters\":\"category=test\"}",
            jsonResult
        );
    }

    [Fact]
    public void ShouldSerializeModifiedURLSearchParams()
    {
        Engine.Execute(
            @"
            const params = new URLSearchParams('initial=value');
            params.append('added', 'new');
            params.set('initial', 'modified');
            params.delete('removed');
        "
        );

        var jsonResult = Engine.Evaluate("JSON.stringify(params)").AsString();

        Assert.Equal("\"initial=modified&added=new\"", jsonResult);
    }

    [Fact]
    public void ShouldSerializeURLSearchParamsWithEmptyValues()
    {
        Engine.Execute(
            @"
            const params = new URLSearchParams();
            params.append('empty', '');
            params.append('null', null);
            params.append('undefined', undefined);
        "
        );

        var jsonResult = Engine.Evaluate("JSON.stringify(params)").AsString();

        Assert.Equal("\"empty=&null=null&undefined=undefined\"", jsonResult);
    }

    [Fact]
    public void ShouldSerializeSortedURLSearchParams()
    {
        Engine.Execute(
            @"
            const params = new URLSearchParams();
            params.append('z', 'last');
            params.append('a', 'first');
            params.append('m', 'middle');
            params.sort();
        "
        );

        var jsonResult = Engine.Evaluate("JSON.stringify(params)").AsString();

        Assert.Equal("\"a=first&m=middle&z=last\"", jsonResult);
    }
}
