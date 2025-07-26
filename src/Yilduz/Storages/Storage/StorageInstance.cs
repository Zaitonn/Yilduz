using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Descriptors;

namespace Yilduz.Storages.Storage;

#pragma warning disable IDE0046

/// <summary>
/// https://developer.mozilla.org/en-US/docs/Web/API/Storage
/// </summary>
public sealed class StorageInstance : ObjectInstance
{
    private readonly IDictionary<string, string> _map;

    public StorageInstance(Engine engine, IDictionary<string, string>? writableDict = null)
        : base(engine)
    {
        _map = writableDict ?? new Dictionary<string, string>();
    }

    public event EventHandler<StorageEventArgs>? Updated;

    private void OnUpdated(string? key, string? newValue, string? oldValue)
    {
        Updated?.Invoke(this, new StorageEventArgs(key, newValue, oldValue, this));
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Storage/length
    /// </summary>
    public int Length
    {
        get
        {
            lock (_map)
            {
                return _map.Count;
            }
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Storage/clear
    /// </summary>
    public void Clear()
    {
        lock (_map)
        {
            _map.Clear();
        }

        OnUpdated(null, null, null);
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Storage/getItem
    /// </summary>
    public string? GetItem(string key)
    {
        lock (_map)
        {
            return _map.TryGetValue(key, out var value) ? value : null;
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Storage/removeItem
    /// </summary>
    public void RemoveItem(string key)
    {
        lock (_map)
        {
#if !NETSTANDARD2_0
            _map.Remove(key, out var oldValue);
#else
            if (!_map.TryGetValue(key, out var oldValue))
            {
                return;
            }
            _map.Remove(key);
#endif
            OnUpdated(key, null, oldValue);
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Storage/setItem
    /// </summary>
    public void SetItem(string key, string? value)
    {
        lock (_map)
        {
            _map.TryGetValue(key, out var oldValue);
            _map[key] = value ?? "null";

            OnUpdated(key, value, oldValue);
        }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Storage/key
    /// </summary>
    public string? Key(int index)
    {
        lock (_map)
        {
            return _map.Keys.ElementAtOrDefault(index);
        }
    }

    public string? this[string key]
    {
        get => GetItem(key);
        set => SetItem(key, value!);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override string ToString()
    {
        return "[object Storage]";
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override bool HasProperty(JsValue property)
    {
        if (property.IsString())
        {
            var key = property.ToString();
            if (_map.ContainsKey(key))
            {
                return true;
            }
        }
        return base.HasProperty(property);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override PropertyDescriptor GetOwnProperty(JsValue property)
    {
        if (IsStorageKey(property, out var key))
        {
            return _map.TryGetValue(key, out var value)
                ? new PropertyDescriptor(value, true, false, true)
                : new PropertyDescriptor(Null, false, false, true);
        }

        return base.GetOwnProperty(property);
    }

    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override bool Set(JsValue property, JsValue value, JsValue receiver)
    {
        if (IsStorageKey(property, out var key))
        {
            SetItem(key, value.ToString());
            return true;
        }

        return base.Set(property, value, receiver);
    }

    private bool IsStorageKey(JsValue property, [NotNullWhen(true)] out string? key)
    {
        key = null;
        var propertyKey = TypeConverter.ToPropertyKey(property);

        if (!propertyKey.IsString())
        {
            return false;
        }
        key = propertyKey.AsString();

        if (string.IsNullOrEmpty(key))
        {
            return false;
        }

        if (Prototype is not null)
        {
            if (Prototype.GetOwnPropertyKeys().Contains(key))
            {
                return false;
            }
        }

        if (Engine.Intrinsics.Object.PrototypeObject.GetOwnPropertyKeys().Contains(key))
        {
            return false;
        }

        return true;
    }
}
