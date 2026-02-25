using System;
using System.Text;
using Jint;
using Jint.Native.Object;
using Yilduz.URLs.URLSearchParams;

namespace Yilduz.URLs.URL;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/URL
/// </summary>
public sealed class URLInstance : ObjectInstance
{
    private string _search = string.Empty;

    internal URLInstance(Engine engine)
        : base(engine)
    {
        SearchParams = engine.GetWebApiIntrinsics().URLSearchParams.ConstructLinkedInstance(this);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URL/origin
    /// </summary>
    public string Origin { get; private set; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URL/href
    /// </summary>
    public string Href
    {
        get => ToString();
        set => UpdateOnHrefChanged(value);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URL/protocol
    /// </summary>
    public string Protocol { get; set; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URL/host
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URL/hostname
    /// </summary>
    public string Hostname { get; set; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URL/port
    /// </summary>
    public string Port { get; set; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URL/pathname
    /// </summary>
    public string Pathname { get; set; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URL/search
    /// </summary>
    public string Search
    {
        get => _search;
        set
        {
            _search = value;
            SearchParams.UpdateWithNewQuery(_search);
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URL/hash
    /// </summary>
    public string Hash { get; set; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URL/username
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URL/password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URL/searchParams
    /// </summary>
    public URLSearchParamsInstance SearchParams { get; }

    private void UpdateOnHrefChanged(string value)
    {
        var uri = new Uri(value);
        var (username, password) = ParseUserInfo(uri.UserInfo);

        Port = uri.Port.ToString();
        Search = uri.Query;
        Hostname = uri.Host;
        Host = uri.Authority;
        Pathname = string.Join(string.Empty, uri.Segments);
        Protocol = uri.Scheme + ":";
        Hash = uri.Fragment;
        Username = username;
        Password = password;
        Origin = uri.OriginalString;
    }

    private static (string Username, string Password) ParseUserInfo(string userInfo)
    {
        if (string.IsNullOrEmpty(userInfo))
        {
            return (string.Empty, string.Empty);
        }

        var index = userInfo.IndexOf(':');
        if (index < 0)
        {
            return (userInfo, string.Empty);
        }

        return (userInfo[..index], userInfo[(index + 1)..]);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    // https://url.spec.whatwg.org/#concept-url-serializer
    public override string ToString()
    {
        var sb = new StringBuilder();

        sb.Append(Protocol);

        if (!string.IsNullOrEmpty(Host))
        {
            sb.Append("//");

            if (!string.IsNullOrEmpty(Username))
            {
                sb.Append(Username);
                if (!string.IsNullOrEmpty(Password))
                {
                    sb.Append(':');
                    sb.Append(Password);
                }
                sb.Append('@');
            }

            sb.Append(Host);
        }
        else if (string.Equals(Protocol, "file:", StringComparison.OrdinalIgnoreCase))
        {
            sb.Append("//");
        }

        sb.Append(Pathname);

        if (!string.IsNullOrEmpty(Search))
        {
            sb.Append(Search);
        }

        if (!string.IsNullOrEmpty(Hash))
        {
            sb.Append(Hash);
        }

        return sb.ToString();
    }
}
