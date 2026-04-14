using _684BakOMeter.Web.Data.Repositories;
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
    public async Task<IActionResult> Get([FromQuery] string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "Name is required." });

        var normalized = name.Trim().ToLowerInvariant();
        var player = await _players.GetByNameAsync(normalized);

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
            playerName = player.Name,
            stats = result,
        });
    }
}
