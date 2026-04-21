using _684BakOMeter.Web.Domain.Entities;

namespace _684BakOMeter.Web.Services;

/// <summary>Dutch display labels for chug types used in the arcade UI.</summary>
public static class ChugTypeLabels
{
    /// <summary>All chug types with their display labels.</summary>
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

    /// <summary>Chug types visible in the main menu and leaderboards.</summary>
    public static readonly Dictionary<ChugType, string> Main = new()
    {
        { ChugType.Bak,     "Bak" },
        { ChugType.Pul,     "Pul" },
        { ChugType.BakPlus, "Bak+" },
        { ChugType.SpaRood, "Spa Rood" },
        { ChugType.Wijn,    "Wijn" },
    };

    /// <summary>Hidden easter-egg chug types (IceFles, Pitcher).</summary>
    public static readonly Dictionary<ChugType, string> Hidden = new()
    {
        { ChugType.IceFles, "Ice Fles" },
        { ChugType.Pitcher, "Pitcher" },
    };

    public static string GetLabel(ChugType type)
        => All.TryGetValue(type, out var label) ? label : type.ToString();

    public static string GetSlug(ChugType type)
        => type.ToString();
}
