using System.Text.Json;
using _684BakOMeter.Web.Data.Repositories;
using _684BakOMeter.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _684BakOMeter.Web.Pages;

public class HiddenLeaderboardsModel : PageModel
{
    private readonly IChugAttemptRepository _attempts;

    public string ChugTypesJson { get; set; } = "[]";
    public string LeaderboardsJson { get; set; } = "{}";

    public HiddenLeaderboardsModel(IChugAttemptRepository attempts)
    {
        _attempts = attempts;
    }

    public async Task OnGetAsync()
    {
        // Only hidden/easter-egg chug types
        var types = ChugTypeLabels.Hidden.Select(kv => new
        {
            slug = kv.Key.ToString(),
            label = kv.Value,
        }).ToList();

        ChugTypesJson = JsonSerializer.Serialize(types);

        var leaderboards = new Dictionary<string, object>();
        foreach (var ct in ChugTypeLabels.Hidden.Keys)
        {
            var entries = await _attempts.GetLeaderboardAsync(ct, 8);
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
