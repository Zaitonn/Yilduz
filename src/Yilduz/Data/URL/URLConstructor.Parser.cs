using System;
using Yilduz.Data.URLSearchParams;

namespace Yilduz.Data.URL;

internal sealed partial class URLConstructor
{
    private URLInstance ParseInternal(string url, string? baseUrl)
    {
        var uri = new Uri(url, UriKind.RelativeOrAbsolute);

        if (!uri.IsAbsoluteUri)
        {
            uri = baseUrl is not null
                ? new(new(baseUrl, UriKind.RelativeOrAbsolute), uri)
                : throw new ArgumentException("Invalid base URL");
        }

        var urlInstance = new URLInstance(Engine, _urlSearchParamsConstructor)
        {
            Prototype = PrototypeObject,
            Href = uri.AbsoluteUri,
        };

        return urlInstance;
    }
}
