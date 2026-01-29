using System;
using System.Collections.Generic;
using Yilduz.Storages.Storage;

namespace Yilduz;

public sealed partial class Options
{
    public StorageOptions Storage { get; init; } = new();

    public sealed class StorageOptions
    {
        public IDictionary<string, string>? LocalStorageDataProvider { get; init; }
        public IDictionary<string, string>? SessionStorageDataProvider { get; init; }

        public Action<StorageInstance>? LocalStorageConfigurator { get; init; }
        public Action<StorageInstance>? SessionStorageConfigurator { get; init; }
    }
}
