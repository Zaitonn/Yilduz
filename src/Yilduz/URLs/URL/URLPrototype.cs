using Jint;
using Jint.Native;
using Yilduz.Models;

namespace Yilduz.URLs.URL;

internal sealed class URLPrototype : PrototypeBase<URLInstance>
{
    public URLPrototype(Engine engine, URLConstructor ctor)
        : base(engine, nameof(URL), ctor)
    {
        RegisterProperty("origin", GetOrigin);
        RegisterProperty("href", GetHref, SetHref);
        RegisterProperty("protocol", GetProtocol, SetProtocol);
        RegisterProperty("host", GetHost, SetHost);
        RegisterProperty("hostname", GetHostname, SetHostname);
        RegisterProperty("port", GetPort, SetPort);
        RegisterProperty("pathname", GetPathname, SetPathname);
        RegisterProperty("search", GetSearch, SetSearch);
        RegisterProperty("hash", GetHash, SetHash);
        RegisterProperty("username", GetUsername, SetUsername);
        RegisterProperty("password", GetPassword, SetPassword);
        RegisterProperty("searchParams", GetSearchParams);

        RegisterMethod("toString", ToString);
        RegisterMethod("toJSON", ToJSON);
    }

    private static JsValue GetOrigin(URLInstance url)
    {
        return url.Origin;
    }

    private static JsValue GetHref(URLInstance url)
    {
        return url.Href;
    }

    private static JsValue SetHref(URLInstance url, JsValue value)
    {
        url.Href = value.ToString();
        return value;
    }

    private static JsValue GetProtocol(URLInstance url)
    {
        return url.Protocol;
    }

    private static JsValue SetProtocol(URLInstance url, JsValue value)
    {
        url.Protocol = value.ToString();
        return value;
    }

    private static JsValue GetHost(URLInstance url)
    {
        return url.Host;
    }

    private static JsValue SetHost(URLInstance url, JsValue value)
    {
        url.Host = value.ToString();
        return value;
    }

    private static JsValue GetHostname(URLInstance url)
    {
        return url.Hostname;
    }

    private static JsValue SetHostname(URLInstance url, JsValue value)
    {
        url.Hostname = value.ToString();
        return value;
    }

    private static JsValue GetPort(URLInstance url)
    {
        return url.Port;
    }

    private static JsValue SetPort(URLInstance url, JsValue value)
    {
        url.Port = value.ToString();
        return value;
    }

    private static JsValue GetPathname(URLInstance url)
    {
        return url.Pathname;
    }

    private static JsValue SetPathname(URLInstance url, JsValue value)
    {
        url.Pathname = value.ToString();
        return value;
    }

    private static JsValue GetSearch(URLInstance url)
    {
        return url.Search;
    }

    private static JsValue SetSearch(URLInstance url, JsValue value)
    {
        url.Search = value.ToString();
        return value;
    }

    private static JsValue GetHash(URLInstance url)
    {
        return url.Hash;
    }

    private static JsValue SetHash(URLInstance url, JsValue value)
    {
        url.Hash = value.ToString();
        return value;
    }

    private static JsValue GetUsername(URLInstance url)
    {
        return url.Username;
    }

    private static JsValue SetUsername(URLInstance url, JsValue value)
    {
        url.Username = value.ToString();
        return value;
    }

    private static JsValue GetPassword(URLInstance url)
    {
        return url.Password;
    }

    private static JsValue SetPassword(URLInstance url, JsValue value)
    {
        url.Password = value.ToString();
        return value;
    }

    private static JsValue GetSearchParams(URLInstance url)
    {
        return url.SearchParams;
    }

    private static JsValue ToString(URLInstance url, JsValue[] _)
    {
        return url.Href;
    }

    private static JsValue ToJSON(URLInstance url, JsValue[] _)
    {
        return url.Href;
    }
}
