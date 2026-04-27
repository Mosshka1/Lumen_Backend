namespace Lumen.API.Models.Entities;

public class ChecklistItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid DailyRecordId { get; set; }
    public DailyRecord DailyRecord { get; set; } = null!;

    public string Text { get; set; } = string.Empty;
    public bool IsCompleted { get; set; } = false;
    public int Order { get; set; } = 0;
}
