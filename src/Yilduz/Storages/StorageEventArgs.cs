using System;
using Yilduz.Storages.Storage;

namespace Yilduz.Storages;

public sealed class StorageEventArgs : EventArgs
{
    public string? Key { get; }

    public string? NewValue { get; }

    public string? OldValue { get; }

    public StorageInstance StorageArea { get; }

    internal StorageEventArgs(
        string? key,
        string? newValue,
        string? oldValue,
        StorageInstance storageArea
    )
    {
        Key = key;
        NewValue = newValue;
        OldValue = oldValue;
        StorageArea = storageArea;
    }
}
