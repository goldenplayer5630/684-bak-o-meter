using _684BakOMeter.Web.Domain.Entities;

namespace _684BakOMeter.Web.Data.Repositories;

/// <summary>Defines async CRUD + query operations for ChugAttempt.</summary>
public interface IChugAttemptRepository
{
    Task<IEnumerable<ChugAttempt>> GetAllAsync();
    Task<ChugAttempt?> GetByIdAsync(int id);
    Task AddAsync(ChugAttempt attempt);
    Task UpdateAsync(ChugAttempt attempt);
    Task DeleteAsync(int id);

    /// <summary>Returns the most recent <paramref name="count"/> attempts, newest first.</summary>
    Task<IEnumerable<ChugAttempt>> GetRecentAsync(int count);

    /// <summary>Returns all attempts made by a specific player.</summary>
    Task<IEnumerable<ChugAttempt>> GetByPlayerIdAsync(int playerId);

    /// <summary>Returns the top <paramref name="count"/> fastest attempts for a given chug type.</summary>
    Task<IEnumerable<ChugAttempt>> GetLeaderboardAsync(ChugType chugType, int count = 10);

    /// <summary>
    /// Returns a paged set of the fastest attempts for a given chug type,
    /// ordered by duration ascending. Page is 1-based.
    /// </summary>
    Task<(IEnumerable<ChugAttempt> Items, int TotalCount)> GetLeaderboardPagedAsync(
        ChugType chugType, int page, int pageSize);

    /// <summary>
    /// Returns the 1-based rank of a specific attempt within its chug type leaderboard
    /// (ordered by duration ascending). Returns null if the attempt is not found.
    /// </summary>
    Task<int?> GetAttemptRankAsync(int attemptId, ChugType chugType);

    /// <summary>
    /// Returns the best (fastest) attempt per chug type for a given player,
    /// along with their 1-based rank in each category.
    /// Only categories where the player has at least one attempt are included.
    /// </summary>
    Task<IEnumerable<PersonalStat>> GetPersonalStatsAsync(int playerId);

    /// <summary>
    /// Returns the player's fastest attempt for a given chug type, or null if they
    /// have no attempts for that type.
    /// </summary>
    Task<ChugAttempt?> GetPersonalBestAsync(int playerId, ChugType chugType);

    /// <summary>
    /// Returns the most recent <paramref name="count"/> attempts for a given player
    /// and chug type, newest first.
    /// </summary>
    Task<IEnumerable<ChugAttempt>> GetRecentByPlayerAndTypeAsync(int playerId, ChugType chugType, int count = 10);
}
