using _684BakOMeter.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace _684BakOMeter.Web.Controllers;

[ApiController]
[Route("api/nfc")]
public class NfcApiController : ControllerBase
{
    private readonly NfcService _nfcService;
    private readonly NfcScanBridge _scanBridge;

    public NfcApiController(NfcService nfcService, NfcScanBridge scanBridge)
    {
        _nfcService = nfcService;
        _scanBridge = scanBridge;
    }

    /// <summary>
    /// Called manually or by an external tool to simulate a tag scan.
    /// POST /api/nfc/push-scan  { "uid": "AB:CD:12:34" }
    /// </summary>
    [HttpPost("push-scan")]
    public IActionResult PushScan([FromBody] NfcResolveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Uid))
            return BadRequest(new { error = "Tag UID is required." });

        _scanBridge.Enqueue(request.Uid.Trim().ToUpperInvariant());
        return Ok(new { queued = true });
    }

    /// <summary>
    /// Polled by the Vue frontend to check if a new NFC scan event is available.
    /// GET /api/nfc/poll-scan
    /// </summary>
    [HttpGet("poll-scan")]
    public IActionResult PollScan()
    {
        if (_scanBridge.TryDequeue(out var uid))
            return Ok(new { uid });

        return Ok(new { uid = (string?)null });
    }

    /// <summary>
    /// Resolves a scanned NFC tag UID.
    /// POST /api/nfc/resolve  { "uid": "AB:CD:12:34" }
    /// Returns known=true + player data, or known=false for unknown tags.
    /// </summary>
    [HttpPost("resolve")]
    public async Task<IActionResult> Resolve([FromBody] NfcResolveRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Uid))
            return BadRequest(new { error = "Tag UID is required." });

        var (tag, player, isKnown) = await _nfcService.ResolveTagAsync(request.Uid);

        if (!isKnown || player is null)
            return Ok(new { known = false, uid = request.Uid.Trim().ToUpperInvariant() });

        return Ok(new
        {
            known = true,
            playerId = player.Id,
            playerName = player.Name,
            tagId = tag!.Id,
        });
    }

    /// <summary>
    /// Links a new NFC tag to an existing user.
    /// POST /api/nfc/link  { "playerId": 1, "uid": "AB:CD:12:34" }
    /// </summary>
    [HttpPost("link")]
    public async Task<IActionResult> Link([FromBody] NfcLinkRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Uid))
            return BadRequest(new { error = "Tag UID is required." });

        var (tag, error) = await _nfcService.LinkTagToPlayerAsync(request.PlayerId, request.Uid);

        if (error is not null)
            return BadRequest(new { error });

        return Ok(new { id = tag!.Id, uid = tag.Uid, createdAt = tag.CreatedAt });
    }

    /// <summary>
    /// Removes an NFC tag from a player.
    /// DELETE /api/nfc/{id}?playerId=1
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remove(int id, [FromQuery] int playerId)
    {
        var error = await _nfcService.RemoveTagAsync(id, playerId);

        if (error is not null)
            return BadRequest(new { error });

        return Ok(new { success = true });
    }

    /// <summary>
    /// Returns all NFC tags linked to a user.
    /// GET /api/nfc/by-player/{playerId}
    /// </summary>
    [HttpGet("by-player/{playerId:int}")]
    public async Task<IActionResult> GetByPlayer(int playerId,
        [FromServices] Data.Repositories.INfcTagRepository nfcTags)
    {
        var tags = await nfcTags.GetByPlayerIdAsync(playerId);
        var result = tags.Select(t => new
        {
            id = t.Id,
            uid = t.Uid,
            label = t.Label,
            isActive = t.IsActive,
            createdAt = t.CreatedAt,
        });
        return Ok(result);
    }
}

public record NfcResolveRequest(string Uid);
public record NfcLinkRequest(int PlayerId, string Uid);
