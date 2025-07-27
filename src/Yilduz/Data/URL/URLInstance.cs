using System;
using Jint;
using Jint.Native.Object;
using Yilduz.Data.URLSearchParams;

namespace Yilduz.Data.URL;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/URL
/// </summary>
public sealed class URLInstance : ObjectInstance
{
    private string _pathname = string.Empty;
    private string _search = string.Empty;

    internal URLInstance(Engine engine, URLSearchParamsConstructor constructor)
        : base(engine)
    {
        SearchParams = constructor.ConstructLinkedInstance(this);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URL/origin
    /// </summary>
    public string Origin => $"{Protocol}//{Host}";

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
    public string Pathname
    {
        get => _pathname.StartsWith("/") ? _pathname : '/' + _pathname;
        set => _pathname = value.StartsWith("/") ? value : '/' + value;
    }

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
        Pathname = uri.AbsolutePath;
        Protocol = uri.Scheme + ":";
        Hash = uri.Fragment;
        Username = username;
        Password = password;
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
    public override string ToString()
    {
        return string.IsNullOrEmpty(Username) && string.IsNullOrEmpty(Password)
            ? $"{Protocol}//{Host}{Pathname}{Search}{Hash}"
            : $"{Protocol}//{Username}:{Password}@{Host}{Pathname}{Search}{Hash}";
    }
}
