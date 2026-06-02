using _684BakOMeter.Web.Data.Repositories;
using _684BakOMeter.Web.Domain.Entities;
using _684BakOMeter.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace _684BakOMeter.Web.Controllers;

[ApiController]
[Route("api/personal")]
public class PersonalApiController : ControllerBase
{
    private readonly IPlayerRepository _players;
    private readonly IChugAttemptRepository _attempts;
    private readonly AppModeService _modeService;

    public PersonalApiController(
        IPlayerRepository players,
        IChugAttemptRepository attempts,
        AppModeService modeService)
    {
        _players  = players;
        _attempts = attempts;
        _modeService = modeService;
    }

    /// <summary>
    /// Looks up a player by name and returns their best time + rank per chug type.
    /// GET /api/personal?name=vincent
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] string? name,
        [FromQuery] int? playerId,
        [FromQuery] string? mode = null)
    {
        Domain.Entities.Player? player = null;

        if (playerId.HasValue)
        {
            player = await _players.GetByIdAsync(playerId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(name))
        {
            var normalized = name.Trim().ToLowerInvariant();
            player = await _players.GetByNameAsync(normalized);
        }
        else
        {
            return BadRequest(new { error = "Name or playerId is required." });
        }

        if (player is null)
            return NotFound(new { error = "Speler niet gevonden." });

        ApplicationMode? appMode;
        if (string.IsNullOrWhiteSpace(mode))
        {
            appMode = await _modeService.GetCurrentModeAsync();
        }
        else if (mode == "null")
        {
            appMode = null;
        }
        else if (Enum.TryParse<ApplicationMode>(mode, ignoreCase: true, out var parsedMode))
        {
            appMode = parsedMode;
        }
        else
        {
            return BadRequest(new { error = "Invalid mode." });
        }

        var stats = await _attempts.GetPersonalStatsAsync(player.Id, appMode);

        var result = stats.Select(s => new
        {
            slug         = s.ChugType.ToString(),
            label        = ChugTypeLabels.GetLabel(s.ChugType),
            bestDurationMs = s.BestDurationMs,
            best         = $"{s.BestDurationMs / 1000.0:F3}s",
            rank         = s.Rank,
            totalPlayers = s.TotalAttempts,
            attemptCount = s.AttemptCount,
        });

        return Ok(new
        {
            playerId = player.Id,
            playerName = player.Name,
            mode = appMode?.ToString(),
            stats = result,
        });
    }

    /// <summary>
    /// Returns the last N attempts for a player + chug type, for graphing.
    /// GET /api/personal/history?playerId=1&amp;type=Bak&amp;count=10
    /// </summary>
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int playerId,
        [FromQuery] string type,
        [FromQuery] int count = 10,
        [FromQuery] string? mode = null)
    {
        if (!Enum.TryParse<ChugType>(type, ignoreCase: true, out var chugType))
            return BadRequest(new { error = "Invalid chug type." });

        ApplicationMode? appMode;
        if (string.IsNullOrWhiteSpace(mode))
        {
            appMode = await _modeService.GetCurrentModeAsync();
        }
        else if (mode == "null")
        {
            appMode = null;
        }
        else if (Enum.TryParse<ApplicationMode>(mode, ignoreCase: true, out var parsedMode))
        {
            appMode = parsedMode;
        }
        else
        {
            return BadRequest(new { error = "Invalid mode." });
        }

        var player = await _players.GetByIdAsync(playerId);
        if (player is null)
            return NotFound(new { error = "Speler niet gevonden." });

        var attempts = await _attempts.GetRecentByPlayerAndTypeAsync(playerId, chugType, count, appMode);

        // Return in chronological order (oldest first) for graphing
        var result = attempts.Reverse().Select(a => new
        {
            id = a.Id,
            durationMs = a.DurationMs,
            duration = $"{a.DurationMs / 1000.0:F3}s",
            date = a.StartedAt.ToString("dd MMM HH:mm"),
            isHighScore = a.IsHighScore,
        });

        return Ok(result);
    }
}
