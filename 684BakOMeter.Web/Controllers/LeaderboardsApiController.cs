using _684BakOMeter.Web.Data.Repositories;
using _684BakOMeter.Web.Domain.Entities;
using _684BakOMeter.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace _684BakOMeter.Web.Controllers;

[ApiController]
[Route("api/leaderboards")]
public class LeaderboardsApiController : ControllerBase
{
    private readonly IChugAttemptRepository _attempts;

    public LeaderboardsApiController(IChugAttemptRepository attempts)
    {
        _attempts = attempts;
    }

    /// <summary>
    /// Returns the top 10 for a given chug type, optionally filtered by mode.
    /// GET /api/leaderboards?type=Bak&amp;mode=Official
    /// Omit <c>mode</c> to include all environments.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string type, [FromQuery] string? mode = "Official")
    {
        if (!Enum.TryParse<ChugType>(type, ignoreCase: true, out var chugType))
            return BadRequest(new { error = "Invalid chug type." });

        ApplicationMode? appMode = null;
        if (!string.IsNullOrEmpty(mode) && mode != "null")
        {
            if (!Enum.TryParse<ApplicationMode>(mode, ignoreCase: true, out var parsedMode))
                return BadRequest(new { error = "Invalid mode." });
            appMode = parsedMode;
        }

        var entries = await _attempts.GetLeaderboardAsync(chugType, 10, appMode);

        var result = entries.Select((e, i) => new
        {
            rank       = i + 1,
            playerName = e.Player.Name,
            mode       = e.Mode.ToString(),
            durationMs = e.DurationMs,
            duration   = $"{e.DurationMs / 1000.0:F3}s",
            date       = e.StartedAt.ToString("dd MMM HH:mm"),
        });

        return Ok(result);
    }

    /// <summary>
    /// Returns all chug types with their Dutch labels.
    /// GET /api/leaderboards/types
    /// </summary>
    [HttpGet("types")]
    public IActionResult GetTypes()
    {
        var types = ChugTypeLabels.All.Select(kv => new
        {
            slug  = kv.Key.ToString(),
            label = kv.Value,
        });
        return Ok(types);
    }

    /// <summary>
    /// Returns all known application modes with their display slugs.
    /// GET /api/leaderboards/modes
    /// </summary>
    [HttpGet("modes")]
    public IActionResult GetModes()
    {
        var modes = Enum.GetValues<ApplicationMode>().Select(m => new
        {
            slug  = m.ToString(),
            value = (int)m,
        });
        return Ok(modes);
    }

    /// <summary>
    /// Returns a paginated leaderboard for a given chug type, optionally filtered by mode.
    /// GET /api/leaderboards/paged?type=Bak&amp;page=1&amp;pageSize=10&amp;mode=Official
    /// Omit <c>mode</c> (or pass <c>null</c>) to include all environments.
    /// </summary>
    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string type,
        [FromQuery] int    page     = 1,
        [FromQuery] int    pageSize = 10,
        [FromQuery] string? mode    = "Official")
    {
        if (!Enum.TryParse<ChugType>(type, ignoreCase: true, out var chugType))
            return BadRequest(new { error = "Invalid chug type." });

        ApplicationMode? appMode = null;
        if (!string.IsNullOrEmpty(mode) && mode != "null")
        {
            if (!Enum.TryParse<ApplicationMode>(mode, ignoreCase: true, out var parsedMode))
                return BadRequest(new { error = "Invalid mode." });
            appMode = parsedMode;
        }

        page     = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var (items, total) = await _attempts.GetLeaderboardPagedAsync(chugType, page, pageSize, appMode);

        var globalOffset = (page - 1) * pageSize;
        var entries = items.Select((e, i) => new
        {
            rank       = globalOffset + i + 1,
            playerName = e.Player.Name,
            mode       = e.Mode.ToString(),
            durationMs = e.DurationMs,
            duration   = $"{e.DurationMs / 1000.0:F3}s",
            date       = e.StartedAt.ToString("dd MMM HH:mm"),
        });

        return Ok(new
        {
            page,
            pageSize,
            totalCount = total,
            totalPages = (int)Math.Ceiling(total / (double)pageSize),
            entries,
        });
    }
}
