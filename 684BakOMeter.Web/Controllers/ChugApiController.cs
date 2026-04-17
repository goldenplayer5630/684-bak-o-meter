using _684BakOMeter.Web.Domain.Entities;
using _684BakOMeter.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace _684BakOMeter.Web.Controllers;

[ApiController]
[Route("api/chug")]
public class ChugApiController : ControllerBase
{
    private readonly ChugService _chugService;

    public ChugApiController(ChugService chugService)
    {
        _chugService = chugService;
    }

    /// <summary>
    /// Starts a new chug session on a specific scale.
    /// POST /api/chug/start-session { playerId, chugType, scaleNumber }
    /// </summary>
    [HttpPost("start-session")]
    public IActionResult StartSession([FromBody] StartChugRequest request)
    {
        if (!Enum.TryParse<ChugType>(request.ChugType, ignoreCase: true, out var chugType))
            return BadRequest(new { error = "Invalid chug type." });

        if (request.ScaleNumber is not (1 or 2))
            return BadRequest(new { error = "Scale number must be 1 or 2." });

        var session = _chugService.StartSession(request.PlayerId, chugType, request.ScaleNumber);

        return Ok(new
        {
            sessionId = session.SessionId,
            state = session.State.ToString(),
            scaleNumber = session.ScaleNumber,
        });
    }

    /// <summary>
    /// Returns the active session for a given scale.
    /// GET /api/chug/session/{scaleNumber}
    /// </summary>
    [HttpGet("session/{scaleNumber:int}")]
    public IActionResult GetSession(int scaleNumber)
    {
        var session = _chugService.GetSession(scaleNumber);
        if (session is null)
            return Ok(new { active = false });

        return Ok(new
        {
            active = true,
            sessionId = session.SessionId,
            state = session.State.ToString(),
            playerId = session.PlayerId,
            scaleNumber = session.ScaleNumber,
            elapsedMs = session.ElapsedMs,
            durationMs = session.DurationMs,
            currentAverage = Math.Round(session.CurrentAverage, 1),
        });
    }

    /// <summary>
    /// Cancels the active session on a given scale.
    /// POST /api/chug/cancel/{scaleNumber}
    /// </summary>
    [HttpPost("cancel/{scaleNumber:int}")]
    public IActionResult CancelSession(int scaleNumber)
    {
        _chugService.CancelSession(scaleNumber);
        return Ok(new { cancelled = true });
    }
}

public record StartChugRequest(int PlayerId, string ChugType, int ScaleNumber);
