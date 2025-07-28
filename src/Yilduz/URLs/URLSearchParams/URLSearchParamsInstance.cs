using System.Collections.Generic;
using System.Text;
using System.Web;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Yilduz.Extensions;
using Yilduz.URLs.URL;
using Yilduz.Utils;

namespace Yilduz.URLs.URLSearchParams;

public sealed class URLSearchParamsInstance : ObjectInstance
{
    private readonly List<KeyValuePair<string, string>> _list;

    private readonly URLInstance? _urlInstance;
    private bool _shouldUpdateUrlInstance;

    public IReadOnlyList<KeyValuePair<string, string>> QueryList => _list;

    private URLSearchParamsInstance(Engine engine)
        : base(engine)
    {
        _list = [];
    }

    internal URLSearchParamsInstance(Engine engine, URLInstance urlInstance)
        : this(engine)
    {
        _urlInstance = urlInstance;
    }

    internal URLSearchParamsInstance(Engine engine, JsValue init)
        : this(engine)
    {
        if (init.IsObject())
        {
            var obj = init.AsObject();

            if (obj is URLSearchParamsInstance existingInstance)
            {
                _list.AddRange(existingInstance.QueryList);
                _shouldUpdateUrlInstance = true;
                return;
            }

            var iterator = obj.Get(GlobalSymbolRegistry.Iterator, obj);
            if (!iterator.IsUndefined())
            {
                foreach (var key in obj)
                {
                    ProcessSequenceInit(key);
                }
            }
            else
            {
                foreach (var key in obj.GetOwnPropertyKeys())
                {
                    Append(key.ToString(), obj.Get(key).ToString());
                }
            }
        }
        else if (init.IsString())
        {
            ParseStringInit(init.AsString());
        }

        _shouldUpdateUrlInstance = true;
    }

    private void ProcessSequenceInit(JsValue value)
    {
        if (!value.IsObject())
        {
            TypeErrorHelper.Throw(
                Engine,
                "Iterator value must be a sequence",
                "constructor",
                nameof(URLSearchParams)
            );
        }

        var innerSequence = value.AsObject();

        // Check if the inner sequence has a length property
        var lengthValue = innerSequence.Get("length");
        if (!lengthValue.IsNumber())
        {
            TypeErrorHelper.Throw(
                Engine,
                "Iterator value must be a sequence with length",
                "constructor",
                nameof(URLSearchParams)
            );
        }

        var length = (int)lengthValue.AsNumber();
        if (length != 2)
        {
            TypeErrorHelper.Throw(
                Engine,
                "Iterator value must be a sequence of length 2",
                "constructor",
                nameof(URLSearchParams)
            );
        }

        var name = innerSequence.Get("0").ToString();
        var sequenceValue = innerSequence.Get("1").ToString();

        Append(name, sequenceValue);
    }

    private void ParseStringInit(string query)
    {
        if (query.StartsWith("?"))
        {
            query = query[1..];
        }

        if (string.IsNullOrEmpty(query))
        {
            return;
        }

        var pairs = query.Split('&');
        foreach (var pair in pairs)
        {
            if (string.IsNullOrEmpty(pair))
            {
                continue;
            }

            var equalIndex = pair.IndexOf('=');
            if (equalIndex >= 0)
            {
                var name = HttpUtility.UrlDecode(pair[..equalIndex]);
                var value = HttpUtility.UrlDecode(pair[(equalIndex + 1)..]);
                Append(name, value);
            }
            else
            {
                var name = HttpUtility.UrlDecode(pair);
                Append(name, string.Empty);
            }
        }
    }

    internal void UpdateWithNewQuery(string query)
    {
        _shouldUpdateUrlInstance = false;

        _list.Clear();
        ParseStringInit(query);

        _shouldUpdateUrlInstance = true;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URLSearchParams/size
    /// </summary>
    public int Size => _list.Count;

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URLSearchParams/append
    /// </summary>
    public void Append(string name, string value)
    {
        _list.Add(new KeyValuePair<string, string>(name, value));
        Update();
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URLSearchParams/get
    /// </summary>
    public string? Get(string name)
    {
        foreach (var pair in _list)
        {
            if (pair.Key == name)
            {
                return pair.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URLSearchParams/getAll
    /// </summary>
    public string[] GetAll(string name)
    {
        var values = new List<string>();
        foreach (var pair in _list)
        {
            if (pair.Key == name)
            {
                values.Add(pair.Value);
            }
        }

        return [.. values];
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URLSearchParams/delete
    /// </summary>
    public void Delete(string name)
    {
        _list.RemoveAll(pair => pair.Key == name);
        Update();
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URLSearchParams/has
    /// </summary>
    public bool Has(string name)
    {
        foreach (var pair in _list)
        {
            if (pair.Key == name)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URLSearchParams/has
    /// </summary>
    public bool Has(string name, string value)
    {
        foreach (var pair in _list)
        {
            if (pair.Key == name && pair.Value == value)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URLSearchParams/set
    /// </summary>
    public void Set(string name, string value)
    {
        var found = false;
        for (int i = _list.Count - 1; i >= 0; i--)
        {
            if (_list[i].Key == name)
            {
                if (!found)
                {
                    _list[i] = new KeyValuePair<string, string>(name, value);
                    found = true;
                }
                else
                {
                    _list.RemoveAt(i);
                }
            }
        }

        if (!found)
        {
            Append(name, value);
        }

        Update();
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/URLSearchParams/sort
    /// </summary>
    public void Sort()
    {
        _list.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
        Update();
    }

    private void Update()
    {
        if (_urlInstance is null || !_shouldUpdateUrlInstance)
        {
            return;
        }

        var serializedQuery = ToString();
        _urlInstance.Search = string.IsNullOrEmpty(serializedQuery)
            ? string.Empty
            : "?" + serializedQuery;
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override string ToString()
    {
        var output = new StringBuilder();

        foreach (var pair in _list)
        {
            var name = HttpUtility.UrlEncode(pair.Key);
            var value = HttpUtility.UrlEncode(pair.Value);

            if (output.Length > 0)
            {
                output.Append('&');
            }

            output.Append(name).Append('=').Append(value);
        }

        return output.ToString();
    }
}
