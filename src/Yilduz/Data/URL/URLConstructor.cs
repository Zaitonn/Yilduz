using System;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Data.URLSearchParams;
using Yilduz.Utils;

namespace Yilduz.Data.URL;

internal sealed partial class URLConstructor : Constructor
{
    private static readonly string CanParseName = nameof(CanParse).ToJsStyleName();
    private static readonly string ParseName = nameof(Parse).ToJsStyleName();
    private readonly URLSearchParamsConstructor _urlSearchParamsConstructor;

    public URLConstructor(Engine engine, URLSearchParamsConstructor urlSearchParamsConstructor)
        : base(engine, nameof(URL))
    {
        _urlSearchParamsConstructor = urlSearchParamsConstructor;
        PrototypeObject = new(engine, this);
        SetOwnProperty("prototype", new(PrototypeObject, false, false, false));

        SetOwnProperty(
            CanParseName,
            new(new ClrFunction(engine, CanParseName, CanParse), true, false, true)
        );
        SetOwnProperty(
            ParseName,
            new(new ClrFunction(engine, ParseName, Parse), true, false, true)
        );
    }

    public URLPrototype PrototypeObject { get; }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        arguments.EnsureCount(Engine, 1, "Failed to construct 'URL'");

        try
        {
            return ParseInternal(
                arguments.At(0).ToString(),
                arguments.Length == 1 ? null : arguments.At(1).ToString()
            );
        }
        catch (Exception e) when (e is not JavaScriptException)
        {
            TypeErrorHelper.Throw(Engine, $"Failed to construct 'URL': {e.Message}");
            return null!;
        }
    }

    private JsValue CanParse(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, CanParseName, nameof(URL));
        try
        {
            ParseInternal(
                arguments.At(0).ToString(),
                arguments.Length == 1 ? null : arguments.At(1).ToString()
            );
            return true;
        }
        catch
        {
            return false;
        }
    }

    private JsValue Parse(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, ParseName, nameof(URL));

        try
        {
            return ParseInternal(
                arguments.At(0).ToString(),
                arguments.Length == 1 ? null : arguments.At(1).ToString()
            );
        }
        catch
        {
            return Null;
        }
    }
}
