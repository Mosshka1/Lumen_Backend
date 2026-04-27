using Lumen.API.Models.Entities;

namespace Lumen.API.Models.DTOs.Records;

// ── Daily Record ──────────────────────────────────────────────────────────────

public record DailyRecordDto(
    Guid Id,
    DateOnly Date,
    List<MoodLogDto> MoodLogs,
    List<FoodLogDto> FoodLogs,
    SleepLogDto? SleepLog,
    List<ChecklistItemDto> ChecklistItems
);

// ── Mood ──────────────────────────────────────────────────────────────────────

public record MoodLogDto(
    Guid Id,
    TimeSlot TimeSlot,
    MoodLevel Mood
);

public record UpsertMoodRequest(
    Guid DailyRecordId,
    TimeSlot TimeSlot,
    MoodLevel Mood
);

// ── Food ──────────────────────────────────────────────────────────────────────

public record FoodLogDto(
    Guid Id,
    MealType MealType,
    bool IsChecked
);

public record UpdateFoodRequest(bool IsChecked);

// ── Sleep ─────────────────────────────────────────────────────────────────────

public record SleepLogDto(
    Guid Id,
    float HoursSlept,
    string? Note
);

public record UpsertSleepRequest(
    Guid DailyRecordId,
    float HoursSlept,
    string? Note
);

// ── Checklist ─────────────────────────────────────────────────────────────────

public record ChecklistItemDto(
    Guid Id,
    string Text,
    bool IsCompleted,
    int Order
);

public record CreateChecklistItemRequest(
    Guid DailyRecordId,
    string Text
);

public record UpdateChecklistItemRequest(
    string? Text,
    bool? IsCompleted
);
