namespace Lumen.API.Models.Entities;

public class DailyRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateOnly Date { get; set; }

    public ICollection<MoodLog> MoodLogs { get; set; } = [];
    public ICollection<FoodLog> FoodLogs { get; set; } = [];
    public SleepLog? SleepLog { get; set; }
    public ICollection<ChecklistItem> ChecklistItems { get; set; } = [];
}
