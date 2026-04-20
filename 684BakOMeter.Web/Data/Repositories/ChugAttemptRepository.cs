using _684BakOMeter.Web.Data.Persistence;
using _684BakOMeter.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace _684BakOMeter.Web.Data.Repositories;

/// <summary>Best personal result for one chug type.</summary>
public record PersonalStat(
    ChugType ChugType,
    string   Label,
    int      BestDurationMs,
    int      Rank,
    int      TotalAttempts,
    int      AttemptCount
);

/// <summary>EF Core implementation of IChugAttemptRepository.</summary>
public class ChugAttemptRepository : IChugAttemptRepository
{
    private readonly AppDbContext _db;

    public ChugAttemptRepository(AppDbContext db)
        => _db = db;

    public async Task<IEnumerable<ChugAttempt>> GetAllAsync()
    {
        var attempts = await _db.ChugAttempts
            .Include(ca => ca.Player)
            .OrderByDescending(ca => ca.StartedAt)
            .ToListAsync();

        await MarkHighScoresAsync(attempts);
        return attempts;
    }

    public async Task<ChugAttempt?> GetByIdAsync(int id)
    {
        var attempt = await _db.ChugAttempts
            .Include(ca => ca.Player)
            .FirstOrDefaultAsync(ca => ca.Id == id);

        if (attempt is not null)
            await MarkHighScoresAsync([attempt]);

        return attempt;
    }

    public async Task AddAsync(ChugAttempt attempt)
    {
        _db.ChugAttempts.Add(attempt);
        await _db.SaveChangesAsync();

        // Compute IsHighScore after insert so the caller can read it on the returned object
        await MarkHighScoresAsync([attempt]);
    }

    public async Task UpdateAsync(ChugAttempt attempt)
    {
        _db.ChugAttempts.Update(attempt);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var attempt = await _db.ChugAttempts.FindAsync(id);
        if (attempt is not null)
        {
            _db.ChugAttempts.Remove(attempt);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<ChugAttempt>> GetRecentAsync(int count)
    {
        var attempts = await _db.ChugAttempts
            .Include(ca => ca.Player)
            .OrderByDescending(ca => ca.StartedAt)
            .Take(count)
            .ToListAsync();

        await MarkHighScoresAsync(attempts);
        return attempts;
    }

    public async Task<IEnumerable<ChugAttempt>> GetByPlayerIdAsync(int playerId)
    {
        var attempts = await _db.ChugAttempts
            .Include(ca => ca.Player)
            .Where(ca => ca.PlayerId == playerId)
            .OrderByDescending(ca => ca.StartedAt)
            .ToListAsync();

        await MarkHighScoresAsync(attempts);
        return attempts;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChugAttempt>> GetLeaderboardAsync(ChugType chugType, int count = 10)
    {
        // One entry per player — the attempt with the lowest DurationMs (ties: lowest Id)
        var bestIds = await _db.ChugAttempts
            .AsNoTracking()
            .Where(a => a.ChugType == chugType)
            .GroupBy(a => a.PlayerId)
            .Select(g => g.OrderBy(a => a.DurationMs).ThenBy(a => a.Id).First().Id)
            .ToListAsync();

        var attempts = await _db.ChugAttempts
            .Include(ca => ca.Player)
            .Where(ca => bestIds.Contains(ca.Id))
            .OrderBy(ca => ca.DurationMs)
            .Take(count)
            .ToListAsync();

        // Every returned attempt is the player's best for this type
        foreach (var a in attempts) a.IsHighScore = true;
        return attempts;
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<ChugAttempt> Items, int TotalCount)> GetLeaderboardPagedAsync(
        ChugType chugType, int page, int pageSize)
    {
        // One entry per player — the attempt with the lowest DurationMs (ties: lowest Id)
        var bestIds = await _db.ChugAttempts
            .AsNoTracking()
            .Where(a => a.ChugType == chugType)
            .GroupBy(a => a.PlayerId)
            .Select(g => g.OrderBy(a => a.DurationMs).ThenBy(a => a.Id).First().Id)
            .ToListAsync();

        var total = bestIds.Count;
        var items = await _db.ChugAttempts
            .Include(ca => ca.Player)
            .Where(ca => bestIds.Contains(ca.Id))
            .OrderBy(ca => ca.DurationMs)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        foreach (var a in items) a.IsHighScore = true;
        return (items, total);
    }

    /// <inheritdoc />
    public async Task<int?> GetAttemptRankAsync(int attemptId, ChugType chugType)
    {
        var attempt = await _db.ChugAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.ChugType == chugType);
        if (attempt is null) return null;

        // Rank the player's personal best duration, not necessarily this specific attempt
        var playerBestDuration = await _db.ChugAttempts
            .AsNoTracking()
            .Where(a => a.PlayerId == attempt.PlayerId && a.ChugType == chugType)
            .MinAsync(a => a.DurationMs);

        // Count players whose personal best is strictly faster
        var rank = await _db.ChugAttempts
            .AsNoTracking()
            .Where(a => a.ChugType == chugType)
            .GroupBy(a => a.PlayerId)
            .Select(g => g.Min(a => a.DurationMs))
            .CountAsync(best => best < playerBestDuration) + 1;

        return rank;
    }

    /// <inheritdoc />
    public async Task<ChugAttempt?> GetPersonalBestAsync(int playerId, ChugType chugType)
    {
        var best = await _db.ChugAttempts
            .AsNoTracking()
            .Where(a => a.PlayerId == playerId && a.ChugType == chugType)
            .OrderBy(a => a.DurationMs)
            .ThenBy(a => a.Id)
            .FirstOrDefaultAsync();

        if (best is not null) best.IsHighScore = true;
        return best;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PersonalStat>> GetPersonalStatsAsync(int playerId)
    {
        // Load all attempts for this player in one query
        var playerAttempts = await _db.ChugAttempts
            .AsNoTracking()
            .Where(a => a.PlayerId == playerId)
            .ToListAsync();

        var stats = new List<PersonalStat>();

        foreach (var group in playerAttempts.GroupBy(a => a.ChugType))
        {
            var chugType     = group.Key;
            // Personal best: lowest DurationMs, tie-broken by earliest Id
            var best         = group.OrderBy(a => a.DurationMs).ThenBy(a => a.Id).First();
            var attemptCount = group.Count();

            // Rank: count players whose personal best is strictly faster than this player's
            var rank = await _db.ChugAttempts
                .AsNoTracking()
                .Where(a => a.ChugType == chugType)
                .GroupBy(a => a.PlayerId)
                .Select(g => g.Min(a => a.DurationMs))
                .CountAsync(bestMs => bestMs < best.DurationMs) + 1;

            var total = await _db.ChugAttempts
                .AsNoTracking()
                .Where(a => a.ChugType == chugType)
                .Select(a => a.PlayerId)
                .Distinct()
                .CountAsync();

            stats.Add(new PersonalStat(chugType, string.Empty, best.DurationMs, rank, total, attemptCount));
        }

        return stats.OrderBy(s => s.ChugType);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<ChugAttempt>> GetRecentByPlayerAndTypeAsync(
        int playerId, ChugType chugType, int count = 10)
    {
        var attempts = await _db.ChugAttempts
            .AsNoTracking()
            .Where(a => a.PlayerId == playerId && a.ChugType == chugType)
            .OrderByDescending(a => a.StartedAt)
            .Take(count)
            .ToListAsync();

        await MarkHighScoresAsync(attempts);
        return attempts;
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Computes and sets <see cref="ChugAttempt.IsHighScore"/> on every attempt in
    /// <paramref name="attempts"/>. For each unique (PlayerId, ChugType) group present
    /// in the list, one database query finds the overall best attempt ID
    /// (lowest DurationMs, ties broken by lowest Id). Only that attempt is marked true.
    /// </summary>
    private async Task MarkHighScoresAsync(List<ChugAttempt> attempts)
    {
        if (attempts.Count == 0) return;

        // Collect unique (player, type) pairs from the loaded set
        var pairs = attempts
            .Select(a => (a.PlayerId, a.ChugType))
            .Distinct()
            .ToList();

        // One query per pair to find the global best attempt ID
        var bestIds = new HashSet<int>();
        foreach (var (playerId, chugType) in pairs)
        {
            var bestId = await _db.ChugAttempts
                .AsNoTracking()
                .Where(a => a.PlayerId == playerId && a.ChugType == chugType)
                .OrderBy(a => a.DurationMs)
                .ThenBy(a => a.Id)      // tie-break: earliest attempt wins
                .Select(a => (int?)a.Id)
                .FirstOrDefaultAsync();

            if (bestId.HasValue)
                bestIds.Add(bestId.Value);
        }

        foreach (var attempt in attempts)
            attempt.IsHighScore = bestIds.Contains(attempt.Id);
    }
}
