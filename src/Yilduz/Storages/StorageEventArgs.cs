using System;
using Yilduz.Storages.Storage;

namespace Yilduz.Storages;

public sealed class StorageEventArgs : EventArgs
{
    public string? Key { get; }

    public string? NewValue { get; }

    public string? OldValue { get; }

    public StorageInstance Storage { get; }

    internal StorageEventArgs(
        string? key,
        string? newValue,
        string? oldValue,
        StorageInstance storage
    )
    {
        Key = key;
        NewValue = newValue;
        OldValue = oldValue;
        Storage = storage;
    }
}
