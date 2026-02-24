using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Yilduz.Extensions;
using Yilduz.Utils;

namespace Yilduz.Network.Headers;

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/Headers
/// </summary>
public sealed partial class HeadersInstance : ObjectInstance
{
    private readonly HeaderList _headers;
    internal Guard Guard { get; set; }

    internal HeadersInstance(Engine engine, HeaderList list, Guard guard)
        : base(engine)
    {
        _headers = list;
        Guard = guard;
    }

    internal HeadersInstance(Engine engine, JsValue init, Guard guard)
        : base(engine)
    {
        _headers = [];
        Guard = guard;

        if (!init.IsUndefined() && !init.IsNull())
        {
            Fill(init);
        }
    }

    internal IReadOnlyList<HeaderEntry> HeaderList => _headers;

    internal void Clear()
    {
        _headers.Clear();
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Headers/append
    /// </summary>
    public void Append(string name, string value)
    {
        var normalizedValue = Normalize(value);

        if (!Validate(name, normalizedValue, nameof(Append)))
        {
            return;
        }

        if (Guard == Guard.RequestNoCors)
        {
            // Let temporaryValue be the result of getting name from headers’s header list.
            var temporaryValue = Get(name);

            // If temporaryValue is null, then set temporaryValue to value.
            temporaryValue =
                temporaryValue == null
                    ? normalizedValue
                    // Otherwise, set temporaryValue to temporaryValue, followed by 0x2C 0x20, followed by value.
                    : $"{temporaryValue}, {normalizedValue}";

            // If (name, temporaryValue) is not a no-CORS-safelisted request-header, then return.
            if (!HttpHelper.IsNoCORSUnsafeRequestHeader(name, temporaryValue))
            {
                return;
            }
        }

        _headers.Add(new(name, normalizedValue));

        if (Guard == Guard.RequestNoCors)
        {
            RemovePrivilegedNoCORSRequestHeaders();
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Headers/delete
    /// </summary>
    public void Delete(string name)
    {
        // If validating (name, ``) for this returns false, then return.
        if (!Validate(name, string.Empty, nameof(Delete)))
        {
            return;
        }

        // If this’s guard is "request-no-cors", name is not a no-CORS-safelisted request-header name, and name is not a privileged no-CORS request-header name, then return.
        if (
            Guard == Guard.RequestNoCors
            && !HttpHelper.IsNoCORSUnsafeRequestHeaderName(name)
            && !HttpHelper.IsPrivilegedNoCORSRequestHeaderName(name)
        )
        {
            return;
        }

        // If this’s header list does not contain name, then return
        // Delete name from this’s header list.
        _headers.RemoveAll(header => header.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        // If this’s guard is "request-no-cors", then remove privileged no-CORS request-headers from this.
        if (Guard == Guard.RequestNoCors)
        {
            RemovePrivilegedNoCORSRequestHeaders();
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Headers/get
    /// </summary>
    public string? Get(string name)
    {
        if (!HttpHelper.IsHeaderName(name))
        {
            TypeErrorHelper.Throw(Engine, "Invalid header name", nameof(Get), nameof(Headers));
        }

        return GetInternal(name);
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-header-list-get
    /// </summary>
    private string? GetInternal(string name)
    {
        var values = _headers
            .Where(header => header.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            .Select(header => header.Value)
            .ToArray();

        return values.Length == 0 ? null : string.Join(", ", values);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Headers/getSetCookie
    /// </summary>
    public string[] GetSetCookie()
    {
        return
        [
            .. _headers
                .Where(header =>
                    header.Name.Equals("set-cookie", StringComparison.OrdinalIgnoreCase)
                )
                .Select(header => header.Value),
        ];
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Headers/has
    /// </summary>
    public bool Has(string name)
    {
        if (!HttpHelper.IsHeaderName(name))
        {
            TypeErrorHelper.Throw(Engine, "Invalid header name", nameof(Has), nameof(Headers));
        }

        return _headers.Any(header => header.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Headers/set
    /// </summary>
    public void Set(string name, string value)
    {
        var normalizedValue = Normalize(value);

        if (!Validate(name, normalizedValue, nameof(Set)))
        {
            return;
        }

        if (
            Guard == Guard.RequestNoCors
            && !HttpHelper.IsNoCORSUnsafeRequestHeader(name, normalizedValue)
        )
        {
            return;
        }

        SetInternal(name, normalizedValue);

        if (Guard == Guard.RequestNoCors)
        {
            RemovePrivilegedNoCORSRequestHeaders();
        }
    }

    private void SetInternal(string name, string value)
    {
        if (!_headers.Any(header => header.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            _headers.Add(new(name, value));
        }
        else
        {
            var found = false;
            for (int i = 0; i < _headers.Count; i++)
            {
                if (_headers[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    if (!found)
                    {
                        _headers[i] = new(name, value);
                        found = true;
                    }
                    else
                    {
                        _headers.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
    }

    internal List<(string Name, string Value)> GetSortedAndCombinedEntries()
    {
        var combined = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var (name, value) in _headers)
        {
            var normalizedName = name.ToLowerInvariant();

            if (!combined.TryGetValue(normalizedName, out var values))
            {
                values = [];
                combined[normalizedName] = values;
            }

            values.Add(value);
        }

        return
        [
            .. combined
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => (pair.Key, string.Join(", ", pair.Value))),
        ];
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-headers-fill
    /// </summary>
    internal void Fill(JsValue init)
    {
        if (init.IsObject())
        {
            var obj = init.AsObject();

            if (obj is HeadersInstance headersInstance)
            {
                foreach (var (name, value) in headersInstance.HeaderList)
                {
                    Append(name, value);
                }

                return;
            }

            var iterator = obj.Get(GlobalSymbolRegistry.Iterator, obj);
            if (!iterator.IsUndefined())
            {
                foreach (var entry in obj)
                {
                    ProcessSequenceEntry(entry);
                }

                return;
            }

            foreach (var key in obj.GetOwnPropertyKeys())
            {
                var value = obj.Get(key);
                Append(key.ToString(), value.ToString());
            }

            return;
        }

        TypeErrorHelper.Throw(
            Engine,
            "The provided value is not a valid sequence or record.",
            "constructor",
            nameof(Headers)
        );
    }

    private void ProcessSequenceEntry(JsValue value)
    {
        if (!value.IsObject())
        {
            TypeErrorHelper.Throw(
                Engine,
                "Each header pair must be an object",
                "constructor",
                nameof(Headers)
            );
        }

        var sequence = value.AsObject();
        var lengthValue = sequence.Get("length");

        if (!lengthValue.IsNumber())
        {
            TypeErrorHelper.Throw(
                Engine,
                "Iterator value must be an object with a length property",
                "constructor",
                nameof(Headers)
            );
        }

        var length = (int)lengthValue.AsNumber();
        if (length < 2)
        {
            TypeErrorHelper.Throw(
                Engine,
                "Iterator value must have a length of 2",
                "constructor",
                nameof(Headers)
            );
        }

        var name = sequence.Get("0").ToString();
        var headerValue = sequence.Get("1").ToString();

        Append(name, headerValue);
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#headers-validate
    /// </summary>
    private bool Validate(string name, [NotNull] string? value, string method)
    {
        // If name is not a header name or value is not a header value, then throw a TypeError.
        if (!HttpHelper.IsHeaderName(name))
        {
            TypeErrorHelper.Throw(Engine, "Invalid header name", method, nameof(Headers));
        }
        if (!HttpHelper.IsHeaderValue(value))
        {
            TypeErrorHelper.Throw(Engine, "Invalid header value", method, nameof(Headers));
        }

        switch (Guard)
        {
            case Guard.Request:
                if (HttpHelper.IsForbiddenRequestHeader(name, value))
                {
                    return false;
                }
                break;

            case Guard.Response:
                if (HttpHelper.IsForbiddenResponseHeader(name))
                {
                    return false;
                }
                break;

            case Guard.Immutable:
                TypeErrorHelper.Throw(Engine, "Headers are immutable", method, nameof(Headers));
                break;
        }

        return true;
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-header-value-normalize
    /// </summary>
    private static string Normalize(string byteSequence)
    {
        // To normalize a byte sequence potentialValue, remove any leading and trailing HTTP whitespace bytes from potentialValue.
        return byteSequence.Trim();
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-headers-remove-privileged-no-cors-request-headers
    /// </summary>
    private void RemovePrivilegedNoCORSRequestHeaders()
    {
        // For each headerName of privileged no-CORS request-header names:
        // Delete headerName from headers’s header list.
        _headers.RemoveAll(header => HttpHelper.IsPrivilegedNoCORSRequestHeaderName(header.Name));
    }
}
