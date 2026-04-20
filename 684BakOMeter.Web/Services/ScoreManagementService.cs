using _684BakOMeter.Web.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _684BakOMeter.Web.Services;

public class ScoreManagementService
{
    private readonly AppDbContext _db;
    private readonly ILogger<ScoreManagementService> _logger;

    public ScoreManagementService(AppDbContext db, ILogger<ScoreManagementService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<PlayerScoreSummary>> GetPlayersWithScoresAsync()
    {
        var players = await _db.Players
            .AsNoTracking()
            .Select(p => new
            {
                p.Id,
                p.Name,
                AttemptCount = p.Attempts.Count()
            })
            .ToListAsync();

        var result = players
            .Where(p => p.AttemptCount > 0)
            .OrderBy(p => p.Name)
            .Select(p => new PlayerScoreSummary(
                p.Id,
                p.Name,
                p.AttemptCount))
            .ToList();

        return result;
    } 

    public async Task<ScoreDeleteResult> ClearAllScoresAsync()
    {
        var total = await _db.ChugAttempts.CountAsync();
        if (total == 0)
            return ScoreDeleteResult.NothingToDelete("Er zijn geen scores om te verwijderen.");

        _db.ChugAttempts.RemoveRange(_db.ChugAttempts);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Manager cleared all scores. Removed {RemovedCount} attempts.", total);
        return ScoreDeleteResult.MarkedDeleted(total, $"{total} score(s) verwijderd.");
    }

    public async Task<ScoreDeleteResult> ClearScoresForPlayerAsync(int playerId)
    {
        var player = await _db.Players
            .AsNoTracking()
            .Where(p => p.Id == playerId)
            .Select(p => new { p.Id, p.Name })
            .FirstOrDefaultAsync();

        if (player is null)
            return ScoreDeleteResult.MarkedNotFound("Speler niet gevonden.");

        var attempts = await _db.ChugAttempts
            .Where(a => a.PlayerId == playerId)
            .ToListAsync();

        if (attempts.Count == 0)
            return ScoreDeleteResult.NothingToDelete($"{player.Name} heeft geen scores om te verwijderen.");

        _db.ChugAttempts.RemoveRange(attempts);
        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "Manager cleared scores for player {PlayerId} ({PlayerName}). Removed {RemovedCount} attempts.",
            player.Id,
            player.Name,
            attempts.Count);

        return ScoreDeleteResult.MarkedDeleted(attempts.Count, $"{attempts.Count} score(s) van {player.Name} verwijderd.");
    }
}

public record PlayerScoreSummary(int PlayerId, string PlayerName, int AttemptCount);

public record ScoreDeleteResult(bool IsDeleted, int RemovedCount, string Message, bool IsNotFound)
{
    public static ScoreDeleteResult MarkedDeleted(int removedCount, string message)
        => new(true, removedCount, message, false);

    public static ScoreDeleteResult NothingToDelete(string message)
        => new(false, 0, message, false);

    public static ScoreDeleteResult MarkedNotFound(string message)
        => new(false, 0, message, true);
}
