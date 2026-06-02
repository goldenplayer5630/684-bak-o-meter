using _684BakOMeter.Web.Data.Persistence;
using _684BakOMeter.Web.Data.Repositories;
using _684BakOMeter.Web.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace _684BakOMeter.Tests.Data.Repositories;

public class ChugAttemptRepositoryPeriodTests
{
    [Fact]
    public async Task GetLeaderboardAsync_Daily_OnlyIncludesTodayAttempts()
    {
        await using var db = CreateContext();

        var now = DateTime.UtcNow;
        var yesterday = now.AddDays(-1);

        var p1 = new Player { Name = "today-fast" };
        var p2 = new Player { Name = "today-slow" };
        var p3 = new Player { Name = "yesterday" };
        db.Players.AddRange(p1, p2, p3);
        await db.SaveChangesAsync();

        db.ChugAttempts.AddRange(
            CreateAttempt(p1.Id, 150, now, ChugType.Bak),
            CreateAttempt(p2.Id, 220, now, ChugType.Bak),
            CreateAttempt(p3.Id, 90, yesterday, ChugType.Bak));
        await db.SaveChangesAsync();

        var sut = new ChugAttemptRepository(db);

        var results = (await sut.GetLeaderboardAsync(
            ChugType.Bak,
            count: 10,
            mode: ApplicationMode.Official,
            period: LeaderboardPeriod.Daily)).ToList();

        Assert.Equal(2, results.Count);
        Assert.DoesNotContain(results, r => r.Player.Name == "yesterday");
        Assert.Equal("today-fast", results[0].Player.Name);
    }

    [Fact]
    public async Task GetLeaderboardAsync_Monthly_ExcludesPreviousMonth()
    {
        await using var db = CreateContext();

        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var lastMonth = startOfMonth.AddDays(-1);

        var current = new Player { Name = "current-month" };
        var previous = new Player { Name = "previous-month" };
        db.Players.AddRange(current, previous);
        await db.SaveChangesAsync();

        db.ChugAttempts.AddRange(
            CreateAttempt(current.Id, 180, now, ChugType.Pul),
            CreateAttempt(previous.Id, 120, lastMonth, ChugType.Pul));
        await db.SaveChangesAsync();

        var sut = new ChugAttemptRepository(db);

        var results = (await sut.GetLeaderboardAsync(
            ChugType.Pul,
            count: 10,
            mode: ApplicationMode.Official,
            period: LeaderboardPeriod.Monthly)).ToList();

        Assert.Single(results);
        Assert.Equal("current-month", results[0].Player.Name);
    }

    [Fact]
    public async Task GetAttemptRankAsync_Daily_ReturnsNullWhenPlayerHasNoDailyAttempt()
    {
        await using var db = CreateContext();

        var now = DateTime.UtcNow;
        var yesterday = now.AddDays(-1);

        var p1 = new Player { Name = "today" };
        var p2 = new Player { Name = "yesterday" };
        db.Players.AddRange(p1, p2);
        await db.SaveChangesAsync();

        var todayAttempt = CreateAttempt(p1.Id, 200, now, ChugType.BakPlus);
        var oldAttempt = CreateAttempt(p2.Id, 150, yesterday, ChugType.BakPlus);

        db.ChugAttempts.AddRange(todayAttempt, oldAttempt);
        await db.SaveChangesAsync();

        var sut = new ChugAttemptRepository(db);

        var rank = await sut.GetAttemptRankAsync(
            oldAttempt.Id,
            ChugType.BakPlus,
            mode: ApplicationMode.Official,
            period: LeaderboardPeriod.Daily);

        Assert.Null(rank);
    }

    [Fact]
    public async Task GetLeaderboardPagedAsync_Daily_PaginationUsesPeriodScopedTotal()
    {
        await using var db = CreateContext();

        var now = DateTime.UtcNow;
        var yesterday = now.AddDays(-1);

        var players = Enumerable.Range(1, 4).Select(i => new Player { Name = $"today-{i}" }).ToList();
        var old = new Player { Name = "old-player" };
        db.Players.AddRange(players);
        db.Players.Add(old);
        await db.SaveChangesAsync();

        db.ChugAttempts.AddRange(
            CreateAttempt(players[0].Id, 200, now, ChugType.SpaRood),
            CreateAttempt(players[1].Id, 210, now, ChugType.SpaRood),
            CreateAttempt(players[2].Id, 220, now, ChugType.SpaRood),
            CreateAttempt(players[3].Id, 230, now, ChugType.SpaRood),
            CreateAttempt(old.Id, 100, yesterday, ChugType.SpaRood));
        await db.SaveChangesAsync();

        var sut = new ChugAttemptRepository(db);

        var (items, total) = await sut.GetLeaderboardPagedAsync(
            ChugType.SpaRood,
            page: 2,
            pageSize: 2,
            mode: ApplicationMode.Official,
            period: LeaderboardPeriod.Daily);

        Assert.Equal(4, total);
        Assert.Equal(2, items.Count());
        Assert.DoesNotContain(items, i => i.Player.Name == "old-player");
    }

    [Fact]
    public async Task GetPersonalStatsAsync_Daily_AttemptCountUsesDailyWindow()
    {
        await using var db = CreateContext();

        var now = DateTime.UtcNow;
        var yesterday = now.AddDays(-1);

        var currentPlayer = new Player { Name = "self" };
        var otherPlayer = new Player { Name = "other" };
        db.Players.AddRange(currentPlayer, otherPlayer);
        await db.SaveChangesAsync();

        db.ChugAttempts.AddRange(
            CreateAttempt(currentPlayer.Id, 250, now, ChugType.Bak),
            CreateAttempt(currentPlayer.Id, 240, now, ChugType.Bak),
            CreateAttempt(currentPlayer.Id, 150, yesterday, ChugType.Bak),
            CreateAttempt(otherPlayer.Id, 230, now, ChugType.Bak));
        await db.SaveChangesAsync();

        var sut = new ChugAttemptRepository(db);

        var stats = (await sut.GetPersonalStatsAsync(
            currentPlayer.Id,
            mode: ApplicationMode.Official,
            period: LeaderboardPeriod.Daily)).ToList();

        var bak = Assert.Single(stats.Where(s => s.ChugType == ChugType.Bak));
        Assert.Equal(2, bak.AttemptCount);
        Assert.Equal(240, bak.BestDurationMs);
        Assert.Equal(2, bak.Rank);
        Assert.Equal(2, bak.TotalAttempts);
    }

    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"chug-attempt-repo-tests-{Guid.NewGuid()}")
            .Options;

        return new AppDbContext(options);
    }

    private static ChugAttempt CreateAttempt(int playerId, int durationMs, DateTime endedAtUtc, ChugType type)
    {
        var end = DateTime.SpecifyKind(endedAtUtc, DateTimeKind.Utc);
        return new ChugAttempt
        {
            PlayerId = playerId,
            ChugType = type,
            DurationMs = durationMs,
            Mode = ApplicationMode.Official,
            IsOfficial = true,
            StartedAt = end.AddMilliseconds(-durationMs),
            EndedAt = end,
        };
    }
}
