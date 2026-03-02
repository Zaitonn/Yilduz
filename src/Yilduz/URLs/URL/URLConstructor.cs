using System;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Yilduz.Extensions;
using Yilduz.Utils;

namespace Yilduz.URLs.URL;

internal sealed partial class URLConstructor : Constructor
{
    private static readonly string CanParseName = nameof(CanParse).ToJsStyleName();
    private static readonly string ParseName = nameof(Parse).ToJsStyleName();

    public URLConstructor(Engine engine)
        : base(engine, nameof(URL))
    {
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
            return Parse(
                arguments.At(0).ToString(),
                arguments.Length == 1 ? null : arguments.At(1).ToString()
            );
        }
        catch (Exception e) when (e is not JavaScriptException)
        {
            TypeErrorHelper.Throw(Engine, $"Failed to construct 'URL': {e.Message}");
            return null;
        }
    }

    private JsValue CanParse(JsValue thisObject, JsValue[] arguments)
    {
        arguments.EnsureCount(Engine, 1, CanParseName, nameof(URL));
        try
        {
            Parse(
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
            return Parse(
                arguments.At(0).ToString(),
                arguments.Length == 1 ? null : arguments.At(1).ToString()
            );
        }
        catch
        {
            return Null;
        }
    }

    internal URLInstance? TryParse(string url, Uri? baseUrl)
    {
        try
        {
            var uri = new Uri(url, UriKind.RelativeOrAbsolute);

            if (!uri.IsAbsoluteUri)
            {
                if (baseUrl is not null)
                {
                    uri = new(baseUrl, uri);
                }
                else
                {
                    return null;
                }
            }

            var urlInstance = new URLInstance(Engine)
            {
                Prototype = PrototypeObject,
                Href = uri.AbsoluteUri,
            };

            return urlInstance;
        }
        catch
        {
            return null;
        }
    }

    internal URLInstance Parse(string url, string? baseUrl = null)
    {
        var uri = new Uri(url, UriKind.RelativeOrAbsolute);

        if (!uri.IsAbsoluteUri)
        {
            if (baseUrl is not null)
            {
                uri = new(new(baseUrl, UriKind.RelativeOrAbsolute), uri);
            }
            else
            {
                TypeErrorHelper.Throw(
                    Engine,
                    $"Failed to parse URL: '{url}' is not an absolute URL and no base URL was provided."
                );
            }
        }

        var urlInstance = new URLInstance(Engine)
        {
            Prototype = PrototypeObject,
            Href = uri.AbsoluteUri,
        };

        return urlInstance;
    }
}
