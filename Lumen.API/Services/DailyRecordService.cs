using Lumen.API.Data;
using Lumen.API.Models.DTOs.Records;
using Lumen.API.Models.Entities;
using Lumen.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Lumen.API.Services;

public class DailyRecordService : IDailyRecordService
{
    private readonly AppDbContext _db;

    public DailyRecordService(AppDbContext db) => _db = db;

    // ── Get or create today's record ─────────────────────────────────────────

    public async Task<DailyRecordDto> GetOrCreateTodayAsync(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await GetOrCreateForDateAsync(userId, today);
    }

    public async Task<DailyRecordDto?> GetByDateAsync(Guid userId, DateOnly date)
    {
        var record = await QueryRecord(userId, date);
        return record is null ? null : MapToDto(record);
    }

    // ── Mood ─────────────────────────────────────────────────────────────────

    public async Task<MoodLogDto> UpsertMoodAsync(Guid userId, UpsertMoodRequest request)
    {
        await ValidateRecordOwnership(userId, request.DailyRecordId);

        var existing = await _db.MoodLogs
            .FirstOrDefaultAsync(m =>
                m.DailyRecordId == request.DailyRecordId &&
                m.TimeSlot == request.TimeSlot);

        if (existing is not null)
        {
            existing.Mood      = request.Mood;
            existing.LoggedAt  = DateTime.UtcNow;
        }
        else
        {
            existing = new MoodLog
            {
                DailyRecordId = request.DailyRecordId,
                TimeSlot      = request.TimeSlot,
                Mood          = request.Mood
            };
            _db.MoodLogs.Add(existing);
        }

        await _db.SaveChangesAsync();
        return new MoodLogDto(existing.Id, existing.TimeSlot, existing.Mood);
    }

    // ── Food ─────────────────────────────────────────────────────────────────

    public async Task<FoodLogDto> UpdateFoodAsync(Guid userId, Guid foodLogId, UpdateFoodRequest request)
    {
        var foodLog = await _db.FoodLogs
            .Include(f => f.DailyRecord)
            .FirstOrDefaultAsync(f => f.Id == foodLogId)
            ?? throw new KeyNotFoundException("Food log not found.");

        if (foodLog.DailyRecord.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        foodLog.IsChecked = request.IsChecked;
        await _db.SaveChangesAsync();

        return new FoodLogDto(foodLog.Id, foodLog.MealType, foodLog.IsChecked);
    }

    // ── Sleep ─────────────────────────────────────────────────────────────────

    public async Task<SleepLogDto> UpsertSleepAsync(Guid userId, UpsertSleepRequest request)
    {
        await ValidateRecordOwnership(userId, request.DailyRecordId);

        var existing = await _db.SleepLogs
            .FirstOrDefaultAsync(s => s.DailyRecordId == request.DailyRecordId);

        if (existing is not null)
        {
            existing.HoursSlept = request.HoursSlept;
            existing.Note       = request.Note;
            existing.LoggedAt   = DateTime.UtcNow;
        }
        else
        {
            existing = new SleepLog
            {
                DailyRecordId = request.DailyRecordId,
                HoursSlept    = request.HoursSlept,
                Note          = request.Note
            };
            _db.SleepLogs.Add(existing);
        }

        await _db.SaveChangesAsync();
        return new SleepLogDto(existing.Id, existing.HoursSlept, existing.Note);
    }

    // ── Checklist ────────────────────────────────────────────────────────────

    public async Task<ChecklistItemDto> AddChecklistItemAsync(Guid userId, CreateChecklistItemRequest request)
    {
        await ValidateRecordOwnership(userId, request.DailyRecordId);

        var maxOrder = await _db.ChecklistItems
            .Where(c => c.DailyRecordId == request.DailyRecordId)
            .Select(c => (int?)c.Order)
            .MaxAsync() ?? -1;

        var item = new ChecklistItem
        {
            DailyRecordId = request.DailyRecordId,
            Text          = request.Text,
            Order         = maxOrder + 1
        };

        _db.ChecklistItems.Add(item);
        await _db.SaveChangesAsync();

        return new ChecklistItemDto(item.Id, item.Text, item.IsCompleted, item.Order);
    }

    public async Task<ChecklistItemDto> UpdateChecklistItemAsync(Guid userId, Guid itemId, UpdateChecklistItemRequest request)
    {
        var item = await _db.ChecklistItems
            .Include(c => c.DailyRecord)
            .FirstOrDefaultAsync(c => c.Id == itemId)
            ?? throw new KeyNotFoundException("Checklist item not found.");

        if (item.DailyRecord.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        if (request.Text is not null)        item.Text        = request.Text;
        if (request.IsCompleted.HasValue)    item.IsCompleted = request.IsCompleted.Value;

        await _db.SaveChangesAsync();
        return new ChecklistItemDto(item.Id, item.Text, item.IsCompleted, item.Order);
    }

    public async Task DeleteChecklistItemAsync(Guid userId, Guid itemId)
    {
        var item = await _db.ChecklistItems
            .Include(c => c.DailyRecord)
            .FirstOrDefaultAsync(c => c.Id == itemId)
            ?? throw new KeyNotFoundException("Checklist item not found.");

        if (item.DailyRecord.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        _db.ChecklistItems.Remove(item);
        await _db.SaveChangesAsync();
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<DailyRecordDto> GetOrCreateForDateAsync(Guid userId, DateOnly date)
    {
        var record = await QueryRecord(userId, date);

        if (record is null)
        {
            record = new DailyRecord { UserId = userId, Date = date };
            _db.DailyRecords.Add(record);
            await _db.SaveChangesAsync();

            // Pre-create food slots
            _db.FoodLogs.AddRange(
                new FoodLog { DailyRecordId = record.Id, MealType = MealType.Breakfast },
                new FoodLog { DailyRecordId = record.Id, MealType = MealType.Lunch },
                new FoodLog { DailyRecordId = record.Id, MealType = MealType.Dinner }
            );
            await _db.SaveChangesAsync();

            record = await QueryRecord(userId, date);
        }

        return MapToDto(record!);
    }

    private Task<DailyRecord?> QueryRecord(Guid userId, DateOnly date) =>
        _db.DailyRecords
           .Include(d => d.MoodLogs)
           .Include(d => d.FoodLogs)
           .Include(d => d.SleepLog)
           .Include(d => d.ChecklistItems.OrderBy(c => c.Order))
           .FirstOrDefaultAsync(d => d.UserId == userId && d.Date == date);

    private async Task ValidateRecordOwnership(Guid userId, Guid recordId)
    {
        var record = await _db.DailyRecords.FindAsync(recordId)
            ?? throw new KeyNotFoundException("Daily record not found.");

        if (record.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");
    }

    private static DailyRecordDto MapToDto(DailyRecord r) => new(
        r.Id,
        r.Date,
        r.MoodLogs.Select(m => new MoodLogDto(m.Id, m.TimeSlot, m.Mood)).ToList(),
        r.FoodLogs.Select(f => new FoodLogDto(f.Id, f.MealType, f.IsChecked)).ToList(),
        r.SleepLog is null ? null : new SleepLogDto(r.SleepLog.Id, r.SleepLog.HoursSlept, r.SleepLog.Note),
        r.ChecklistItems.Select(c => new ChecklistItemDto(c.Id, c.Text, c.IsCompleted, c.Order)).ToList()
    );
}
