using _684BakOMeter.Web.Domain.Enums;

namespace _684BakOMeter.Web.ViewModels;

/// <summary>One row in the chug-attempts scoreboard / list.</summary>
public class ChugAttemptListItemViewModel
{
    public int Id { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public int DurationMs { get; set; }
    public ChugType ChugType { get; set; }

    /// <summary>Formatted duration string for display (e.g. "3.2 s").</summary>
    public string DurationDisplay => $"{DurationMs / 1000.0:F2} s";

    /// <summary>Formatted ChugType for display.</summary>
    public string ChugTypeDisplay => ChugType.ToString();
}
