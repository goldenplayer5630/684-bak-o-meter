using System.Text.Json;
using _684BakOMeter.Web.Data.Repositories;
using _684BakOMeter.Web.Domain.Enums;
using _684BakOMeter.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _684BakOMeter.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IChugAttemptRepository _attempts;

    public string ChugTypesJson { get; set; } = "[]";
    public string LeaderboardsJson { get; set; } = "{}";

    public IndexModel(IChugAttemptRepository attempts)
    {
        _attempts = attempts;
    }

    public async Task OnGetAsync()
    {
        // Build chug types list
        var types = ChugTypeLabels.All.Select(kv => new
        {
            slug = kv.Key.ToString(),
            label = kv.Value,
        }).ToList();

        ChugTypesJson = JsonSerializer.Serialize(types);

        // Build leaderboards for all types
        var leaderboards = new Dictionary<string, object>();
        foreach (var ct in Enum.GetValues<ChugType>())
        {
            var entries = await _attempts.GetLeaderboardAsync(ct, 10);
            leaderboards[ct.ToString()] = entries.Select((e, i) => new
            {
                rank = i + 1,
                playerName = e.Player.Name,
                durationMs = e.DurationMs,
                duration = $"{e.DurationMs / 1000.0:F3}s",
            }).ToList();
        }

        LeaderboardsJson = JsonSerializer.Serialize(leaderboards);
    }
}
