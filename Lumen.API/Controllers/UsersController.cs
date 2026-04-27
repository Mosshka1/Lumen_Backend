using System.Security.Claims;
using Lumen.API.Data;
using Lumen.API.Models.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lumen.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsersController(AppDbContext db) => _db = db;

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var user = await _db.Users
            .Include(u => u.Character)
            .FirstOrDefaultAsync(u => u.Id == CurrentUserId);

        if (user is null) return NotFound();

        return Ok(new UserDto(
            user.Id,
            user.Login,
            user.Email,
            user.DisplayName,
            user.CharacterId,
            user.Character.AssetKey,
            user.InviteCode,
            user.ConnectionId.HasValue
        ));
    }

    [HttpPut("me/character")]
    public async Task<IActionResult> UpdateCharacter([FromBody] UpdateCharacterRequest request)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        var characterExists = await _db.Characters.AnyAsync(c => c.Id == request.CharacterId);
        if (!characterExists) return BadRequest(new { error = "Character not found." });

        user.CharacterId = request.CharacterId;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("me/name")]
    public async Task<IActionResult> UpdateName([FromBody] UpdateNameRequest request)
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        user.DisplayName = request.DisplayName.Trim();
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _db.Users.FindAsync(CurrentUserId);
        if (user is null) return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public record UpdateCharacterRequest(int CharacterId);
public record UpdateNameRequest(string DisplayName);
