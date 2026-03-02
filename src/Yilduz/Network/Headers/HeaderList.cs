using System;
using System.Collections.Generic;
using System.Linq;

namespace Yilduz.Network.Headers;

internal sealed class HeaderList : List<HeaderEntry>
{
    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-header-list-get
    /// </summary>
    public string? Get(string name)
    {
        var values = this.Where(header =>
                header.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
            )
            .Select(header => header.Value)
            .ToArray();

        return values.Length == 0 ? null : string.Join(", ", values);
    }

    /// <summary>
    /// https://fetch.spec.whatwg.org/#concept-header-list-set
    /// </summary>
    public void Set(string name, string value)
    {
        if (!this.Any(header => header.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            Add(new(name, value));
        }
        else
        {
            var found = false;
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    if (!found)
                    {
                        this[i] = new(name, value);
                        found = true;
                    }
                    else
                    {
                        RemoveAt(i);
                        i--;
                    }
                }
            }
        }
    }
}
