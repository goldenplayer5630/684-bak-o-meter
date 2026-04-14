using _684BakOMeter.Web.Domain.Entities;
using _684BakOMeter.Web.Domain.Enums;

namespace _684BakOMeter.Web.Data.Persistence;

/// <summary>
/// Seeds sample players and chug attempts so the app has data on first run.
/// All player names are stored normalized (lower-case, trimmed).
/// </summary>
public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Players.Any()) return;

        var now = DateTime.UtcNow;

        var players = new List<Player>
        {
            new Player { Name = "vincent" },
            new Player { Name = "lars" },
            new Player { Name = "joost" },
            new Player { Name = "sven" },
            new Player { Name = "tom" },
        };

        db.Players.AddRange(players);
        db.SaveChanges();

        var attempts = new List<ChugAttempt>
        {
            // Bak
            new() { PlayerId = players[0].Id, StartedAt = now.AddHours(-5), EndedAt = now.AddHours(-5).AddMilliseconds(3200), DurationMs = 3200, ChugType = ChugType.Bak },
            new() { PlayerId = players[1].Id, StartedAt = now.AddHours(-4), EndedAt = now.AddHours(-4).AddMilliseconds(2900), DurationMs = 2900, ChugType = ChugType.Bak },
            new() { PlayerId = players[2].Id, StartedAt = now.AddHours(-3), EndedAt = now.AddHours(-3).AddMilliseconds(3500), DurationMs = 3500, ChugType = ChugType.Bak },
            new() { PlayerId = players[3].Id, StartedAt = now.AddHours(-2), EndedAt = now.AddHours(-2).AddMilliseconds(4100), DurationMs = 4100, ChugType = ChugType.Bak },
            // Pul
            new() { PlayerId = players[0].Id, StartedAt = now.AddHours(-4), EndedAt = now.AddHours(-4).AddMilliseconds(4800), DurationMs = 4800, ChugType = ChugType.Pul },
            new() { PlayerId = players[1].Id, StartedAt = now.AddHours(-3), EndedAt = now.AddHours(-3).AddMilliseconds(4200), DurationMs = 4200, ChugType = ChugType.Pul },
            new() { PlayerId = players[4].Id, StartedAt = now.AddHours(-2), EndedAt = now.AddHours(-2).AddMilliseconds(5100), DurationMs = 5100, ChugType = ChugType.Pul },
            // BakPlus
            new() { PlayerId = players[2].Id, StartedAt = now.AddHours(-3), EndedAt = now.AddHours(-3).AddMilliseconds(2750), DurationMs = 2750, ChugType = ChugType.BakPlus },
            new() { PlayerId = players[3].Id, StartedAt = now.AddHours(-2), EndedAt = now.AddHours(-2).AddMilliseconds(3100), DurationMs = 3100, ChugType = ChugType.BakPlus },
            // IceFles
            new() { PlayerId = players[2].Id, StartedAt = now.AddMinutes(-30), EndedAt = now.AddMinutes(-30).AddMilliseconds(1800), DurationMs = 1800, ChugType = ChugType.IceFles },
            new() { PlayerId = players[0].Id, StartedAt = now.AddMinutes(-20), EndedAt = now.AddMinutes(-20).AddMilliseconds(2200), DurationMs = 2200, ChugType = ChugType.IceFles },
            // SpaRood
            new() { PlayerId = players[4].Id, StartedAt = now.AddMinutes(-15), EndedAt = now.AddMinutes(-15).AddMilliseconds(1500), DurationMs = 1500, ChugType = ChugType.SpaRood },
            new() { PlayerId = players[1].Id, StartedAt = now.AddMinutes(-10), EndedAt = now.AddMinutes(-10).AddMilliseconds(1700), DurationMs = 1700, ChugType = ChugType.SpaRood },
            // Pitcher
            new() { PlayerId = players[1].Id, StartedAt = now.AddMinutes(-8), EndedAt = now.AddMinutes(-8).AddMilliseconds(5200), DurationMs = 5200, ChugType = ChugType.Pitcher },
            new() { PlayerId = players[3].Id, StartedAt = now.AddMinutes(-5), EndedAt = now.AddMinutes(-5).AddMilliseconds(6100), DurationMs = 6100, ChugType = ChugType.Pitcher },
            // Wijn
            new() { PlayerId = players[0].Id, StartedAt = now.AddMinutes(-3), EndedAt = now.AddMinutes(-3).AddMilliseconds(3800), DurationMs = 3800, ChugType = ChugType.Wijn },
            new() { PlayerId = players[4].Id, StartedAt = now.AddMinutes(-1), EndedAt = now.AddMinutes(-1).AddMilliseconds(4500), DurationMs = 4500, ChugType = ChugType.Wijn },
        };

        db.ChugAttempts.AddRange(attempts);
        db.SaveChanges();
    }
}
