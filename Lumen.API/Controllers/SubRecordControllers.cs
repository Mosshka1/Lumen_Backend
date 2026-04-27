using System.Security.Claims;
using Lumen.API.Models.DTOs.Records;
using Lumen.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Lumen.API.Controllers;

// ── Mood ──────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/mood-logs")]
[Authorize]
public class MoodLogsController : ControllerBase
{
    private readonly IDailyRecordService _svc;
    public MoodLogsController(IDailyRecordService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<MoodLogDto>> Upsert([FromBody] UpsertMoodRequest request)
    {
        var result = await _svc.UpsertMoodAsync(UserId, request);
        return Ok(result);
    }
}

// ── Food ──────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/food-logs")]
[Authorize]
public class FoodLogsController : ControllerBase
{
    private readonly IDailyRecordService _svc;
    public FoodLogsController(IDailyRecordService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FoodLogDto>> Update(Guid id, [FromBody] UpdateFoodRequest request)
    {
        var result = await _svc.UpdateFoodAsync(UserId, id, request);
        return Ok(result);
    }
}

// ── Sleep ─────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/sleep-logs")]
[Authorize]
public class SleepLogsController : ControllerBase
{
    private readonly IDailyRecordService _svc;
    public SleepLogsController(IDailyRecordService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<SleepLogDto>> Upsert([FromBody] UpsertSleepRequest request)
    {
        var result = await _svc.UpsertSleepAsync(UserId, request);
        return Ok(result);
    }
}

// ── Checklist ─────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/checklist")]
[Authorize]
public class ChecklistController : ControllerBase
{
    private readonly IDailyRecordService _svc;
    public ChecklistController(IDailyRecordService svc) => _svc = svc;

    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<ChecklistItemDto>> Add([FromBody] CreateChecklistItemRequest request)
    {
        var result = await _svc.AddChecklistItemAsync(UserId, request);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ChecklistItemDto>> Update(Guid id, [FromBody] UpdateChecklistItemRequest request)
    {
        var result = await _svc.UpdateChecklistItemAsync(UserId, id, request);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _svc.DeleteChecklistItemAsync(UserId, id);
        return NoContent();
    }
}
