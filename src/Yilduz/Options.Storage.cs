using System;
using System.Collections.Generic;
using Yilduz.Storages.Storage;

namespace Yilduz;

public sealed partial class Options
{
    public StorageOptions Storage { get; init; } = new();

    public sealed class StorageOptions
    {
        public IDictionary<string, string>? LocalStorageDataProvider { get; set; }
        public IDictionary<string, string>? SessionStorageDataProvider { get; set; }

        public Action<StorageInstance>? LocalStorageConfigurator { get; set; }
        public Action<StorageInstance>? SessionStorageConfigurator { get; set; }
    }
}
