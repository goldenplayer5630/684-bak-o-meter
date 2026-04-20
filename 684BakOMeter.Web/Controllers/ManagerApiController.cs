using _684BakOMeter.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace _684BakOMeter.Web.Controllers;

[ApiController]
[Route("api/manager")]
public class ManagerApiController : ControllerBase
{
    private readonly ScoreManagementService _scores;

    public ManagerApiController(ScoreManagementService scores)
    {
        _scores = scores;
    }

    [HttpGet("users-with-scores")]
    public async Task<IActionResult> GetUsersWithScores()
    {
        var users = await _scores.GetPlayersWithScoresAsync();
        return Ok(users.Select(u => new
        {
            playerId = u.PlayerId,
            playerName = u.PlayerName,
            attemptCount = u.AttemptCount,
        }));
    }

    [HttpDelete("scores/all")]
    public async Task<IActionResult> ClearAllScores()
    {
        var result = await _scores.ClearAllScoresAsync();
        return Ok(new
        {
            deleted = result.IsDeleted,
            removedCount = result.RemovedCount,
            message = result.Message,
        });
    }

    [HttpDelete("scores/player/{playerId:int}")]
    public async Task<IActionResult> ClearScoresForPlayer(int playerId)
    {
        if (playerId <= 0)
            return BadRequest(new { error = "Ongeldige speler." });

        var result = await _scores.ClearScoresForPlayerAsync(playerId);
        if (result.IsNotFound)
            return NotFound(new { error = result.Message });

        return Ok(new
        {
            deleted = result.IsDeleted,
            removedCount = result.RemovedCount,
            message = result.Message,
        });
    }
}
