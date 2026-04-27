using System.Security.Claims;
using Lumen.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Lumen.API.Controllers;

[ApiController]
[Route("api/statistics")]
[Authorize]
public class StatisticsController : ControllerBase
{
    private readonly AppDbContext _db;
    public StatisticsController(AppDbContext db) => _db = db;

    private Guid UserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET /api/statistics?from=2024-01-01&to=2024-01-31
    [HttpGet]
    public async Task<ActionResult<StatisticsDto>> Get(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var end   = to   ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var start = from ?? end.AddDays(-6); // default: last 7 days

        var records = await _db.DailyRecords
            .Include(d => d.MoodLogs)
            .Include(d => d.FoodLogs)
            .Include(d => d.SleepLog)
            .Where(d => d.UserId == UserId &&
                        d.Date >= start &&
                        d.Date <= end)
            .OrderBy(d => d.Date)
            .ToListAsync();

        var days = records.Select(r => new DayStatDto(
            Date: r.Date,

            // Середній настрій за день (1=дуже добре, 5=дуже погано)
            AvgMood: r.MoodLogs.Any()
                ? r.MoodLogs.Average(m => (double)m.Mood)
                : null,

            // Кількість відмічених прийомів їжі
            FoodCheckedCount: r.FoodLogs.Count(f => f.IsChecked),

            // Години сну
            HoursSlept: r.SleepLog?.HoursSlept
        )).ToList();

        return Ok(new StatisticsDto(
            From:    start,
            To:      end,
            Days:    days,
            AvgMoodOverall: days.Where(d => d.AvgMood.HasValue)
                               .Select(d => d.AvgMood!.Value)
                               .DefaultIfEmpty(0)
                               .Average(),
            AvgSleepOverall: days.Where(d => d.HoursSlept.HasValue)
                                .Select(d => (double)d.HoursSlept!.Value)
                                .DefaultIfEmpty(0)
                                .Average(),
            TotalDaysTracked: days.Count(d => d.AvgMood.HasValue)
        ));
    }
}

public record DayStatDto(
    DateOnly  Date,
    double?   AvgMood,
    int       FoodCheckedCount,
    float?    HoursSlept
);

public record StatisticsDto(
    DateOnly        From,
    DateOnly        To,
    List<DayStatDto> Days,
    double          AvgMoodOverall,
    double          AvgSleepOverall,
    int             TotalDaysTracked
);