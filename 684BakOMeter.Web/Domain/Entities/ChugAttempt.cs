using _684BakOMeter.Web.Domain.Enums;

namespace _684BakOMeter.Web.Domain.Entities;

/// <summary>
/// Represents one measured beer-chug session recorded by the load cell.
/// "ChugAttempt" is preferred over "Score" or "Measurement" because it directly
/// maps to the real-world action: a player picks up a glass and attempts to chug
/// it while the load cell tracks the event timing.
/// </summary>
public class ChugAttempt
{
    public int Id { get; set; }

    /// <summary>Foreign key to the player who performed this attempt.</summary>
    public int PlayerId { get; set; }

    /// <summary>The player who performed this attempt.</summary>
    public Player Player { get; set; } = null!;

    /// <summary>UTC timestamp when the session started.</summary>
    public DateTime StartedAt { get; set; }

    /// <summary>UTC timestamp when the session ended.</summary>
    public DateTime EndedAt { get; set; }

    /// <summary>Total duration of the chug in milliseconds.</summary>
    public int DurationMs { get; set; }

    /// <summary>The type of drink used in this attempt.</summary>
    public ChugType ChugType { get; set; }

    /// <summary>Optional free-text notes (e.g. "cold lager", "personal best").</summary>
    public string? Notes { get; set; }

    /// <summary>HIgh score</summary>
    public bool IsHighScore { get; set; }   

    // --- 1v1 back-references (null when this is a solo attempt) ---

    /// <summary>Set when this attempt is Player 1's turn in a 1v1 match.</summary>
    public int? OneVsOneMatchIdAsPlayer1 { get; set; }
    public OneVsOneMatch? OneVsOneMatchAsPlayer1 { get; set; }

    /// <summary>Set when this attempt is Player 2's turn in a 1v1 match.</summary>
    public int? OneVsOneMatchIdAsPlayer2 { get; set; }
    public OneVsOneMatch? OneVsOneMatchAsPlayer2 { get; set; }
}
