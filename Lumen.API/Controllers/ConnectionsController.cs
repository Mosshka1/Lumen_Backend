using System.Security.Claims;
using Lumen.API.Models.DTOs.Connections;
using Lumen.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumen.API.Controllers;

[ApiController]
[Route("api/connections")]
[Authorize]
public class ConnectionsController : ControllerBase
{
    private readonly IConnectionService _svc;

    public ConnectionsController(IConnectionService svc) => _svc = svc;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("my-code")]
    public async Task<ActionResult<MyCodeResponse>> GetMyCode()
    {
        var result = await _svc.GetMyCodeAsync(CurrentUserId);
        return Ok(result);
    }

    [HttpPost("connect")]
    public async Task<ActionResult<PartnerDto>> Connect([FromBody] ConnectRequest request)
    {
        var result = await _svc.ConnectAsync(CurrentUserId, request.Code);
        return Ok(result);
    }

    [HttpDelete]
    public async Task<IActionResult> Disconnect()
    {
        await _svc.DisconnectAsync(CurrentUserId);
        return NoContent();
    }

    [HttpGet("partner")]
    public async Task<ActionResult<PartnerDto?>> GetPartner()
    {
        var result = await _svc.GetPartnerAsync(CurrentUserId);
        return Ok(result);
    }
}
