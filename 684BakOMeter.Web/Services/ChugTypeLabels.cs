using _684BakOMeter.Web.Domain.Enums;

namespace _684BakOMeter.Web.Services;

/// <summary>Dutch display labels for chug types used in the arcade UI.</summary>
public static class ChugTypeLabels
{
    public static readonly Dictionary<ChugType, string> All = new()
    {
        { ChugType.Bak,     "Bak" },
        { ChugType.Pul,     "Pul" },
        { ChugType.BakPlus, "Bak+" },
        { ChugType.IceFles, "Ice Fles" },
        { ChugType.SpaRood, "Spa Rood" },
        { ChugType.Wijn,    "Wijn" },
        { ChugType.Pitcher, "Pitcher" },
    };

    public static string GetLabel(ChugType type)
        => All.TryGetValue(type, out var label) ? label : type.ToString();

    public static string GetSlug(ChugType type)
        => type.ToString();
}
