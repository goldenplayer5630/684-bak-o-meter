using _684BakOMeter.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace _684BakOMeter.Web.Controllers;

[ApiController]
[Route("api/calibration")]
public class CalibrationController : ControllerBase
{
    private readonly CalibrationService _calibrationService;

    public CalibrationController(CalibrationService calibrationService)
    {
        _calibrationService = calibrationService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var config = _calibrationService.Load();
        return Ok(config);
    }

    [HttpPost]
    public IActionResult Save([FromBody] CalibrationConfig config)
    {
        _calibrationService.Save(config);
        return Ok(new { success = true });
    }
}
