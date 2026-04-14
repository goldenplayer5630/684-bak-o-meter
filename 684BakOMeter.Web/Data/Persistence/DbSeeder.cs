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
            new Player { Name = "milan" },
            new Player { Name = "daan" },
            new Player { Name = "stijn" },
            new Player { Name = "bas" },
            new Player { Name = "jeroen" },
            new Player { Name = "thijs" },
            new Player { Name = "kevin" },
            new Player { Name = "nick" },
            new Player { Name = "martijn" },
            new Player { Name = "rob" },
        };

        db.Players.AddRange(players);
        db.SaveChanges();

        var attempts = new List<ChugAttempt>();
        var random = new Random();
        var chugTypes = Enum.GetValues<ChugType>();
        var notes = new string?[]
        {
            null,
            null,
            null,
            "cold lager",
            "foam start",
            "clean finish",
            "personal best",
            "crowd favorite",
            "fast pickup",
            "late stumble"
        };

        var totalAttempts = 500;

        for (var i = 0; i < totalAttempts; i++)
        {
            var player = players[random.Next(players.Count)];
            var duration = random.Next(1500, 7001);

            var startedAt = now
                .AddDays(-random.Next(0, 120))
                .AddMinutes(-random.Next(0, 24 * 60))
                .AddSeconds(-random.Next(0, 60));

            attempts.Add(new ChugAttempt
            {
                PlayerId = player.Id,
                StartedAt = startedAt,
                EndedAt = startedAt.AddMilliseconds(duration),
                DurationMs = duration,
                ChugType = chugTypes[random.Next(chugTypes.Length)],
                Notes = notes[random.Next(notes.Length)],
                IsHighScore = false
            });
        }

        // Mark the fastest attempt per player + chug type as the active high score.
        foreach (var group in attempts.GroupBy(a => new { a.PlayerId, a.ChugType }))
        {
            var best = group.OrderBy(a => a.DurationMs).ThenBy(a => a.StartedAt).First();
            best.IsHighScore = true;
        }

        db.ChugAttempts.AddRange(attempts);
        db.SaveChanges();
    }
}
