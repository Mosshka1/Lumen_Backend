using Lumen.API.Models.DTOs.Records;

namespace Lumen.API.Services.Interfaces;

public interface IDailyRecordService
{
    Task<DailyRecordDto> GetOrCreateTodayAsync(Guid userId);
    Task<DailyRecordDto?> GetByDateAsync(Guid userId, DateOnly date);

    // Mood
    Task<MoodLogDto> UpsertMoodAsync(Guid userId, UpsertMoodRequest request);

    // Food
    Task<FoodLogDto> UpdateFoodAsync(Guid userId, Guid foodLogId, UpdateFoodRequest request);

    // Sleep
    Task<SleepLogDto> UpsertSleepAsync(Guid userId, UpsertSleepRequest request);

    // Checklist
    Task<ChecklistItemDto> AddChecklistItemAsync(Guid userId, CreateChecklistItemRequest request);
    Task<ChecklistItemDto> UpdateChecklistItemAsync(Guid userId, Guid itemId, UpdateChecklistItemRequest request);
    Task DeleteChecklistItemAsync(Guid userId, Guid itemId);
}
