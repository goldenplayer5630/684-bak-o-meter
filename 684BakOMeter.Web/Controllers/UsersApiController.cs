using _684BakOMeter.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace _684BakOMeter.Web.Controllers;

[ApiController]
[Route("api/users")]
public class UsersApiController : ControllerBase
{
    private readonly NfcService _nfcService;

    public UsersApiController(NfcService nfcService)
    {
        _nfcService = nfcService;
    }

    /// <summary>
    /// Creates a new user from an unknown NFC tag scan.
    /// POST /api/users/create-from-nfc  { "name": "vincent", "tagUid": "AB:CD:12:34" }
    /// </summary>
    [HttpPost("create-from-nfc")]
    public async Task<IActionResult> CreateFromNfc([FromBody] CreateFromNfcRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { error = "Naam is verplicht." });

        if (string.IsNullOrWhiteSpace(request.TagUid))
            return BadRequest(new { error = "NFC tag UID is verplicht." });

        var (player, tag, error) = await _nfcService.CreateUserFromNfcAsync(request.Name, request.TagUid);

        if (error is not null)
            return BadRequest(new { error });

        return Ok(new
        {
            playerId = player.Id,
            playerName = player.Name,
            tagId = tag.Id,
            tagUid = tag.Uid,
        });
    }
}

public record CreateFromNfcRequest(string Name, string TagUid);
