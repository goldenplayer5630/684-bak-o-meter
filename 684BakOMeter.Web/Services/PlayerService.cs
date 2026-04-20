using _684BakOMeter.Web.Data.Repositories;
using _684BakOMeter.Web.Domain.Entities;

namespace _684BakOMeter.Web.Services;

/// <summary>
/// Resolves a player from a raw name input.
/// Normalizes to lower-case, trims whitespace, and either finds
/// an existing player or creates a new one.
/// </summary>
public class PlayerService
{
    private readonly IPlayerRepository _players;

    public PlayerService(IPlayerRepository players)
    {
        _players = players;
    }

    /// <summary>
    /// Normalizes the raw name, looks up an existing player,
    /// and creates one if none exists. Returns the resolved player.
    /// </summary>
    public async Task<Player> ResolvePlayerAsync(string rawName)
    {
        var displayName = rawName.Trim();
        var normalized = Normalize(displayName);

        if (string.IsNullOrEmpty(normalized))
            throw new ArgumentException("Player name cannot be empty.", nameof(rawName));

        var existing = await _players.GetByNameAsync(normalized);
        if (existing is not null)
            return existing;

        var player = new Player { Name = displayName };
        await _players.AddAsync(player);
        return player;
    }

    /// <summary>Trims and lower-cases a raw name for uniqueness comparisons.</summary>
    public static string Normalize(string raw)
        => raw.Trim().ToLowerInvariant();
}
