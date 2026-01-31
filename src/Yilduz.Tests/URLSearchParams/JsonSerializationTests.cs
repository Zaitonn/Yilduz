using Jint;
using Xunit;

namespace Yilduz.Tests.URLSearchParams;

public sealed class JsonSerializationTests : TestBase
{
    [Fact]
    public void ShouldSerializeEmptyURLSearchParamsToJSON()
    {
        Execute("const params = new URLSearchParams();");

        var jsonResult = Evaluate("JSON.stringify(params)").AsString();

        Assert.Equal("\"\"", jsonResult);
    }

    [Fact]
    public void ShouldSerializeURLSearchParamsWithSingleParam()
    {
        Execute(
            """
            const params = new URLSearchParams();
            params.append('key', 'value');
            """
        );

        var jsonResult = Evaluate("JSON.stringify(params)").AsString();

        Assert.Equal("\"key=value\"", jsonResult);
    }

    [Fact]
    public void ShouldSerializeURLSearchParamsWithMultipleParams()
    {
        Execute(
            """
            const params = new URLSearchParams();
            params.append('first', 'value1');
            params.append('second', 'value2');
            params.append('third', 'value3');
            """
        );

        var jsonResult = Evaluate("JSON.stringify(params)").AsString();

        Assert.Equal("\"first=value1&second=value2&third=value3\"", jsonResult);
    }

    [Fact]
    public void ShouldCallToJSONMethod()
    {
        Execute(
            """
            const params = new URLSearchParams();
            params.append('test', 'value');
            """
        );

        var toJsonResult = Evaluate("params.toJSON()").AsString();
        var toStringResult = Evaluate("params.toString()").AsString();

        Assert.Equal(toStringResult, toJsonResult);
        Assert.Equal("test=value", toJsonResult);
    }

    [Fact]
    public void ShouldSerializeURLSearchParamsWithSpecialCharacters()
    {
        Execute(
            """
            const params = new URLSearchParams();
            params.append('key with spaces', 'value with spaces');
            params.append('unicode', '中文测试');
            params.append('symbols', '!@#$%^&*()');
            """
        );

        var jsonResult = Evaluate("JSON.stringify(params)").AsString();
        var expectedString = Evaluate("params.toString()").AsString();

        Assert.Equal($"\"{expectedString}\"", jsonResult);
    }

    [Fact]
    public void ShouldSerializeURLSearchParamsWithDuplicateKeys()
    {
        Execute(
            """
            const params = new URLSearchParams();
            params.append('key', 'value1');
            params.append('key', 'value2');
            params.append('key', 'value3');
            """
        );

        var jsonResult = Evaluate("JSON.stringify(params)").AsString();

        Assert.Equal("\"key=value1&key=value2&key=value3\"", jsonResult);
    }

    [Fact]
    public void ShouldSerializeURLSearchParamsInArray()
    {
        Execute(
            """
            const params1 = new URLSearchParams('a=1&b=2');
            const params2 = new URLSearchParams('c=3&d=4');
            const array = [params1, params2];
            """
        );

        var jsonResult = Evaluate("JSON.stringify(array)").AsString();

        Assert.Equal("[\"a=1&b=2\",\"c=3&d=4\"]", jsonResult);
    }

    [Fact]
    public void ShouldSerializeURLSearchParamsInObject()
    {
        Execute(
            """
            const obj = {
                search: new URLSearchParams('query=hello&type=world'),
                filters: new URLSearchParams('category=test')
            };
            """
        );

        var jsonResult = Evaluate("JSON.stringify(obj)").AsString();

        Assert.Equal(
            "{\"search\":\"query=hello&type=world\",\"filters\":\"category=test\"}",
            jsonResult
        );
    }

    [Fact]
    public void ShouldSerializeModifiedURLSearchParams()
    {
        Execute(
            """
            const params = new URLSearchParams('initial=value');
            params.append('added', 'new');
            params.set('initial', 'modified');
            params.delete('removed');
            """
        );

        var jsonResult = Evaluate("JSON.stringify(params)").AsString();

        Assert.Equal("\"initial=modified&added=new\"", jsonResult);
    }

    [Fact]
    public void ShouldSerializeURLSearchParamsWithEmptyValues()
    {
        Execute(
            """
            const params = new URLSearchParams();
            params.append('empty', '');
            params.append('null', null);
            params.append('undefined', undefined);
            """
        );

        var jsonResult = Evaluate("JSON.stringify(params)").AsString();

        Assert.Equal("\"empty=&null=null&undefined=undefined\"", jsonResult);
    }

    [Fact]
    public void ShouldSerializeSortedURLSearchParams()
    {
        Execute(
            """
            const params = new URLSearchParams();
            params.append('z', 'last');
            params.append('a', 'first');
            params.append('m', 'middle');
            params.sort();
            """
        );

        var jsonResult = Evaluate("JSON.stringify(params)").AsString();

        Assert.Equal("\"a=first&m=middle&z=last\"", jsonResult);
    }
}
