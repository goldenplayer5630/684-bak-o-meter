using _684BakOMeter.Web.Domain.Enums;

namespace _684BakOMeter.Web.ViewModels;

/// <summary>Full detail view of a single chug attempt.</summary>
public class ChugAttemptDetailsViewModel
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
    public int DurationMs { get; set; }
    public ChugType ChugType { get; set; }
    public string? Notes { get; set; }

    /// <summary>Formatted duration for display (e.g. "3.20 s").</summary>
    public string DurationDisplay => $"{DurationMs / 1000.0:F2} s";

    /// <summary>Formatted ChugType for display.</summary>
    public string ChugTypeDisplay => ChugType.ToString();
}
