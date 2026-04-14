using _684BakOMeter.Web.Data.Persistence;
using _684BakOMeter.Web.Domain.Entities;
using _684BakOMeter.Web.Domain.Enums;
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
        => await _db.ChugAttempts
                    .Include(ca => ca.Player)
                    .OrderByDescending(ca => ca.StartedAt)
                    .ToListAsync();

    public async Task<ChugAttempt?> GetByIdAsync(int id)
        => await _db.ChugAttempts
                    .Include(ca => ca.Player)
                    .FirstOrDefaultAsync(ca => ca.Id == id);

    public async Task AddAsync(ChugAttempt attempt)
    {
        _db.ChugAttempts.Add(attempt);
        await _db.SaveChangesAsync();
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
        => await _db.ChugAttempts
                    .Include(ca => ca.Player)
                    .OrderByDescending(ca => ca.StartedAt)
                    .Take(count)
                    .ToListAsync();

    public async Task<IEnumerable<ChugAttempt>> GetByPlayerIdAsync(int playerId)
        => await _db.ChugAttempts
                    .Include(ca => ca.Player)
                    .Where(ca => ca.PlayerId == playerId)
                    .OrderByDescending(ca => ca.StartedAt)
                    .ToListAsync();

    /// <inheritdoc />
    public async Task<IEnumerable<ChugAttempt>> GetLeaderboardAsync(ChugType chugType, int count = 10)
    {
        // One entry per player — their personal best for this chug type
        var bestIds = await _db.ChugAttempts
            .AsNoTracking()
            .Where(a => a.ChugType == chugType)
            .GroupBy(a => a.PlayerId)
            .Select(g => g.OrderBy(a => a.DurationMs).First().Id)
            .ToListAsync();

        return await _db.ChugAttempts
            .Include(ca => ca.Player)
            .Where(ca => bestIds.Contains(ca.Id))
            .OrderBy(ca => ca.DurationMs)
            .Take(count)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<(IEnumerable<ChugAttempt> Items, int TotalCount)> GetLeaderboardPagedAsync(
        ChugType chugType, int page, int pageSize)
    {
        // One entry per player — their personal best for this chug type
        var bestIds = await _db.ChugAttempts
            .AsNoTracking()
            .Where(a => a.ChugType == chugType)
            .GroupBy(a => a.PlayerId)
            .Select(g => g.OrderBy(a => a.DurationMs).First().Id)
            .ToListAsync();

        var query = _db.ChugAttempts
            .Include(ca => ca.Player)
            .Where(ca => bestIds.Contains(ca.Id))
            .OrderBy(ca => ca.DurationMs);

        var total = bestIds.Count;
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, total);
    }

    /// <inheritdoc />
    public async Task<int?> GetAttemptRankAsync(int attemptId, ChugType chugType)
    {
        var attempt = await _db.ChugAttempts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.ChugType == chugType);
        if (attempt is null) return null;

        // Only rank against each player's personal best
        var rank = await _db.ChugAttempts
            .AsNoTracking()
            .Where(a => a.ChugType == chugType)
            .GroupBy(a => a.PlayerId)
            .Select(g => g.Min(a => a.DurationMs))
            .CountAsync(best => best < attempt.DurationMs) + 1;

        return rank;
    }

    /// <inheritdoc />
    public async Task<ChugAttempt?> GetPersonalBestAsync(int playerId, ChugType chugType)
        => await _db.ChugAttempts
                    .AsNoTracking()
                    .Where(a => a.PlayerId == playerId && a.ChugType == chugType)
                    .OrderBy(a => a.DurationMs)
                    .FirstOrDefaultAsync();

    /// <inheritdoc />
    public async Task<IEnumerable<PersonalStat>> GetPersonalStatsAsync(int playerId)
    {
        // Load all attempts for this player
        var playerAttempts = await _db.ChugAttempts
            .AsNoTracking()
            .Where(a => a.PlayerId == playerId)
            .ToListAsync();

        var stats = new List<PersonalStat>();

        foreach (var group in playerAttempts.GroupBy(a => a.ChugType))
        {
            var chugType    = group.Key;
            var best        = group.MinBy(a => a.DurationMs)!;
            var attemptCount = group.Count();

            // Rank = players with a strictly better best time + 1
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
}
