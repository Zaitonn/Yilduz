using System.Collections.Generic;

namespace Yilduz.Network;

sealed record MIMEType
{
    public MIMEType(string type, string subtype)
    {
        Type = type;
        Subtype = subtype;
        Parameters = [];
    }

    internal string Type { get; }
    internal string Subtype { get; }
    internal Dictionary<string, string> Parameters { get; }
    internal string Essence => $"{Type}/{Subtype}";
}
