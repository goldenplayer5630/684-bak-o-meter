namespace _684BakOMeter.Web.Domain.Entities;

/// <summary>
/// Represents a player / participant registered on the machine.
/// </summary>
public class Player
{
    public int Id { get; set; }

    /// <summary>The display name shown on the scoreboard.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>All solo chug attempts made by this player.</summary>
    public ICollection<ChugAttempt> Attempts { get; set; } = new List<ChugAttempt>();

    /// <summary>All 1v1 matches where this player is on the left side (challenger).</summary>
    public ICollection<OneVsOneMatch> MatchesAsPlayer1 { get; set; } = new List<OneVsOneMatch>();

    /// <summary>All 1v1 matches where this player is on the right side (challenged).</summary>
    public ICollection<OneVsOneMatch> MatchesAsPlayer2 { get; set; } = new List<OneVsOneMatch>();

    /// <summary>All 1v1 matches this player has won.</summary>
    public ICollection<OneVsOneMatch> MatchesWon { get; set; } = new List<OneVsOneMatch>();

    /// <summary>All NFC tags linked to this player for authentication.</summary>
    public ICollection<NfcTag> NfcTags { get; set; } = new List<NfcTag>();
}
