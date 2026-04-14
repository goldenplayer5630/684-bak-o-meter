namespace _684BakOMeter.Web.Domain.Entities;

/// <summary>
/// Represents a physical NFC tag linked to a player.
/// Each player can have multiple NFC tags; each tag belongs to exactly one player.
/// </summary>
public class NfcTag
{
    public int Id { get; set; }

    /// <summary>Foreign key to the owning player.</summary>
    public int PlayerId { get; set; }

    /// <summary>Navigation property to the owning player.</summary>
    public Player Player { get; set; } = null!;

    /// <summary>The unique UID read from the NFC tag hardware.</summary>
    public string Uid { get; set; } = string.Empty;

    /// <summary>UTC timestamp when this tag was first linked.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Whether this tag is currently active and can be used to authenticate.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Optional human-readable label for the tag (e.g. "Rode kaart").</summary>
    public string? Label { get; set; }
}
