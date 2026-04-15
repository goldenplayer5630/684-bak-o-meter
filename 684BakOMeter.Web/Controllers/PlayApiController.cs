using _684BakOMeter.Web.Data.Repositories;
using _684BakOMeter.Web.Domain.Entities;
using _684BakOMeter.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace _684BakOMeter.Web.Controllers;

[ApiController]
[Route("api/play")]
public class PlayApiController : ControllerBase
{
    private readonly PlayerService _playerService;
    private readonly IChugAttemptRepository _attempts;

    public PlayApiController(PlayerService playerService, IChugAttemptRepository attempts)
    {
        _playerService = playerService;
        _attempts = attempts;
    }

    /// <summary>
    /// Resolves a player by name (find-or-create).
    /// POST /api/play/resolve-player  { "name": "Vincent" }
    /// </summary>
    [HttpPost("resolve-player")]
    public async Task<IActionResult> ResolvePlayer([FromBody] ResolvePlayerRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Name is required." });

        var player = await _playerService.ResolvePlayerAsync(request.Name);
        return Ok(new { id = player.Id, name = player.Name });
    }

    /// <summary>
    /// Saves a completed chug attempt only if it is the player's personal best
    /// for that chug type. If the player already has a faster record the attempt
    /// is discarded and the existing best is returned with isNewBest = false.
    /// POST /api/play/save-attempt
    /// </summary>
    [HttpPost("save-attempt")]
    public async Task<IActionResult> SaveAttempt([FromBody] SaveAttemptRequest request)
    {
        if (!Enum.TryParse<ChugType>(request.ChugType, ignoreCase: true, out var chugType))
            return BadRequest(new { error = "Invalid chug type." });

        if (request.DurationMs <= 0)
            return BadRequest(new { error = "Duration must be positive." });

        // Check whether the player already has a better record for this type
        var existing = await _attempts.GetPersonalBestAsync(request.PlayerId, chugType);
        bool isNewBest = existing is null || request.DurationMs < existing.DurationMs;

        if (isNewBest && existing is not null)
        {
            // if this attempt is a new best, mark the old one as not a high score anymore
            existing.IsHighScore = false;
            await _attempts.UpdateAsync(existing);
        }

        var now = DateTime.UtcNow;
        var attempt = new ChugAttempt
        {
            PlayerId   = request.PlayerId,
            StartedAt  = now.AddMilliseconds(-request.DurationMs),
            EndedAt    = now,
            DurationMs = request.DurationMs,
            ChugType   = chugType,
            IsHighScore = isNewBest,
        };

        await _attempts.AddAsync(attempt);

        var rank = await _attempts.GetAttemptRankAsync(attempt.Id, chugType);

        return Ok(new
        {
            id         = attempt.Id,
            rank,
            durationMs = attempt.DurationMs,
            duration   = $"{attempt.DurationMs / 1000.0:F3}s",
            isNewBest  = isNewBest,
        });
    }
}

public record ResolvePlayerRequest(string Name);
public record SaveAttemptRequest(int PlayerId, string ChugType, int DurationMs);
