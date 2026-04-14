using _684BakOMeter.Web.Data.Repositories;
using _684BakOMeter.Web.Domain.Enums;
using _684BakOMeter.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace _684BakOMeter.Web.Controllers;

[ApiController]
[Route("api/personal")]
public class PersonalApiController : ControllerBase
{
    private readonly IPlayerRepository _players;
    private readonly IChugAttemptRepository _attempts;

    public PersonalApiController(IPlayerRepository players, IChugAttemptRepository attempts)
    {
        _players  = players;
        _attempts = attempts;
    }

    /// <summary>
    /// Looks up a player by name and returns their best time + rank per chug type.
    /// GET /api/personal?name=vincent
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? name, [FromQuery] int? playerId)
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

        var stats = await _attempts.GetPersonalStatsAsync(player.Id);

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
        [FromQuery] int count = 10)
    {
        if (!Enum.TryParse<ChugType>(type, ignoreCase: true, out var chugType))
            return BadRequest(new { error = "Invalid chug type." });

        var player = await _players.GetByIdAsync(playerId);
        if (player is null)
            return NotFound(new { error = "Speler niet gevonden." });

        var attempts = await _attempts.GetRecentByPlayerAndTypeAsync(playerId, chugType, count);

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
