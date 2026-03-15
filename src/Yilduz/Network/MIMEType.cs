using System.Collections.Generic;

namespace Yilduz.Network;

internal sealed record MIMEType
{
    public MIMEType(string type, string subtype)
    {
        Type = type;
        Subtype = subtype;
        Parameters = [];
    }

    public string Type { get; }

    public string Subtype { get; }

    public Dictionary<string, string> Parameters { get; }

    public string Essence => $"{Type}/{Subtype}";
}
