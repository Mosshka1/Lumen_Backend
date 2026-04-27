using System.Security.Claims;
using Lumen.API.Data;
using Lumen.API.Models.DTOs.Records;
using Lumen.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lumen.API.Controllers;

[ApiController]
[Route("api/daily-records")]
[Authorize]
public class DailyRecordsController : ControllerBase
{
    private readonly IDailyRecordService _svc;
    private readonly AppDbContext _db;

    public DailyRecordsController(IDailyRecordService svc, AppDbContext db)
    {
        _svc = svc;
        _db  = db;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /api/daily-records?date=2024-12-01
    [HttpGet]
    public async Task<ActionResult<DailyRecordDto>> Get([FromQuery] DateOnly? date)
    {
        if (date is null)
            return Ok(await _svc.GetOrCreateTodayAsync(CurrentUserId));

        var record = await _svc.GetByDateAsync(CurrentUserId, date.Value);
        return record is null ? NotFound() : Ok(record);
    }

    // GET /api/daily-records/partner?date=2024-12-01
    [HttpGet("partner")]
    public async Task<ActionResult<DailyRecordDto?>> GetPartner([FromQuery] DateOnly? date)
    {
        var me = await _db.Users
            .Include(u => u.Connection)
            .ThenInclude(c => c!.Users)
            .FirstOrDefaultAsync(u => u.Id == CurrentUserId);

        if (me?.Connection is null) return Ok(null);

        var partner = me.Connection.Users.FirstOrDefault(u => u.Id != CurrentUserId);
        if (partner is null) return Ok(null);

        var targetDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var record = await _svc.GetByDateAsync(partner.Id, targetDate);
        return Ok(record);
    }

    // POST /api/daily-records  → ensures today's record exists
    [HttpPost]
    public async Task<ActionResult<DailyRecordDto>> EnsureToday()
    {
        var record = await _svc.GetOrCreateTodayAsync(CurrentUserId);
        return Ok(record);
    }
}
