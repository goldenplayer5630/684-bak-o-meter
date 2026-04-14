using _684BakOMeter.Web.Data.Repositories;
using _684BakOMeter.Web.Domain.Entities;

namespace _684BakOMeter.Web.Services;

/// <summary>
/// Handles NFC tag resolution, user creation from unknown tags,
/// and linking/unlinking tags to existing users.
/// </summary>
public class NfcService
{
    private readonly INfcTagRepository _nfcTags;
    private readonly IPlayerRepository _players;

    public NfcService(INfcTagRepository nfcTags, IPlayerRepository players)
    {
        _nfcTags = nfcTags;
        _players = players;
    }

    /// <summary>
    /// Resolves a scanned NFC tag UID.
    /// Returns the linked player if the tag is known, or null if unknown.
    /// </summary>
    public async Task<(NfcTag? Tag, Player? Player, bool IsKnown)> ResolveTagAsync(string uid)
    {
        var normalized = uid.Trim().ToUpperInvariant();
        var tag = await _nfcTags.GetByUidAsync(normalized);

        if (tag is null)
            return (null, null, false);

        if (!tag.IsActive)
            return (tag, null, false);

        return (tag, tag.Player, true);
    }

    /// <summary>
    /// Creates a new user and immediately links the scanned NFC tag to them.
    /// Validates that the username is unique and the tag UID is not already in use.
    /// </summary>
    public async Task<(Player Player, NfcTag Tag, string? Error)> CreateUserFromNfcAsync(
        string rawName, string tagUid)
    {
        var normalizedName = rawName.Trim().ToLower();
        var normalizedUid = tagUid.Trim().ToUpperInvariant();

        if (string.IsNullOrEmpty(normalizedName))
            return (null!, null!, "Naam mag niet leeg zijn.");

        // Check unique username
        var existingPlayer = await _players.GetByNameAsync(normalizedName);
        if (existingPlayer is not null)
            return (null!, null!, "Deze naam is al in gebruik.");

        // Check tag is not already linked
        var existingTag = await _nfcTags.GetByUidAsync(normalizedUid);
        if (existingTag is not null)
            return (null!, null!, "Deze NFC tag is al gekoppeld aan een andere speler.");

        var player = new Player { Name = normalizedName };
        await _players.AddAsync(player);

        var tag = new NfcTag
        {
            PlayerId = player.Id,
            Uid = normalizedUid,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };
        await _nfcTags.AddAsync(tag);

        return (player, tag, null);
    }

    /// <summary>
    /// Links a new NFC tag to an existing user.
    /// Validates that the tag UID is not already in use by another player.
    /// </summary>
    public async Task<(NfcTag? Tag, string? Error)> LinkTagToPlayerAsync(int playerId, string tagUid)
    {
        var normalizedUid = tagUid.Trim().ToUpperInvariant();

        var existingTag = await _nfcTags.GetByUidAsync(normalizedUid);
        if (existingTag is not null)
        {
            if (existingTag.PlayerId == playerId)
                return (null, "Deze tag is al gekoppeld aan jouw account.");
            return (null, "Deze tag is al gekoppeld aan een andere speler.");
        }

        var player = await _players.GetByIdAsync(playerId);
        if (player is null)
            return (null, "Speler niet gevonden.");

        var tag = new NfcTag
        {
            PlayerId = playerId,
            Uid = normalizedUid,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
        };
        await _nfcTags.AddAsync(tag);

        return (tag, null);
    }

    /// <summary>
    /// Removes an NFC tag from a player. Prevents removing the last active tag
    /// to avoid locking the user out.
    /// </summary>
    public async Task<string?> RemoveTagAsync(int tagId, int playerId)
    {
        var tag = await _nfcTags.GetByIdAsync(tagId);
        if (tag is null)
            return "Tag niet gevonden.";

        if (tag.PlayerId != playerId)
            return "Deze tag hoort niet bij jouw account.";

        // Prevent removing the last active tag
        var activeCount = await _nfcTags.CountByPlayerIdAsync(playerId);
        if (activeCount <= 1)
            return "Je kunt je laatste NFC tag niet verwijderen.";

        await _nfcTags.DeleteAsync(tagId);
        return null;
    }
}
