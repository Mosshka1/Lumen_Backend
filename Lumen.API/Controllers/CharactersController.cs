using Lumen.API.Data;
using Lumen.API.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lumen.API.Controllers;

[ApiController]
[Route("api/characters")]
public class CharactersController : ControllerBase
{
    private readonly AppDbContext _db;

    public CharactersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<CharacterDto>>> GetAll()
    {
        var characters = await _db.Characters
            .Select(c => new CharacterDto(c.Id, c.Name, c.AssetKey, c.Description))
            .ToListAsync();

        return Ok(characters);
    }
}

public record CharacterDto(int Id, string Name, string AssetKey, string Description);
