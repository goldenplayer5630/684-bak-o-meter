using _684BakOMeter.Web.Domain.Enums;

namespace _684BakOMeter.Web.Domain.Entities;

/// <summary>
/// Represents a head-to-head 1v1 competition between two players for a specific ChugType.
/// Each player performs a ChugAttempt and the fastest time wins.
/// The match can be pending (one or both players haven't gone yet) or completed.
/// </summary>
public class OneVsOneMatch
{
    public int Id { get; set; }

    /// <summary>The type of drink being contested in this match.</summary>
    public ChugType ChugType { get; set; }

    /// <summary>UTC timestamp when the match was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>UTC timestamp when both players have finished and a winner is determined. Null while pending.</summary>
    public DateTime? CompletedAt { get; set; }

    // --- Player 1 (challenger) ---
    public int Player1Id { get; set; }
    public Player Player1 { get; set; } = null!;

    /// <summary>Player 1's attempt. Null until they have gone.</summary>
    public int? Player1AttemptId { get; set; }
    public ChugAttempt? Player1Attempt { get; set; }

    // --- Player 2 (challenged) ---
    public int Player2Id { get; set; }
    public Player Player2 { get; set; } = null!;

    /// <summary>Player 2's attempt. Null until they have gone.</summary>
    public int? Player2AttemptId { get; set; }
    public ChugAttempt? Player2Attempt { get; set; }

    // --- Outcome ---

    /// <summary>
    /// The ID of the winning player, or null if the match is still pending or tied.
    /// </summary>
    public int? WinnerId { get; set; }
    public Player? Winner { get; set; }

    /// <summary>Optional notes about the match (e.g. "Rematch requested!").</summary>
    public string? Notes { get; set; }

    /// <summary>True when both players have completed their attempt and a result is recorded.</summary>
    public bool IsComplete => Player1AttemptId.HasValue && Player2AttemptId.HasValue && CompletedAt.HasValue;
}
