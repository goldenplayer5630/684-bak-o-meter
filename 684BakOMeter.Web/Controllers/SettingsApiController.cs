using Microsoft.AspNetCore.Mvc;

namespace _684BakOMeter.Web.Controllers;

[ApiController]
[Route("api/settings")]
public class SettingsApiController : ControllerBase
{
    /// <summary>
    /// Gets volume setting. Volume is stored client-side in localStorage,
    /// but this endpoint allows sync if needed.
    /// GET /api/settings/volume?playerId=1
    /// </summary>
    [HttpGet("volume")]
    public IActionResult GetVolume([FromQuery] int playerId)
    {
        // Volume is persisted client-side in localStorage for simplicity.
        // This endpoint exists for future server-side persistence.
        return Ok(new { volume = 50 });
    }

    /// <summary>
    /// Saves volume setting.
    /// POST /api/settings/volume  { "playerId": 1, "volume": 75 }
    /// </summary>
    [HttpPost("volume")]
    public IActionResult SaveVolume([FromBody] SaveVolumeRequest request)
    {
        // Volume is persisted client-side in localStorage for simplicity.
        return Ok(new { success = true, volume = request.Volume });
    }
}

public record SaveVolumeRequest(int PlayerId, int Volume);
