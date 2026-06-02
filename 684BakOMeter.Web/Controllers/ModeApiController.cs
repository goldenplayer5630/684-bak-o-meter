using _684BakOMeter.Web.Domain.Entities;
using _684BakOMeter.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace _684BakOMeter.Web.Controllers;

[ApiController]
[Route("api/mode")]
public class ModeApiController : ControllerBase
{
    private readonly AppModeService _modeService;

    public ModeApiController(AppModeService modeService) => _modeService = modeService;

    /// <summary>
    /// Returns the current application mode.
    /// GET /api/mode
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var mode = await _modeService.GetCurrentModeAsync();
        return Ok(new { mode = mode.ToString() });
    }

    /// <summary>
    /// Sets the application mode.
    /// POST /api/mode  { "mode": "Dms" }
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Set([FromBody] SetModeRequest request)
    {
        if (!Enum.TryParse<ApplicationMode>(request.Mode, ignoreCase: true, out var mode))
            return BadRequest(new { error = "Invalid mode. Use \"Official\" or \"Dms\"." });

        await _modeService.SetModeAsync(mode);
        return Ok(new { mode = mode.ToString() });
    }
}

public record SetModeRequest(string Mode);
